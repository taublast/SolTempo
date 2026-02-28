using AppoMobi.Maui.Gestures;
using DrawnUi.Camera;
using SolTempo.UI;
using System.Diagnostics;

namespace SolTempo.Audio
{
    /// <summary>
    /// Musical Note Detector (Tuner)
    /// Uses AMDF (Average Magnitude Difference Function) with Parabolic Interpolation for accurate pitch.
    /// AudioNotesDetector is a SkiaLayer that implements IAudioVisualizer to detect musical notes from audio input.
    /// </summary>
    public class AudioNotesDetector : SkiaLayer, IAudioVisualizer, IDisposable
    {
        private SkiaLabel _labelNote;
        private SkiaLabel _labelFrequency;
        private SkiaLabel _labelCents;

        public Action<int, char, float> NoteChangedDelegate;
        public Action<NoteSequenceEventKind, int, ReadOnlySpan<char>, ReadOnlySpan<int>> SequenceDetectedDelegate;

        public AudioNotesDetector()
        {
            UseCache = SkiaCacheType.Operations;

            // Keep analysis parameters consistent across platforms.
            // Some platforms may start at 44.1kHz and never trigger the sample-rate change path,
            // so we compute effective buffer/scan sizes up-front.
            UpdateSampleRateParams();

            _sequenceTracker = new NoteSequenceTracker(RaiseSequenceDetectedFast);

            Children = new List<SkiaControl>
            {

                new SkiaLabel()
                {
                    FontSize = 140,
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

        private readonly NoteSequenceTracker _sequenceTracker;

        private void RaiseNoteChanged(int midiNote, float frequencyHz)
        {
            int noteIndex = midiNote % 12;
            if (noteIndex < 0) noteIndex += 12;

            string noteName = NoteNames[noteIndex];
            char naturalLetter = ToNaturalLetter(noteIndex);

            // Non-alloc fast 
            NoteChangedDelegate?.Invoke(midiNote, naturalLetter, frequencyHz);

            _sequenceTracker.OnStableNote(midiNote, naturalLetter);
        }

 
        private void RaiseSequenceDetectedFast(NoteSequenceEventKind kind, int consecutiveNotes, ReadOnlySpan<char> lastLetters, ReadOnlySpan<int> lastMidiNotes)
        {
            SequenceDetectedDelegate?.Invoke(kind, consecutiveNotes, lastLetters, lastMidiNotes);
        }

        private static char ToNaturalLetter(int noteIndex)
        {
            return noteIndex switch
            {
                0 or 1 => 'C',
                2 or 3 => 'D',
                4 => 'E',
                5 or 6 => 'F',
                7 or 8 => 'G',
                9 or 10 => 'A',
                11 => 'B',
                _ => '?'
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

            float minDim = MathF.Min(width, height);
            float cyStaff = top + height * 0.72f;

            // Always Draw Staff
            float staffWidth = width * 0.75f;
            float lineSpacing = MathF.Max(2f * scale, height * 0.035f);

            // Show notes if we have a detected note (even if signal isn't perfect)
            DrawMusicalStaff(canvas, cx, cyStaff, lineSpacing, staffWidth, scale, _displayMidiNote > 0 ? _displayMidiNote : 0, 71);

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
        private string _detectedNoteLetterName = placeholder;
        private string _detectedNoteSolfegeName = placeholder;
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
        // PC (6ms scans) → ~25 slots; mobile (23ms chunks) → ~7 slots — both cover ~150ms.
        private const float VoteWindowMs = 100f;  // target history in wall-clock ms (large enough to absorb vocal vibrato)
        private const float VoteThresholdRatio = 0.30f; // fraction of window needed to confirm a note (from silence/unknown)
        private const float VoteChangeRatio    = 0.5f; // fraction of window needed to CHANGE an already-confirmed note (hysteresis)
        private const int MaxVoteWindow = 128;  // pre-alloc ceiling
        private int _voteWindowScans = 5;      // recomputed from actual scan interval
        private int _voteThreshold = 3;        // recomputed: confirm threshold
        private int _voteChangeThreshold = 4;  // recomputed: change threshold (higher — hysteresis)
        private float _avgScanIntervalSamples = 265f; // EMA of measured scan intervals
        private int[] _noteVoteBuffer = new int[MaxVoteWindow];
        private int _voteBufferHead = 0;

        // Cents smoothing — circular buffer replaces List<float> + RemoveAt(0)
        private static readonly int[] _diatonicOffsets = { 0, 0, 1, 1, 2, 3, 3, 4, 4, 5, 5, 6 };

        private const int CentsMaxHistory = 5;
        private readonly float[] _centsBuf = new float[CentsMaxHistory];
        private int _centsHead = 0;
        private int _centsCount = 0;

        // Smoothing for UI
        private float _smoothCents = 0;

        // Render State
        private int _displayMidiNote = 0;
        private float _displayFrequency = 0;
        private float _displayCents = 0;
        private Color   _displayColor   = Colors.Gray;
        private SKColor _displayColorSK = SKColors.Gray;   // cached — avoids ToSKColor() per frame
        private bool _displayHasSignal = false;
        private int _swapRequested = 0;

        // Pre-allocated colors — avoids Color.WithAlpha() allocation per frame
        private static readonly Color _colorFreqOnGray   = Colors.Gray.WithAlpha(95);
        private static readonly Color _colorFreqOffGray  = Colors.Gray.WithAlpha(50);
        private static readonly Color _colorFreqOnWhite  = Colors.White.WithAlpha(95);
        private static readonly Color _colorFreqOffWhite = Colors.White.WithAlpha(50);

        // String caching — avoids $"" allocation every Render() frame when values unchanged
        private float _lastRenderedFreq  = -1f;
        private float _lastRenderedCents = float.NaN;

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
        public bool VoiceMode { get; set; } = false;

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
            // Desired time-based buffer length.
            int desired = Math.Min((int)(_sampleRate * (BufferMs / 1000f)), BufferSize);

            // AMDF needs enough history to cover the maximum lag for low notes plus an analysis window.
            // If the user configured a very small BufferMs, clamp the internal buffer so detection
            // doesn't break (e.g. windowSize becoming 0 on 48kHz).
            int minFreq = VoiceMode ? 80 : 60;
            int maxLag = _sampleRate / minFreq;
            int targetWindow = (int)(_sampleRate * 0.020f); // ~20ms
            int required = maxLag + targetWindow + 2;

            _effectiveBufferSamples = Math.Min(Math.Max(desired, required), BufferSize);
            _scanIntervalSamples = Math.Max(64, (int)(_sampleRate * (ScanIntervalMs / 1000f)));
            _avgScanIntervalSamples = _scanIntervalSamples; // reset EMA on sample rate change
            RecalcVoteParams();
        }

        private void RecalcVoteParams()
        {
            float scanMs = _avgScanIntervalSamples * 1000f / _sampleRate;
            _voteWindowScans     = Math.Max(3, Math.Min(MaxVoteWindow, (int)Math.Ceiling(VoteWindowMs / scanMs)));
            _voteThreshold       = Math.Max(2, (int)Math.Ceiling(_voteWindowScans * VoteThresholdRatio));
            _voteChangeThreshold = Math.Max(_voteThreshold + 1, (int)Math.Ceiling(_voteWindowScans * VoteChangeRatio));
        }

        public void Reset()
        {
            Array.Clear(_sampleBuffer, 0, _sampleBuffer.Length);
            _writePos = 0;
            _samplesAddedSinceLastScan = 0;
            _sampleRate = 44100;
            UpdateSampleRateParams();

            _detectedNoteLetterName = placeholder;
            _detectedNoteSolfegeName = placeholder;
            _currentMidiNote = 0;
            _currentFrequency = 0;
            _currentCents = 0;
            HasSignal = false;

            Array.Clear(_noteVoteBuffer, 0, _noteVoteBuffer.Length);
            _voteBufferHead = 0;
            _centsHead = 0; _centsCount = 0;
            _smoothCents = 0;

            _detectedNoteLetterName = placeholder;
            _detectedNoteSolfegeName = placeholder;
            _displayMidiNote = 0;
            _displayFrequency = 0;
            _displayCents = 0;
            _displayColor = Colors.Gray;
            _displayHasSignal = false;
            _swapRequested = 0;

            _sequenceTracker.Reset();

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
                _displayMidiNote = _currentMidiNote;
                _displayHasSignal = HasSignal;

                // Freeze Hz at the last good reading — only update while actively detecting a confirmed note
                if (_displayHasSignal && _currentMidiNote > 0)
                    _displayFrequency = _currentFrequency;

                // Freeze cents when silent — decay to 0 was meaningless.
                // _smoothCents is reset to 0 when a new confirmed note starts (in DetectPitch).
                if (_displayHasSignal)
                    _smoothCents = _smoothCents * 0.5f + _currentCents * 0.5f;

                _displayCents = _smoothCents;

                // Color logic
                if (_displayHasSignal)
                {
                    float absCents = MathF.Abs(_displayCents);
                    if (absCents < 10)      { _displayColor = Colors.Lime;     _displayColorSK = SKColors.Lime; }
                    else if (absCents < 25) { _displayColor = Colors.Yellow;   _displayColorSK = SKColors.Yellow; }
                    else                    { _displayColor = Colors.Orange;   _displayColorSK = SKColors.Orange; }
                }
                else
                {
                    _displayColor = Colors.DarkGray; _displayColorSK = SKColors.DarkGray;
                    // Keep showing last note in gray - don't clear
                    //_displayNote = placeholder;
                    //_displayNoteSolf = placeholder;
                }

                // Draw Note Name
                _labelNote.TextColor = _displayColor;
                _labelNote.Text = _detectedNoteSolfegeName;

                // Draw Info
                _labelFrequency.TextColor = _displayHasSignal ? _colorFreqOnGray : _colorFreqOffGray;

                _labelFrequency.Text = $"{_displayFrequency:F1} Hz";

                // Cents Text
                _labelCents.Text = $"{_displayCents:+0;-0} cents";
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
            rms = MathF.Sqrt(rms / rmsWindow);

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
                _centsHead = 0; _centsCount = 0;

                //_sequenceTracker.Reset();
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

            // Clamp lag/window to the available buffer.
            // If the buffer is too short for the desired minFreq (large maxLag),
            // we reduce maxLag so we still have a non-zero analysis window.
            int targetWindow = Math.Min((int)(_sampleRate * 0.020f), bufLen - 2);
            if (targetWindow <= 0)
                return;

            int minWindow = Math.Min(64, targetWindow);
            int maxLagLimit = bufLen - minWindow - 1;
            if (maxLagLimit <= 0)
                return;

            if (maxLag > maxLagLimit)
                maxLag = maxLagLimit;

            if (maxLag < minLag)
                return;

            // USE NEWEST DATA — window size is time-based (~20ms) when possible.
            int windowSize = Math.Min(targetWindow, bufLen - maxLag - 1);
            if (windowSize <= 0)
                return;
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
                    diffSum += MathF.Abs(frame[idx] - frame[idx + lag]);
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
                {
                    //Debug.WriteLine($"Confidence {confidence:F2}");
                    return; // Signal too noisy — keep last confirmed note on screen, don't add bad votes
                }
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

                if (MathF.Abs(denominator) > 0.0001f)
                {
                    offset = (y1 - y3) / denominator;
                }

                float exactLag = bestLag + offset;

                float rawFrequency = _sampleRate / exactLag;

                // Convert to Note and Cents
                double noteNum = 69 + 12 * Math.Log2(rawFrequency / 440.0);
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

                // Hysteresis: confirming a note from scratch needs VoteThresholdRatio (50%),
                // but overriding an already-confirmed note needs VoteChangeRatio (70%).
                // This absorbs voice trembles near a note boundary without slowing real transitions.
                int requiredVotes = (_currentMidiNote != 0 && winnerNote != _currentMidiNote)
                    ? _voteChangeThreshold
                    : _voteThreshold;

                if (winnerCount >= requiredVotes && winnerNote > 0)
                {
                    bool noteChanged = false;
                    if (_currentMidiNote != winnerNote)
                    {
                        _currentMidiNote = winnerNote;
                        _centsHead = 0; _centsCount = 0;
                        _smoothCents = 0;
                        noteChanged = true;
                    }

                    int noteIndex = _currentMidiNote % 12;
                    if (noteIndex < 0) noteIndex += 12;
                    _detectedNoteLetterName = NoteNames[noteIndex];

                    switch (Notation)
                    {
                    case 1:
                    _detectedNoteSolfegeName = SolfegeNames[noteIndex];
                    break;

                    case 2:
                    _detectedNoteSolfegeName = SolfegeFloat[noteIndex];
                    break;

                    case 3:
                    _detectedNoteSolfegeName = SolfegeCyr[noteIndex];
                    break;

                    case 4:
                    _detectedNoteSolfegeName = SolfegeNums[noteIndex];
                    break;

                    default:
                    _detectedNoteSolfegeName = NoteNames[noteIndex];
                    break;
                    }

                    // Lock in Hz and cents only when vote is confident — keeps them
                    // in sync with the confirmed note, not jumping during transitions.
                    _currentFrequency = rawFrequency;

                    float targetFreq = 440.0f * MathF.Pow(2f, (_currentMidiNote - 69) / 12f);
                    if (targetFreq > 0)
                    {
                        float rawCents = 1200f * MathF.Log2(_currentFrequency / targetFreq);

                        _centsBuf[_centsHead] = rawCents;
                        _centsHead = (_centsHead + 1) % CentsMaxHistory;
                        if (_centsCount < CentsMaxHistory) _centsCount++;

                        float sum = 0;
                        int oldest = (_centsHead - _centsCount + CentsMaxHistory) % CentsMaxHistory;
                        for (int ci = 0; ci < _centsCount; ci++)
                            sum += _centsBuf[(oldest + ci) % CentsMaxHistory];
                        _currentCents = sum / _centsCount;
                    }

                    if (noteChanged)
                    {
                        RaiseNoteChanged(_currentMidiNote, _currentFrequency);
                    }
                }
            }
        }

        /// <summary>
        /// To make a screenshot on iPad simulator :)
        /// </summary>
        public void Demo()
        {
            // Option A: Middle C (MIDI 60) — most common reference "Do" in many contexts
            const int demoMidiNote = 60;           // C4
            const float demoFrequency = 261.63f;   // A440 tuning → C4
            const float demoCents = 12f;       // slightly sharp for realism

            // Option B: concert A as reference (uncomment if you prefer)
            // const int demoMidiNote = 69;
            // const float demoFrequency = 440f;
            // const float demoCents     = 0f;

            // Force internal state — minimal set needed to look realistic
            _currentMidiNote = demoMidiNote;
            _currentFrequency = demoFrequency;
            _currentCents = demoCents;
            _smoothCents = demoCents;

            // Make vote buffer look like we've had stable votes for a while
            int votesNeeded = Math.Max(_voteThreshold, _voteChangeThreshold);
            for (int i = 0; i < _voteWindowScans; i++)
            {
                _noteVoteBuffer[i] = demoMidiNote;
            }
            _voteBufferHead = _voteWindowScans;   // looks like we've just filled the window

            // Update visible note name / solfege
            int noteIndex = demoMidiNote % 12;
            if (noteIndex < 0) noteIndex += 12;

            _detectedNoteLetterName = NoteNames[noteIndex];

            switch (Notation)
            {
            case 1: _detectedNoteSolfegeName = SolfegeNames[noteIndex]; break;
            case 2: _detectedNoteSolfegeName = SolfegeFloat[noteIndex]; break;
            case 3: _detectedNoteSolfegeName = SolfegeCyr[noteIndex]; break;
            case 4: _detectedNoteSolfegeName = SolfegeNums[noteIndex]; break;
            default: _detectedNoteSolfegeName = NoteNames[noteIndex]; break;
            }

            // Make UI believe we have strong signal
            HasSignal = true;
            _displayHasSignal = true;
            _displayMidiNote = demoMidiNote;
            _displayFrequency = demoFrequency;
            _displayCents = demoCents;

            // Choose nice in-tune color
            _displayColor = Colors.Lime;
            _displayColorSK = SKColors.Lime;

            // Optional: push one stable note into the sequence tracker
            // (so four-note-run / seven-note-run can be tested immediately after)
            char naturalLetter = ToNaturalLetter(noteIndex);
            _sequenceTracker.OnStableNote(demoMidiNote, naturalLetter);

            // Finally — update labels (same logic as in AddSample)
            _labelNote.TextColor = _displayColor;
            _labelNote.Text = _detectedNoteSolfegeName;

            _labelFrequency.TextColor = _colorFreqOnGray;
            _labelFrequency.Text = $"{_displayFrequency:F1} Hz";

            _labelCents.Text = $"{_displayCents:+0;-0} cents";

            // Optional: force redraw / layout pass if needed in your control
            Update();           // most Skia controls react to this
                                // Invalidate();    // alternative name in some versions

            // You can now call DetectPitch() right after Demo() if you want
            // to see how the sequence tracker reacts to the next notes
            // DetectPitch();
        }

        /// <summary>
        /// Here we will not render anything anymore, 
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="viewport"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public bool Render(SKCanvas canvas, SKRect viewport, float scale)
        {
            //unimplemented
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
            var diatonicOffsets = _diatonicOffsets;
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
            _paintNeedle.Color = _displayColorSK;
            canvas.DrawOval(cx, noteY, 15 * scale, 11 * scale, _paintNeedle);

            // Stem
            _paintGauge.Color = _displayColorSK;
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
                _paintTextSmall.Color = _displayColorSK;

                canvas.DrawText("#", cx - 22 * scale, noteY + 10 * scale, _paintTextSmall);
            }
            
        }

        /// <summary>
        /// Tracks directional note runs:
        ///   FourNoteRun   — exactly 4 consecutive steps in one direction (fires once per run start)
        ///   Seven         — 7 consecutive steps in one direction
        ///   Fourteen      — two consecutive seven-runs back to back (any direction combo)
        ///
        /// "Consecutive" means each step is exactly +1 or -1 in the natural letter scale
        /// (A B C D E F G, wrapping). CDEDEDE is NOT consecutive — direction must be maintained.
        /// </summary>
        private sealed class NoteSequenceTracker
        {
            private readonly Action<NoteSequenceEventKind, int, ReadOnlySpan<char>, ReadOnlySpan<int>> _raiseSequence;

            // Rolling note buffer for span payloads sent to callbacks
            private const int Capacity = 32;
            private readonly int[] _midiNotes = new int[Capacity];
            private readonly char[] _letters = new char[Capacity];
            private int _count;

            // Run tracking
            private int _runLength;              // notes in current directional run (1 = seed)
            private int _runDir;                 // +1 ascending, -1 descending, 0 = not yet determined
            private char _lastLetter;            // last accepted natural letter
            private bool _fired4;                // whether 4-note streak fired in current chain
            private bool _fired8;                // whether 8-note streak fired in current directional run
            private bool _awaitingSuper;         // set after 8-streak fires; survives one direction reversal
            private bool _reversedAfterEight;    // set when direction reverses while _awaitingSuper is active

            public NoteSequenceTracker(Action<NoteSequenceEventKind, int, ReadOnlySpan<char>, ReadOnlySpan<int>> callback)
            {
                _raiseSequence = callback;
            }

            public void Reset()
            {
                _runLength = 0;
                _runDir = 0;
                _lastLetter = '\0';
                _fired4 = false;
                _fired8 = false;
                _awaitingSuper = false;
                _reversedAfterEight = false;
                Array.Clear(_midiNotes, 0, _midiNotes.Length);
                Array.Clear(_letters, 0, _letters.Length);
                _count = 0;
            }

            public void OnStableNote(int midiNote, char naturalLetter)
            {
                // Ignore repeated natural letter (e.g. C# and C map to same letter)
                if (_lastLetter == naturalLetter)
                    return;

                AppendCapped(midiNote, naturalLetter);

                if (_lastLetter == '\0')
                {
                    // Very first note — just seed the run
                    _lastLetter = naturalLetter;
                    _runLength = 1;
                    _runDir = 0;
                    return;
                }

                // Determine step direction in the 7-letter cycle A B C D E F G
                int stepMod = ((LetterIndex(naturalLetter) - LetterIndex(_lastLetter)) % 7 + 7) % 7;
                int dir = stepMod == 1 ? +1   // one step ascending (wraps G→A)
                        : stepMod == 6 ? -1   // one step descending (wraps A→G)
                        : 0;                  // skip — not consecutive

                _lastLetter = naturalLetter;

                if (dir == 0)
                {
                    // Non-consecutive step — break the run entirely
                    _runLength = 1;
                    _runDir = 0;
                    _fired4 = false;
                    _fired8 = false;
                    _awaitingSuper = false;
                    _reversedAfterEight = false;
                    return;
                }

                if (_runDir == 0)
                {
                    // First step: direction established, 2 notes in run
                    _runDir = dir;
                    _runLength = 2;
                }
                else if (_runDir == dir)
                {
                    // Same direction — continue
                    _runLength++;
                }
                else
                {
                    // Direction reversed
                    if (_awaitingSuper && !_reversedAfterEight)
                    {
                        // First reversal after an 8-streak — allow it toward the super achievement
                        _reversedAfterEight = true;
                    }
                    else
                    {
                        // Second reversal or no pending super — clear super state entirely
                        _awaitingSuper = false;
                        _reversedAfterEight = false;
                    }
                    _runLength = 2;
                    _runDir = dir;
                    _fired4 = false;
                    _fired8 = false;
                }

                // — Fire events —

                if (_runLength == 4 && !_fired4)
                {
                    _fired4 = true;
                    // Fire once per run, exactly at the 4-step milestone
                    _raiseSequence(
                        _runDir > 0 ? NoteSequenceEventKind.FourNoteRunAscending
                                    : NoteSequenceEventKind.FourNoteRunDescending,
                        4,
                        _letters.AsSpan(_count - 4, 4),
                        _midiNotes.AsSpan(_count - 4, 4));
                }

                // Super achievement: 8 notes one way, then 7 notes reversed (back to tonic).
                // Checked first so the reversed-8 doesn't also trigger the plain 8-streak below.
                if (_runLength == 8 && _awaitingSuper && _reversedAfterEight)
                {
                    _raiseSequence(
                        NoteSequenceEventKind.FourteenConsecutiveNotes,
                        8,
                        _letters.AsSpan(_count - 8, 8),
                        _midiNotes.AsSpan(_count - 8, 8));

                    // Reset so it can trigger again later
                    _runLength = 1;
                    _runDir = 0;
                    _fired4 = false;
                    _fired8 = false;
                    _awaitingSuper = false;
                    _reversedAfterEight = false;
                }
                else if (_runLength == 8 && !_fired8)
                {
                    _fired8 = true;
                    _awaitingSuper = true;
                    _raiseSequence(
                        NoteSequenceEventKind.SevenConsecutiveNotes,
                        8,
                        _letters.AsSpan(_count - 8, 8),
                        _midiNotes.AsSpan(_count - 8, 8));
                }
            }

            private void AppendCapped(int midiNote, char naturalLetter)
            {
                if (_count < Capacity)
                {
                    _midiNotes[_count] = midiNote;
                    _letters[_count] = naturalLetter;
                    _count++;
                    return;
                }

                // Shift left; constant size => trivial cost, zero GC
                Array.Copy(_midiNotes, 1, _midiNotes, 0, Capacity - 1);
                Array.Copy(_letters, 1, _letters, 0, Capacity - 1);
                _midiNotes[Capacity - 1] = midiNote;
                _letters[Capacity - 1] = naturalLetter;
                // _count stays at Capacity
            }

            private static int LetterIndex(char letter)
            {
                return letter switch
                {
                    'A' => 0,
                    'B' => 1,
                    'C' => 2,
                    'D' => 3,
                    'E' => 4,
                    'F' => 5,
                    'G' => 6,
                    _ => -1,
                };
            }
        }

    }
}
