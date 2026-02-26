
public class Confetti
{
    public class ConfettiPiece
    {
        public SKPoint Position;
        public SKSize Size;
        public float Rotation;
        public float RotationSpeed;
        public SKColor Color;
        public float Alpha = 1f;
        public SKPoint Velocity;
        public long BirthTimeNanos;
        public long LifetimeNanos;
    }

    private readonly List<ConfettiPiece> _pieces = new();
    private readonly Random _rnd = new();
    private readonly SKColor[] _colors;
    private SKPaint _paint;

    private long _timeBaseNanos;
    private long _lastFrameTimeNanos;
    private bool _initialized;

    // Config
    public float Gravity { get; set; } = 380f;
    public float AirResistance { get; set; } = 0.92f;
    public float MinSize { get; set; } = 6f;
    public float MaxSize { get; set; } = 18f;
    public float MinLifetimeSec { get; set; } = 2.5f;
    public float MaxLifetimeSec { get; set; } = 5.5f;
    public float MaxRotationSpeedDegPerSec { get; set; } = 360f;

    /// <summary>When false, no new pieces are spawned but existing ones fall and fade naturally.</summary>
    public bool IsSpawning { get; set; } = true;
    public bool HasPieces => _pieces.Count > 0;

    /// <summary>
    /// Resets the internal spawn timer so the next Enable causes an organic ramp-up
    /// instead of an instant burst (the burst happens when elapsed time is huge vs spawn interval).
    /// </summary>
    public void ResetSpawnTimer()
    {
        _initialized = false;
        _lastFrameTimeNanos = 0;
    }

    public Confetti()
    {
        _colors = new[]
        {
            SKColors.Red, SKColors.Blue, SKColors.Yellow, SKColors.Green,
            SKColors.Purple, SKColors.Orange, SKColors.Cyan
        };
    }

    public Confetti(SKColor[] colors)
    {
        _colors = colors ?? throw new ArgumentNullException(nameof(colors));
        if (_colors.Length == 0)
            throw new ArgumentException("At least one color is required");
    }

    public void Initialize(long currentTimeNanos)
    {
        if (_initialized) return;

        _timeBaseNanos = currentTimeNanos;
        _initialized = true;

        // Optional: you can add initial particles here for auto-start
        // we can add via Burst/AddFalling if needed
    }

    public void DrawAndUpdate(SKCanvas canvas, SKRect drawArea, float scale, long frameTimeNanos)
    {

        // 1. Lazy init
        if (!_initialized)
        {
            Initialize(frameTimeNanos);
            return;
        }

        // 2. Add falling confetti from top – but not every frame!
        //    Adjust frequency as needed (e.g. every 0.2–0.5 seconds ≈ 12–30 pieces/sec)
        if (IsSpawning && _pieces.Count < 60)   // keep roughly 40–80 pieces on screen
        {
            // How often to spawn new ones (tune this value)
            const float spawnIntervalSec = 0.08f; // ~12 new pieces per second

            // Very simple timer using frame time
            float totalSec = (frameTimeNanos - _timeBaseNanos) / 1_000_000_000f;

            // Spawn when we cross spawn intervals
            int expectedCount = (int)(totalSec / spawnIntervalSec);
            int alreadySpawned = _pieces.Count; // rough – works well enough

            if (expectedCount > alreadySpawned / 2) // prevent too aggressive spawning after pause
            {
                int toAdd = Math.Min(3, expectedCount - alreadySpawned / 2); // 1–3 per call

                for (int i = 0; i < toAdd; i++)
                {
                    AddFalling(
                        xMin: drawArea.Left + 20,
                        xMax: drawArea.Right - 20,
                        scale,
                        yStart: drawArea.Top - 100  // start slightly above top
                    );
                }
            }
        }

        // Early exit if still nothing
        if (_pieces.Count == 0) return;

        // Real frame delta — FPS-independent physics
        long frameDeltaNanos = _lastFrameTimeNanos > 0 ? frameTimeNanos - _lastFrameTimeNanos : 16_000_000L;
        float frameDeltaSec = frameDeltaNanos / 1_000_000_000f;
        if (frameDeltaSec <= 0f || frameDeltaSec > 0.25f) frameDeltaSec = 0.016f;

        _paint ??= new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };

        //canvas.Save();
        //canvas.ClipRect(drawArea);

        for (int i = _pieces.Count - 1; i >= 0; i--)
        {
            var p = _pieces[i];

            if (p.BirthTimeNanos == 0)
            {
                p.BirthTimeNanos = frameTimeNanos;
                continue;
            }

            long ageNanos = frameTimeNanos - p.BirthTimeNanos;

            if (ageNanos > p.LifetimeNanos || ageNanos < 0)
            {
                _pieces.RemoveAt(i);
                continue;
            }

            float ageSec = ageNanos / 1_000_000_000f;
            p.Alpha = 1f - ageSec / (p.LifetimeNanos / 1_000_000_000f);

            if (p.Alpha < 0.02f)
            {
                _pieces.RemoveAt(i);
                continue;
            }

            float particleDeltaSec = frameDeltaSec * scale;

            float drag = (float)Math.Pow(AirResistance, particleDeltaSec * 60f);

            p.Velocity = new SKPoint(
                p.Velocity.X * drag,
                p.Velocity.Y * drag);

            p.Velocity = new SKPoint(
                p.Velocity.X,
                p.Velocity.Y + Gravity * particleDeltaSec * scale);

            p.Position = new SKPoint(
                p.Position.X + p.Velocity.X * particleDeltaSec,
                p.Position.Y + p.Velocity.Y * particleDeltaSec);

            p.Rotation += p.RotationSpeed * particleDeltaSec;

            _paint.Color = p.Color.WithAlpha((byte)(p.Alpha * 255));

            canvas.Save();
            canvas.Translate(p.Position.X, p.Position.Y);
            canvas.RotateDegrees(p.Rotation);

            float hw = p.Size.Width * 0.5f;
            float hh = p.Size.Height * 0.5f;

            if (p.Position.X + hw >= drawArea.Left && p.Position.X - hw <= drawArea.Right &&
                p.Position.Y + hh >= drawArea.Top && p.Position.Y - hh <= drawArea.Bottom)
            {
                canvas.DrawRect(-hw, -hh, p.Size.Width, p.Size.Height, _paint);
            }

            canvas.Restore();
        }

        //canvas.Restore();

        _lastFrameTimeNanos = frameTimeNanos;
    }

    //  Public spawning methods, call at will

    public void Burst(int count, float x, float y, float scale, float initialSpeed = 320f)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (float)(_rnd.NextDouble() * Math.PI * 2);
            float speed = initialSpeed * (0.6f + (float)_rnd.NextDouble() * 0.8f);

            var piece = CreatePiece(x, y, scale);
            piece.Velocity = new SKPoint(
                (float)Math.Cos(angle) * speed,
                (float)Math.Sin(angle) * speed - 60f);

            _pieces.Add(piece);
        }
    }

    public void AddFalling(float xMin, float xMax, float scale, float yStart = -50f)
    {
        var piece = CreatePiece(
            xMin + (float)_rnd.NextDouble() * (xMax - xMin), 
            yStart, scale);

        piece.Velocity = new SKPoint(
            (float)(_rnd.NextDouble() * 100*scale - 50*scale),
            140 + (float)_rnd.NextDouble() * 180 * scale);

        _pieces.Add(piece);
    }

    private ConfettiPiece CreatePiece(float x, float y, float scale)
    {
        float size = MinSize  + (float)_rnd.NextDouble()  * (MaxSize  - MinSize);

        size *= scale;

        float aspect = 0.4f + (float)_rnd.NextDouble() * 1.8f;

        return new ConfettiPiece
        {
            Position = new SKPoint(x, y),
            Size = new SKSize(size * aspect, size / aspect),
            Rotation = (float)_rnd.NextDouble() * 360f,
            RotationSpeed = (float)(_rnd.NextDouble() * MaxRotationSpeedDegPerSec * 2 - MaxRotationSpeedDegPerSec),
            Color = _colors[_rnd.Next(_colors.Length)],
            BirthTimeNanos = 0,
            LifetimeNanos = (long)((MinLifetimeSec + (float)_rnd.NextDouble() * (MaxLifetimeSec - MinLifetimeSec)) * 1_000_000_000L)
        };
    }

    // Optional debug helper
    private ConfettiPiece CreateDebugPiece(float x, float y, float scale)
    {
        return new ConfettiPiece
        {
            Position = new SKPoint(x, y),
            Size = new SKSize(60 * scale, 60 * scale),
            Rotation = 0,
            RotationSpeed = 120,
            Color = SKColors.Magenta,
            Velocity = new SKPoint(0, 120),
            BirthTimeNanos = 0,
            LifetimeNanos = 15_000_000_000L // 15 sec
        };
    }

    public void Clear() => _pieces.Clear();

    public void Dispose()
    {
        _pieces.Clear();
        _paint?.Dispose();
        _paint = null;
    }
}