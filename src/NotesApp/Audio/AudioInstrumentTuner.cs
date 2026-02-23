using System.Diagnostics;
using AppoMobi.Maui.Gestures;
using DrawnUi.Camera;
using MusicNotes.UI;
using SkiaSharp.Views.Maui;

namespace MusicNotes.Audio
{
    /// <summary>
    /// Musical Note Detector (Tuner)
    /// Uses AMDF (Average Magnitude Difference Function) with Parabolic Interpolation for accurate pitch.
    /// </summary>
    public class AudioInstrumentTuner : SkiaLayer, IAudioVisualizer, IDisposable
    {
        private SkiaLabel _labelNote;
        private SkiaLabel _labelFrequency;
        private SkiaLabel _labelCents;

        public AudioInstrumentTuner()
        {
            UseCache = SkiaCacheType.Operations;

            Children = new List<SkiaControl>
            {

                new SkiaLabel("Hi")
                {
                    FontSize = 140,
                    IsParentIndependent = true,
                    FontAttributes = FontAttributes.Bold,
                    Margin = new (2,16),
                    MaxLines = 1,
                    LineBreakMode = LineBreakMode.CharacterWrap,
                    UseCache = SkiaCacheType.Operations,
                    HorizontalOptions = LayoutOptions.Fill,
                    HorizontalTextAlignment = DrawTextAlignment.Center,
                    VerticalOptions = LayoutOptions.Start,
                    FontFamily = AppFonts.Default,
                    TextColor = Colors.Gray,
                    Text = placeholder
                }.Assign(out _labelNote),

                new SkiaLabel("0.0 Hz")
                {
                    UseCache = SkiaCacheType.Operations,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontFamily = AppFonts.Default,
                    FontSize = 22,
                    TextColor = Colors.Gray,
                }.Assign(out _labelFrequency),

                new SkiaLabel("0 cents")
                {
                    Margin = 24,
                    UseCache = SkiaCacheType.Operations,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.End,
                    FontFamily = AppFonts.Default,
                    FontSize = 16,
                    TextColor = Colors.Gray,
                }.Assign(out _labelCents),

            };


        }

        public override ISkiaGestureListener ProcessGestures(SkiaGesturesParameters args, GestureEventProcessingInfo apply)
        {
            if (args.Type == TouchActionResult.Tapped)
            {
                this.Reset();
                return this;
            }
            return base.ProcessGestures(args, apply);
        }

        protected override void Paint(DrawingContext ctx)
        {
            base.Paint(ctx);

            var canvas = ctx.Context.Canvas;
            float scale = ctx.Scale;
            float width = (float)DrawingRect.Width;
            float height = (float)DrawingRect.Height;
            float left = (float)DrawingRect.Left;
            float top = (float)DrawingRect.Top;

            if (width <= 0 || height <= 0) return;

            if (_paintNeedle == null)
            {
                _paintGauge = new SKPaint { Color = SKColors.DarkGray, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 6 * scale };
                _paintNeedle = new SKPaint { Color = SKColors.Cyan, IsAntialias = true, Style = SKPaintStyle.Fill };
            }

            float cx = left + width / 2f;

            float minDim = Math.Min(width, height);
            float cyStaff = top + height * 0.72f;
            float cyGauge = top + height * 0.88f;
            float textLarge = Math.Max(12f * scale, minDim * 0.44f);
            float textSmall = Math.Max(10f * scale, minDim * 0.16f);
            float textInfo = Math.Max(8f * scale, minDim * 0.10f);

 
            // Always Draw Staff
            float staffWidth = width * 0.75f;
            float lineSpacing = Math.Max(2f * scale, height * 0.035f);
            // Show notes if we have a detected note (even if signal isn't perfect)
            DrawMusicalStaff(canvas, cx, cyStaff, lineSpacing, staffWidth, scale, _displayMidiNote > 0 ? _displayMidiNote : 0, 71);



            // Gauge Background
            /*

            _paintGauge.StrokeWidth = 6 * scale;
    float barWidth = width * 0.75f;
    float barY = cyGauge;
    canvas.DrawLine(cx - barWidth / 2, barY, cx + barWidth / 2, barY, _paintGauge);

    float tick = Math.Max(2f * scale, height * 0.03f);
    canvas.DrawLine(cx, barY - tick, cx, barY + tick, _paintGauge); // Center tick

    // Gauge Needle

    float offset = (_displayCents / 50.0f) * (barWidth / 2);
    offset = Math.Clamp(offset, -barWidth / 2, barWidth / 2);
    _paintNeedle.Color = _displayColor.ToSKColor();
    float dotRadius = Math.Max(2f * scale, height * 0.03f);
    canvas.DrawCircle(cx + offset, barY, dotRadius, _paintNeedle);
    */


        }

        public override void OnWillDisposeWithChildren()
        {
            base.OnWillDisposeWithChildren();

            _paintGauge?.Dispose(); 
            _paintGauge = null;
            _paintNeedle?.Dispose(); 
            _paintNeedle = null;
            _paintTextSmall?.Dispose();
            _paintTextSmall = null;
        }


        // Pre-allocated max size — 8192 supports up to 96kHz with a ~85ms buffer.
        // The ACTUAL active portion is _effectiveBufferSamples, computed from the real sample rate.
        private const int BufferSize = 8192;
        private float[] _sampleBuffer = new float[BufferSize];
        private float[] _frame = new float[BufferSize];      // Pre-allocated: avoids per-scan GC
        private float[] _amdfBuffer = new float[BufferSize]; // Pre-allocated: avoids per-scan GC
        private int _writePos = 0;
        private int _samplesAddedSinceLastScan = 0;
        private int _sampleRate = 44100;

        // Time-based parameters — recomputed on every sample-rate change so behaviour is
        // identical in wall-clock seconds on mobile (44.1kHz), PC (48kHz) and high-res PC (96kHz).
        private int _effectiveBufferSamples = 2205; // sampleRate × 50ms  (default 44.1kHz)
        private int _scanIntervalSamples = 265;  // sampleRate ×  6ms  (default 44.1kHz)

        static string placeholder = "Hi";

        // Detection State
        private string _currentNote = placeholder;
        private string _currentNoteSolf = placeholder;
        private int _currentMidiNote = 0;
        private float _currentFrequency = 0;
        private float _currentCents = 0;

        bool _hasSignal;
        public bool HasSignal
        {
        	get => _hasSignal;
        	set
        	{
        		if (_hasSignal != value)
        		{
               		_hasSignal = value;
                    Update();
                }
        	}
        }

        // Note Vote Buffer — rolling majority vote over a TIME-BASED window.
        // Target window = VoteWindowMs of real audio regardless of scan rate.
        // PC (6ms scans) → ~10 slots; mobile (23ms chunks) → ~3 slots — both cover ~60ms.
        private const float VoteWindowMs = 60f;  // target history in wall-clock ms
        private const float VoteThresholdRatio = 0.30f; // fraction of window needed to confirm
        private const int MaxVoteWindow = 32;  // pre-alloc ceiling
        private int _voteWindowScans = 5;   // recomputed from actual scan interval
        private int _voteThreshold = 3;   // recomputed from _voteWindowScans
        private float _avgScanIntervalSamples = 265f; // EMA of measured scan intervals
        private int[] _noteVoteBuffer = new int[MaxVoteWindow];
        private int _voteBufferHead = 0;
        private System.Collections.Generic.List<float> _centsBuffer = new System.Collections.Generic.List<float>();

        // Smoothing for UI
        private float _smoothCents = 0;

        // Render State
        private string _displayNote = placeholder;
        private string _displayNoteSolf = placeholder;
        private int _displayMidiNote = 0;
        private float _displayFrequency = 0;
        private float _displayCents = 0;
        private Color _displayColor = Colors.Gray;
        private bool _displayHasSignal = false;
        private int _swapRequested = 0;

        private SKPaint _paintGauge;
        private SKPaint _paintNeedle;
        private SKPaint _paintTextSmall;

        public int Notation { get; set; } = 2;

        /// <summary>
        /// Letter notation
        /// </summary>
        private static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        // Chromatic positions of the 7 natural notes: C D E F G A B
        private static readonly int[] NaturalNoteOffsets = { 0, 2, 4, 5, 7, 9, 11 };

        /// <summary>Snap an exact MIDI value to the nearest natural (non-sharp/flat) MIDI note.</summary>
        private static int SnapToNaturalNote(double exactMidi)
        {
            int octaveBase = ((int)Math.Floor(exactMidi) / 12) * 12;
            int best = 0;
            double bestDist = double.MaxValue;
            // Check current octave and neighbours to handle B→C and C→B boundaries
            for (int o = -1; o <= 1; o++)
            {
                foreach (int offset in NaturalNoteOffsets)
                {
                    int candidate = octaveBase + o * 12 + offset;
                    double dist = Math.Abs(exactMidi - candidate);
                    if (dist < bestDist) { bestDist = dist; best = candidate; }
                }
            }
            return best;
        }

        /// <summary>
        /// Fixed-do
        /// </summary>
        private static readonly string[] SolfegeNames = { "Do", "Do#", "Re", "Re#", "Mi", "Fa", "Fa#", "Sol", "Sol#", "La", "La#", "Si" };

        /// <summary>
        /// Movable-do
        /// </summary>
        private static readonly string[] SolfegeFloat = { "Do", "Do#", "Re", "Re#", "Mi", "Fa", "Fa#", "Sol", "Sol#", "La", "La#", "Ti" };

        /// <summary>
        /// Cyrillic solfeggio
        /// </summary>
        private static readonly string[] SolfegeCyr = { "До", "До#", "Ре", "Ре#", "Ми", "Фа", "Фа#", "Соль", "Со#", "Ля", "Ля#", "Си" };

        /// <summary>
        /// Numbers
        /// </summary>
        private static readonly string[] SolfegeNums = { "1", "1#", "2", "2#", "3", "4", "4#", "5", "5#", "6", "6#", "7"  };

        public bool UseGain { get; set; } = true;
        public int Skin { get; set; } = 0;

        /// <summary>
        /// When true (default): all 12 chromatic notes including sharps/flats are detected.
        /// When false: only the 7 natural notes (C D E F G A B) — a detected C# snaps to
        /// whichever of C or D is physically closer in pitch.
        /// </summary>
        public bool UseSemiNotes { get; set; } = false;

        /// <summary>
        /// Voice mode: 80–1100 Hz (bass voice to soprano high C).
        /// Instrument mode: 60–1600 Hz (bass guitar to violin/flute upper range).
        /// </summary>
        public bool VoiceMode { get; set; } = true;

        /// <summary>
        /// Audio ring buffer duration in milliseconds. Larger = more stable at low frequencies;
        /// smaller = faster response to note changes. Default 50ms. Call Reset() to apply mid-stream.
        /// </summary>
        public float BufferMs { get; set; } = 16f;

        /// <summary>
        /// How often pitch detection runs. Smaller = more responsive, more CPU. Default 6ms.
        /// Call Reset() to apply mid-stream.
        /// </summary>
        public float ScanIntervalMs { get; set; } = 6f;

        private void UpdateSampleRateParams()
        {
            _effectiveBufferSamples = Math.Min((int)(_sampleRate * (BufferMs / 1000f)), BufferSize);
            _scanIntervalSamples = Math.Max(64, (int)(_sampleRate * (ScanIntervalMs / 1000f)));
            _avgScanIntervalSamples = _scanIntervalSamples; // reset EMA on sample rate change
            RecalcVoteParams();
        }

        private void RecalcVoteParams()
        {
            float scanMs = _avgScanIntervalSamples * 1000f / _sampleRate;
            _voteWindowScans = Math.Max(3, Math.Min(MaxVoteWindow, (int)Math.Ceiling(VoteWindowMs / scanMs)));
            _voteThreshold = Math.Max(2, (int)Math.Ceiling(_voteWindowScans * VoteThresholdRatio));
        }

        public void Reset()
        {
            Array.Clear(_sampleBuffer, 0, _sampleBuffer.Length);
            _writePos = 0;
            _samplesAddedSinceLastScan = 0;
            _sampleRate = 44100;
            UpdateSampleRateParams();

            _currentNote = placeholder;
            _currentNoteSolf = placeholder;
            _currentMidiNote = 0;
            _currentFrequency = 0;
            _currentCents = 0;
            HasSignal = false;

            Array.Clear(_noteVoteBuffer, 0, _noteVoteBuffer.Length);
            _voteBufferHead = 0;
            _centsBuffer?.Clear();
            _smoothCents = 0;

            _displayNote = placeholder;
            _displayNoteSolf = placeholder;
            _displayMidiNote = 0;
            _displayFrequency = 0;
            _displayCents = 0;
            _displayColor = Colors.Gray;
            _displayHasSignal = false;
            _swapRequested = 0;

            Update();
        }

        public void AddSample(AudioSample sample)
        {
            if (sample.SampleRate > 0 && sample.SampleRate != _sampleRate)
            {
                _sampleRate = sample.SampleRate;
                UpdateSampleRateParams();
                // Clamp write position in case the effective buffer shrank
                _writePos = _writePos % _effectiveBufferSamples;
                _samplesAddedSinceLastScan = 0;
            }

            // Handle channels (use first channel only)
            int channels = sample.Channels > 0 ? sample.Channels : 1;
            int step = channels;
            int sampleCount = sample.Data.Length / 2;

            float gainMultiplier = UseGain ? 3.0f : 1.0f;

            for (int i = 0; i < sampleCount; i += step)
            {
                int byteIndex = i * 2;
                if (byteIndex + 1 < sample.Data.Length)
                {
                    short pcm = (short)(sample.Data[byteIndex] | (sample.Data[byteIndex + 1] << 8));
                    float val = (pcm / 32768f) * gainMultiplier;
                    val = Math.Clamp(val, -1.0f, 1.0f); // Prevent clipping

                    _sampleBuffer[_writePos] = val;
                    _writePos = (_writePos + 1) % _effectiveBufferSamples;
                }
            }

            _samplesAddedSinceLastScan += (sampleCount / channels);

            if (_samplesAddedSinceLastScan >= _scanIntervalSamples)
            {
                // Update EMA of actual scan interval (reflects real audio chunk size per platform)
                _avgScanIntervalSamples = _avgScanIntervalSamples * 0.75f + _samplesAddedSinceLastScan * 0.25f;
                RecalcVoteParams();
                DetectPitch(); //MIGHT call UPDATE() here if singal changed
                _samplesAddedSinceLastScan = 0;

                _displayNote = _currentNote;
                _displayNoteSolf = _currentNoteSolf;
                _displayMidiNote = _currentMidiNote;
                _displayFrequency = _currentFrequency;
                _displayHasSignal = HasSignal;

                // Smooth cents for display
                if (_displayHasSignal)
                    _smoothCents = _smoothCents * 0.5f + _currentCents * 0.5f;
                else
                    _smoothCents = _smoothCents * 0.9f;

                _displayCents = _smoothCents;

                // Color logic
                if (_displayHasSignal)
                {
                    if (Math.Abs(_displayCents) < 10) _displayColor = Colors.Lime;
                    else if (Math.Abs(_displayCents) < 25) _displayColor = Colors.Yellow;
                    else _displayColor = Colors.Orange;
                }
                else
                {
                    _displayColor = Colors.DarkGray;
                    // Keep showing last note in gray - don't clear
                    //_displayNote = placeholder;
                    //_displayNoteSolf = placeholder;
                }

                // Draw Note Name
                _labelNote.TextColor = _displayColor;
                _labelNote.Text = _displayNoteSolf;

                // Draw Info
                if (_displayHasSignal)
                {
                    _labelFrequency.TextColor = Colors.Gray.WithAlpha(95);
                    //_paintGauge.Color = SKColors.Gray.WithAlpha(95);
                }
                else
                {
                    _labelFrequency.TextColor = Colors.Gray.WithAlpha(50);
                    //_paintGauge.Color = SKColors.Gray.WithAlpha(50);
                }

                _labelFrequency.Text = $"{_displayFrequency:F1} Hz";

                // Cents Text
                _labelCents.Text = $"{_displayCents:+0;-0} cents";

                System.Threading.Interlocked.Exchange(ref _swapRequested, 1);
            }

        }

        private void DetectPitch()
        {
            int bufLen = _effectiveBufferSamples;

            // Unroll buffer for analysis (Older -> Newer) — reuse pre-allocated field
            var frame = _frame;
            int head = _writePos;
            for (int i = 0; i < bufLen; i++)
            {
                int idx = (head - bufLen + i + bufLen) % bufLen;
                frame[i] = _sampleBuffer[idx];
            }

            // 1. RMS Check (Silence detection) — time-based window (~23ms) so sensitivity
            //    is the same regardless of sample rate.
            int rmsWindow = Math.Min((int)(_sampleRate * 0.023f), bufLen / 2);
            float rms = 0;
            for (int i = bufLen - rmsWindow; i < bufLen; i++) rms += frame[i] * frame[i];
            rms = (float)Math.Sqrt(rms / rmsWindow);

            // Adjust silence threshold based on gain setting
            float silenceThreshold = UseGain ? 0.02f : 0.01f;

            if (rms < silenceThreshold) // Silence Threshold
            {
                HasSignal = false;
                // Keep last note visible (in gray) - don't clear
                //_currentNote = placeholder;
                //_currentNoteSolf = placeholder;
                //_currentMidiNote = 0;
                Array.Clear(_noteVoteBuffer, 0, _noteVoteBuffer.Length);
                _voteBufferHead = 0;
                _centsBuffer.Clear();
                return;
            }
            HasSignal = true;

            // 2. AMDF Pitch Detection
            // Voice mode : 80–1100 Hz (bass voice E2 → soprano high C6)
            // Instrument : 60–1600 Hz (bass guitar low B1 → violin/flute upper range)
            // Lower minFreq increases maxLag and shifts the analysis window back in time;
            // the voice-mode 80 Hz floor keeps maxLag reasonable for fast note detection.
            int minFreq = VoiceMode ? 80 : 60;
            int maxFreq = VoiceMode ? 1100 : 1600;
            int minLag = _sampleRate / maxFreq;
            int maxLag = _sampleRate / minFreq;

            if (maxLag >= bufLen) maxLag = bufLen - 1;

            // USE NEWEST DATA — window size is time-based (~20ms) so AMDF always analyses
            // the same duration of audio regardless of sample rate.
            int windowSize = Math.Min((int)(_sampleRate * 0.020f), bufLen - maxLag - 1);
            int analysisStart = bufLen - maxLag - windowSize;

            if (analysisStart < 0) analysisStart = 0;

            int bestLag = -1;
            float minVal = float.MaxValue;

            // Reuse pre-allocated AMDF buffer — avoids per-scan allocation in the render loop
            var amdf = _amdfBuffer;

            for (int lag = minLag; lag <= maxLag; lag++)
            {
                float diffSum = 0;

                for (int i = 0; i < windowSize; i += 2)
                {
                    int idx = analysisStart + i;
                    diffSum += Math.Abs(frame[idx] - frame[idx + lag]);
                }

                amdf[lag] = diffSum;

                if (diffSum < minVal)
                {
                    minVal = diffSum;
                    bestLag = lag;
                }
            }

            // Octave Error Correction
            // AMDF often returns double the true lag (one octave below the real pitch).
            // Check if bestLag/2 is also a strong candidate — if so, prefer it (higher frequency = less error).
            // Periodicity confidence check.
            // For a real pitched signal, AMDF has a deep dip at the fundamental period (minVal << meanVal).
            // For noise or a fading/silent signal, AMDF is nearly flat (minVal ≈ meanVal).
            // Without this check, noise always resolves to minLag (highest detectable frequency),
            // which at 44100 Hz is ~2004 Hz → MIDI 95 → note index 11 → "Si" / "B" every time.
            {
                float totalAmdf = 0f;
                for (int lag = minLag; lag <= maxLag; lag++) totalAmdf += amdf[lag];
                float meanAmdf = totalAmdf / (maxLag - minLag + 1);
                float confidence = meanAmdf > 0.0001f ? 1.0f - (minVal / meanAmdf) : 0f;
                if (confidence < 0.35f)
                    return; // Signal too noisy — keep last confirmed note on screen, don't add bad votes
            }

            if (bestLag > 0)
            {
                int halfLag = bestLag / 2;
                if (halfLag >= minLag)
                {
                    float halfVal = amdf[halfLag];
                    // Accept the half-lag if its AMDF value is within 10% of the global minimum.
                    // Tighter than 25% to avoid wrong-direction octave errors on voiced signals.
                    if (halfVal <= minVal * 1.10f)
                    {
                        bestLag = halfLag;
                        minVal = halfVal;
                    }
                }
            }

            // Parabolic Interpolation
            if (bestLag > 0 && bestLag < maxLag)
            {
                // Parabolic Interpolation
                float y1 = amdf[bestLag - 1];
                float y2 = amdf[bestLag];
                float y3 = amdf[bestLag + 1];

                float denominator = 2 * (y1 - 2 * y2 + y3);
                float offset = 0;

                if (Math.Abs(denominator) > 0.0001f)
                {
                    offset = (y1 - y3) / denominator;
                }

                float exactLag = bestLag + offset;

                _currentFrequency = _sampleRate / exactLag;

                // Convert to Note and Cents
                double noteNum = 69 + 12 * Math.Log2(_currentFrequency / 440.0);
                // UseSemiNotes=false: snap to nearest natural note (C D E F G A B) using exact
                // pitch so e.g. a slightly-flat C# → C, slightly-sharp C# → D.
                int detectedMidiNote = UseSemiNotes
                    ? (int)Math.Round(noteNum)
                    : SnapToNaturalNote(noteNum);

                // Rolling majority vote over a time-based window (_voteWindowScans slots = ~60ms).
                // _voteWindowScans adapts to actual scan interval so PC and mobile behave the same.
                _noteVoteBuffer[_voteBufferHead % _voteWindowScans] = detectedMidiNote;
                _voteBufferHead++;

                int winnerNote = 0;
                int winnerCount = 0;
                for (int vi = 0; vi < _voteWindowScans; vi++)
                {
                    int candidate = _noteVoteBuffer[vi];
                    if (candidate == 0) continue;
                    int count = 0;
                    for (int vj = 0; vj < _voteWindowScans; vj++)
                        if (_noteVoteBuffer[vj] == candidate) count++;
                    if (count > winnerCount) { winnerCount = count; winnerNote = candidate; }
                }

                if (winnerCount >= _voteThreshold && winnerNote > 0)
                {
                    if (_currentMidiNote != winnerNote)
                    {
                        _currentMidiNote = winnerNote;
                        _centsBuffer.Clear();
                    }

                    int noteIndex = _currentMidiNote % 12;
                    if (noteIndex < 0) noteIndex += 12;
                    _currentNote = NoteNames[noteIndex];

                    switch (Notation)
                    {
                    case 1:
                    _currentNoteSolf = SolfegeNames[noteIndex];
                    break;

                    case 2:
                    _currentNoteSolf = SolfegeFloat[noteIndex];
                    break;

                    case 3:
                    _currentNoteSolf = SolfegeCyr[noteIndex];
                    break;

                    case 4:
                    _currentNoteSolf = SolfegeNums[noteIndex];
                    break;

                    default:
                    _currentNoteSolf = NoteNames[noteIndex];
                    break;
                    }

                }

                // Calculate cents relative to the STABLE note (so needle shows true drift)
                float targetFreq = 440.0f * (float)Math.Pow(2, (_currentMidiNote - 69) / 12.0f);
                if (targetFreq > 0)
                {
                    float rawCents = 1200 * (float)Math.Log2(_currentFrequency / targetFreq);

                    _centsBuffer.Add(rawCents);
                    if (_centsBuffer.Count > 5) _centsBuffer.RemoveAt(0);

                    float sum = 0;
                    foreach (var c in _centsBuffer) sum += c;
                    _currentCents = sum / _centsBuffer.Count;
                }
            }
        }

        /// <summary>
        /// Here we will nor render anything anymore, 
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="viewport"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public bool Render(SKCanvas canvas, SKRect viewport, float scale)
        {
            if (viewport.Width <= 0 || viewport.Height <= 0)
                return false;

            float width = viewport.Width;
            float height = viewport.Height;
            float left = viewport.Left;
            float top = viewport.Top;

            //if (_paintNeedle == null)
            //{
            //    _paintGauge = new SKPaint { Color = SKColors.DarkGray, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 6 * scale };
            //    _paintNeedle = new SKPaint { Color = SKColors.Cyan, IsAntialias = true, Style = SKPaintStyle.Fill };
            //}

            //if (System.Threading.Interlocked.CompareExchange(ref _swapRequested, 0, 1) == 1)
            //{
               
            //}

            float cx = left + width / 2f;

            float cyStaff = top + height * 0.72f;
            float cyGauge = top + height * 0.88f;


            // Always Draw Staff
            float staffWidth = width * 0.75f;
            float lineSpacing = Math.Max(2f * scale, height * 0.035f);
            // Show notes if we have a detected note (even if signal isn't perfect)
            DrawMusicalStaff(canvas, cx, cyStaff, lineSpacing, staffWidth, scale, _displayMidiNote > 0 ? _displayMidiNote : 0, 71);

            // Draw Info
            if (_displayHasSignal)
            {
                _labelFrequency.TextColor  = Colors.White.WithAlpha(95);
                //_paintGauge.Color = SKColors.Gray.WithAlpha(95);
            }
            else
            {
                _labelFrequency.TextColor = Colors.White.WithAlpha(50);
                //_paintGauge.Color = SKColors.Gray.WithAlpha(50);
            }

            //_paintGauge.StrokeWidth = 6 * scale;

            _labelFrequency.Text = $"{_displayFrequency:F1} Hz";

            // Gauge Background
            //float barWidth = width * 0.75f;
            //float barY = cyGauge;

            //canvas.DrawLine(cx - barWidth / 2, barY, cx + barWidth / 2, barY, _paintGauge);
            //float tick = Math.Max(2f * scale, height * 0.03f);
            //canvas.DrawLine(cx, barY - tick, cx, barY + tick, _paintGauge); // Center tick

            // Gauge Needle
            //float offset = (_displayCents / 50.0f) * (barWidth / 2);
            //offset = Math.Clamp(offset, -barWidth / 2, barWidth / 2);
            //_paintNeedle.Color = _displayColor.ToSKColor();
            //float dotRadius = Math.Max(2f * scale, height * 0.03f);
            //canvas.DrawCircle(cx + offset, barY, dotRadius, _paintNeedle);

            // Cents Text
            _labelCents.Text = $"{_displayCents:+0;-0} cents";

            return false;
        }

        private void DrawMusicalStaff(SKCanvas canvas, float cx, float cy, float lineSpacing, float staffWidth, float scale, int midiNote, int referenceMidi)
        {
            float startX = cx - staffWidth / 2;
            float endX = cx + staffWidth / 2;

            _paintGauge.StrokeWidth = 2 * scale;
            _paintGauge.Color = SKColors.LightGray.WithAlpha(200);

            // Draw 5 lines (staff dynamically centered on referenceMidi)
            for (int i = -2; i <= 2; i++)
            {
                float y = cy - (i * lineSpacing);
                canvas.DrawLine(startX, y, endX, y, _paintGauge);
            }

            if (midiNote <= 0) return;

            // Map MIDI to visual steps
            int[] diatonicOffsets = { 0, 0, 1, 1, 2, 3, 3, 4, 4, 5, 5, 6 };
            int octave = (midiNote / 12) - 1;
            int noteInOctave = midiNote % 12;
            int absStep = octave * 7 + diatonicOffsets[noteInOctave];

            // Dynamic reference based on currently playing notes
            int refOctave = (referenceMidi / 12) - 1;
            int refNoteInOctave = referenceMidi % 12;
            int refStep = refOctave * 7 + diatonicOffsets[refNoteInOctave];
            int stepsFromCenter = absStep - refStep;

            // Ignore octave — only pitch class determines staff position.
            // Wrap by 7 (one diatonic octave) until within the 5-line staff bounds.
            while (stepsFromCenter < -4) stepsFromCenter += 7;
            while (stepsFromCenter >  4) stepsFromCenter -= 7;

            float noteY = cy - (stepsFromCenter * (lineSpacing / 2));

            // Ledger Lines (only draw if within reasonable range)
            _paintGauge.Color = SKColors.White;
            // Upper (limit to avoid drawing far outside viewport)
            int maxLedgerSteps = Math.Min(stepsFromCenter, 20); // Cap at 20 steps
            for (int s = 6; s <= maxLedgerSteps; s += 2)
            {
                float ly = cy - (s * (lineSpacing / 2));
                canvas.DrawLine(cx - 24 * scale, ly, cx + 24 * scale, ly, _paintGauge);
            }
            // Lower (limit to avoid drawing far outside viewport)
            int minLedgerSteps = Math.Max(stepsFromCenter, -20); // Cap at -20 steps
            for (int s = -6; s >= minLedgerSteps; s -= 2)
            {
                float ly = cy - (s * (lineSpacing / 2));
                canvas.DrawLine(cx - 24 * scale, ly, cx + 24 * scale, ly, _paintGauge);
            }

            // Note Head
            _paintNeedle.Color = _displayColor.ToSKColor();
            canvas.DrawOval(cx, noteY, 15 * scale, 11 * scale, _paintNeedle);

            // Stem
            _paintGauge.Color = _displayColor.ToSKColor();
            _paintGauge.StrokeWidth = 3 * scale;
            float stemHeight = 50 * scale;
            if (stepsFromCenter >= 0) // Stem Down (Left)
                canvas.DrawLine(cx - 13 * scale, noteY, cx - 13 * scale, noteY + stemHeight, _paintGauge);
            else // Stem Up (Right)
                canvas.DrawLine(cx + 13 * scale, noteY, cx + 13 * scale, noteY - stemHeight, _paintGauge);

            // Sharp symbol
            
            bool isSharp = noteInOctave == 1 || noteInOctave == 3 || noteInOctave == 6 || noteInOctave == 8 || noteInOctave == 10;
            if (isSharp)
            {
                if (_paintTextSmall == null)
                {
                    _paintTextSmall = new();
                    _paintTextSmall.TextAlign = SKTextAlign.Right;
                    _paintTextSmall.FakeBoldText = true;
                }

                _paintTextSmall.TextSize = 30 * scale;
                _paintTextSmall.Color = _displayColor.ToSKColor();

                canvas.DrawText("#", cx - 22 * scale, noteY + 10 * scale, _paintTextSmall);
            }
            
        }

    }
}
