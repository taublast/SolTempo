using AppoMobi.Maui.Gestures;
using DrawnUi.Camera;
using SolTempo.UI;

namespace SolTempo.Audio
{
    /// <summary>
    /// Music BPM Detector - Drawn version subclassing SkiaLayer.
    /// Uses SkiaLabels for text (caching via Operations) and paints only the
    /// energy/raw waveforms directly on the canvas inside Paint().
    /// Audio processing is identical to AudioMusicBPMDetector.
    /// </summary>
    public class AudioMusicBPM : SkiaLayer, IAudioVisualizer, IDisposable
    {
        // ---- UI children ----
        private SkiaLabel _labelTitle;
        private SkiaLabel _labelBpm;
        private SkiaLabel _labelBpmUnit;
        private SkiaLabel _labelConfidence;
        private SkiaLabel _labelNoSignal;

        // ---- Waveform paints and paths (reused every frame — no per-frame native alloc) ----
        private SKPaint _paintEnergyWave;
        private SKPaint _paintWaveform;
        private readonly SKPath _pathEnergyWave = new SKPath();
        private readonly SKPath _pathRawWave    = new SKPath();

        // ---- Audio processing state ----
        private const int BufferSize = 8192;
        private float[] _sampleBuffer = new float[BufferSize];
        private int _writePos = 0;
        private int _samplesAddedSinceLastScan = 0;
        private int _sampleRate = 44100;
        private const int ScanInterval = 1024;

        // Energy history — circular buffer (replaces List<float> + RemoveAt(0))
        private const int MaxEnergyHistory = 800;
        private readonly float[] _energyBuf = new float[MaxEnergyHistory];
        private int _energyHead = 0;   // next write position
        private int _energyCount = 0;  // valid entries [0..MaxEnergyHistory]

        private float _currentBPM = 0;

        // BPM history — circular buffer (replaces List<float> + RemoveAt(0))
        private const int MaxBpmHistory = 40;
        private readonly float[] _bpmBuf = new float[MaxBpmHistory];
        private int _bpmHead = 0;
        private int _bpmCount = 0;

        private bool _hasSignal = false;
        private float _confidence = 0;
        private float _lockedBPM = 0;

        private long _clockMs = 0;
        private float _noiseFloor = 0.005f;

        private float _displayBPM = 0;
        private float _displayConfidence = 0;
        private int _swapRequested = 0;

        // Display snapshot written by audio thread, read by render thread — no per-frame allocation
        private readonly float[] _displayEnergyBuf = new float[MaxEnergyHistory];
        private int _displayEnergyCount = 0;

        // Multi-band filter state
        private float _lowPassState = 0;
        private float _prevVal = 0;
        private int _framesSinceLastDetect = 0;

        // Raw waveform display snapshot — written by audio thread, read by render thread
        private const int RawWaveformSamples = 150;
        private readonly float[] _displayRawBuf = new float[RawWaveformSamples];

        // Pre-allocated working buffers for DetectMusicBPM — zero GC per call
        private readonly float[] _onsetsBuf   = new float[MaxEnergyHistory];
        private readonly int[]   _peaksBuf    = new int[MaxEnergyHistory / 3 + 10];
        private readonly int[]   _intervalsBuf = new int[MaxEnergyHistory / 3 + 10];
        private readonly float[] _lagCorBuf   = new float[MaxEnergyHistory / 2 + 10];

        public bool UseGain { get; set; } = true;
        public int Skin { get; set; } = 0;

        // ---- Circular buffer helpers ----

        // logicalIndex 0 = oldest entry, _energyCount-1 = newest
        private float EnergyAt(int logicalIndex) =>
            _energyBuf[(_energyHead - _energyCount + logicalIndex + MaxEnergyHistory) % MaxEnergyHistory];

        private void PushEnergy(float e)
        {
            _energyBuf[_energyHead] = e;
            _energyHead = (_energyHead + 1) % MaxEnergyHistory;
            if (_energyCount < MaxEnergyHistory) _energyCount++;
        }

        private float BpmAt(int logicalIndex) =>
            _bpmBuf[(_bpmHead - _bpmCount + logicalIndex + MaxBpmHistory) % MaxBpmHistory];

        private void PushBpm(float b)
        {
            _bpmBuf[_bpmHead] = b;
            _bpmHead = (_bpmHead + 1) % MaxBpmHistory;
            if (_bpmCount < MaxBpmHistory) _bpmCount++;
        }

        public void Demo()
        {
            // NotImplemented;
        }
        public AudioMusicBPM()
        {
            UseCache = SkiaCacheType.Operations;

            Children = new List<SkiaControl>
            {
                new SkiaLabel
                {
                    FontSize = 140,
                    //MonoForDigits = "8",
                    CharacterSpacing = 5.0,
                    IsParentIndependent = true,
                    Margin = new (2,16),
                    MaxLines = 1,
                    LineBreakMode = LineBreakMode.CharacterWrap,
                    UseCache = SkiaCacheType.Operations,
                    FontAttributes = FontAttributes.Bold,
                    FontFamily = AppFonts.Default,
                    TextColor = Colors.White,
                    HorizontalOptions = LayoutOptions.Center,
                }.Assign(out _labelBpm),

                new SkiaLabel
                {
                    Text = "BPM",
                    Margin = new(0,150,0,0),
                    FontSize = 24,
                    FontFamily = AppFonts.Default,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center,
                    UseCache = SkiaCacheType.Operations,
                }.Assign(out _labelBpmUnit),

                new SkiaLabel
                {
                    FontSize = 19,
                    Margin = new(0,180,0,0),
                    FontFamily = AppFonts.Default,
                    TextColor = Colors.LimeGreen,
                    HorizontalOptions = LayoutOptions.Center,
                    UseCache = SkiaCacheType.Operations,
                }.Assign(out _labelConfidence),

                new SkiaLabel
                {
                    Margin = new Thickness(16,40),
                    Text = "Tap to reset BPM metering",
                    FontSize = 22,
                    FontFamily = AppFonts.Default,
                    TextColor = Colors.LightGray,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.Center,
                    UseCache = SkiaCacheType.Operations,
                    IsVisible = true,
                }.Assign(out _labelNoSignal),

            };
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

            float centerX = left + width / 2f;
            float centerY = top + height / 2f;

            if (_paintEnergyWave == null)
            {
                _paintEnergyWave = new SKPaint
                {
                    Color = SKColors.LightGreen,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2 * scale,
                    IsAntialias = true,
                };
                _paintWaveform = new SKPaint
                {
                    Color = SKColors.Cyan,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2 * scale,
                    IsAntialias = true,
                };
            }

            // Energy waveform — read from pre-snapshotted buffer, no allocation
            int waveCount = _displayEnergyCount;
            if (waveCount > 1)
            {
                float waveHeight = 60 * scale;
                float waveY = centerY + 130 * scale;
                float step = width / Math.Max(1, waveCount);
                float maxEnergy = 0;
                for (int i = 0; i < waveCount; i++)
                    if (_displayEnergyBuf[i] > maxEnergy) maxEnergy = _displayEnergyBuf[i];

                if (maxEnergy > 0)
                {
                    _pathEnergyWave.Reset();
                    bool first = true;
                    for (int i = 0; i < waveCount; i++)
                    {
                        float val = _displayEnergyBuf[i] / maxEnergy;
                        float x = left + i * step;
                        float y = waveY - val * waveHeight;
                        if (first) { _pathEnergyWave.MoveTo(x, y); first = false; }
                        else _pathEnergyWave.LineTo(x, y);
                    }
                    canvas.DrawPath(_pathEnergyWave, _paintEnergyWave);
                }
            }

            // Raw waveform at bottom — lock-free, reads from double-buffered snapshot
            {
                float waveHeight = 40 * scale;
                float waveY = top + height - 70 * scale;
                float step = width / RawWaveformSamples;

                _pathRawWave.Reset();
                bool first = true;
                for (int i = 0; i < RawWaveformSamples; i++)
                {
                    float val = _displayRawBuf[i];
                    float x = left + i * step;
                    float y = waveY + val * waveHeight * 0.5f;
                    if (first) { _pathRawWave.MoveTo(x, y); first = false; }
                    else _pathRawWave.LineTo(x, y);
                }
                canvas.DrawPath(_pathRawWave, _paintWaveform);
            }
        }

        public override void OnWillDisposeWithChildren()
        {
            base.OnWillDisposeWithChildren();
            _paintEnergyWave?.Dispose();
            _paintEnergyWave = null;
            _paintWaveform?.Dispose();
            _paintWaveform = null;
            _pathEnergyWave.Dispose();
            _pathRawWave.Dispose();
        }

        public override ISkiaGestureListener ProcessGestures(SkiaGesturesParameters args, GestureEventProcessingInfo apply)
        {
            if (args.Type == TouchActionResult.Tapped)
            {
                Reset();
                return this;
            }
            return base.ProcessGestures(args, apply);
        }

        // ---- IAudioVisualizer ----

        public void Reset()
        {
            lock (_sampleBuffer)
            {
                Array.Clear(_sampleBuffer, 0, _sampleBuffer.Length);
                _writePos = 0;
                _samplesAddedSinceLastScan = 0;
                _sampleRate = 44100;

                _energyHead = 0;
                _energyCount = 0;
                _bpmHead = 0;
                _bpmCount = 0;
                _currentBPM = 0;
                _lockedBPM = 0;
                _confidence = 0;
                _hasSignal = false;

                _clockMs = 0;
                _noiseFloor = 0.005f;
                _lowPassState = 0;
                _prevVal = 0;
                _framesSinceLastDetect = 0;

                _displayBPM = 0;
                _displayConfidence = 0;
                _displayEnergyCount = 0;
                _swapRequested = 0;

                _labelBpm.IsVisible = false;
                _labelBpmUnit.IsVisible = false;
                _labelNoSignal.IsVisible = true;

                Update();
            }
        }

        private long AdvanceClock(int frames)
        {
            if (frames > 0 && _sampleRate > 0)
                _clockMs += (long)Math.Round(frames * 1000.0 / _sampleRate);
            return _clockMs;
        }

        private static float ReadMonoSample(AudioSample sample, int frameIndex)
        {
            int channels = sample.Channels > 0 ? sample.Channels : 1;
            int bytesPerSample = sample.BytesPerSample;

            int offset = (frameIndex * channels) * bytesPerSample;
            if (offset < 0 || offset >= sample.Data.Length)
                return 0f;

            if (bytesPerSample == 2)
            {
                if (offset + 1 >= sample.Data.Length)
                    return 0f;
                short pcm = (short)(sample.Data[offset] | (sample.Data[offset + 1] << 8));
                return pcm / 32768f;
            }

            if (bytesPerSample == 4 && sample.BitDepth == AudioBitDepth.Float32Bit)
            {
                if (offset + 3 >= sample.Data.Length)
                    return 0f;
                return BitConverter.ToSingle(sample.Data, offset);
            }

            return 0f;
        }

        public void AddSample(AudioSample sample)
        {
            lock (_sampleBuffer)
            {
                if (sample.SampleRate > 0)
                    _sampleRate = sample.SampleRate;

                int frames = sample.SampleCount;
                _ = AdvanceClock(frames);

                float gainMultiplier = UseGain ? 4.0f : 1.0f;

                for (int frame = 0; frame < frames; frame++)
                {
                    float val = ReadMonoSample(sample, frame) * gainMultiplier;
                    val = Math.Clamp(val, -1.0f, 1.0f);

                    _sampleBuffer[_writePos] = val;
                    _writePos = (_writePos + 1) % BufferSize;

                    _samplesAddedSinceLastScan++;

                    if (_samplesAddedSinceLastScan >= ScanInterval)
                    {
                        ComputeEnergyFrame();

                        if (_framesSinceLastDetect++ >= 4)
                        {
                            DetectMusicBPM();
                            _framesSinceLastDetect = 0;
                            UpdateDisplayLabels();
                            Update();
                        }

                        _samplesAddedSinceLastScan = 0;
                    }
                }
            }
        }

        private void UpdateDisplayLabels()
        {
            // Snapshot energy history into flat display buffer (audio thread, inside lock)
            // Paint() reads this without a lock — avoids per-frame allocation
            int count = _energyCount;
            int oldest = (_energyHead - count + MaxEnergyHistory) % MaxEnergyHistory;
            for (int i = 0; i < count; i++)
                _displayEnergyBuf[i] = _energyBuf[(oldest + i) % MaxEnergyHistory];
            _displayEnergyCount = count;

            if (_displayBPM > 0)
            {
                _labelNoSignal.IsVisible = false;
                _labelBpm.IsVisible = true;
                _labelBpmUnit.IsVisible = true;

                if (_displayConfidence > 80)
                {
                    _labelBpm.Text = $"{_displayBPM:F0}";
                }

                _labelBpm.TextColor = _displayConfidence > 99 && _hasSignal ? Colors.White : Colors.Gray;

                if (_displayConfidence > 20)
                {
                    Color confidenceColor = _displayConfidence > 70 ? Colors.LimeGreen :
                                           _displayConfidence > 40 ? Colors.Yellow : Colors.Orange;
                    _labelConfidence.TextColor = confidenceColor;
                    _labelConfidence.Text = $"Confidence: {_displayConfidence:F0}%";
                }
                else
                {
                    _labelConfidence.TextColor = Colors.Orange;
                    _labelConfidence.Text = "Thinking..";
                }
            }
            else
            {
                _labelNoSignal.IsVisible = true;
                _labelBpm.IsVisible = false;
                _labelBpmUnit.IsVisible = false;

                _labelConfidence.TextColor = Colors.Orange;
                _labelConfidence.Text = "Listening..";
            }
        }

        private void ComputeEnergyFrame()
        {
            float energyLow = 0f;
            float energyMidHigh = 0f;

            int windowSize = ScanInterval;
            float alphaLow = 0.05f;  // 0.03–0.08 range is fine; lower = smoother bass energy

            for (int i = 0; i < windowSize; i++)
            {
                int idx = (_writePos - windowSize + i + BufferSize) % BufferSize;
                float val = _sampleBuffer[idx];

                // Low-frequency emphasis (bass/kick body)
                _lowPassState += alphaLow * (val - _lowPassState);
                energyLow += _lowPassState * _lowPassState;

                // High-frequency content (snare/hi-hat/perc transients)
                float diff = val - _prevVal;
                _prevVal = val;
                energyMidHigh += diff * diff;
            }

            energyLow = (float)Math.Sqrt(energyLow / windowSize);
            energyMidHigh = (float)Math.Sqrt(energyMidHigh / windowSize);

            // Slight boost to percussive energy — helps mid-tempo grooves
            float energy = energyLow + energyMidHigh * 1.2f;

            // Noise floor tracking (slow adaptation)
            if (_noiseFloor < 0.0001f) _noiseFloor = 0.0001f;
            _noiseFloor = _noiseFloor * 0.999f + energy * 0.001f;

            float silenceThreshold = Math.Max(UseGain ? 0.0006f : 0.00025f, _noiseFloor * 0.85f);

            bool isSilence = energy < silenceThreshold;

            if (isSilence)
            {
                energy = 0f;
                _hasSignal = false;

                // Clear history after ~10–15 seconds of real silence to kill old wrong tempos
                if (_energyCount > 40)
                {
                    float limit = silenceThreshold * 1.4f;
                    bool recentAllSilent = true;
                    int checkStart = Math.Max(0, _energyCount - 30);
                    for (int i = checkStart; i < _energyCount; i++)
                    {
                        if (EnergyAt(i) > limit) { recentAllSilent = false; break; }
                    }
                    if (recentAllSilent)
                    {
                        _bpmHead = 0; _bpmCount = 0;
                        _currentBPM = 0f;
                        _lockedBPM = 0f;
                        _confidence = 0f;
                    }
                }
            }
            else
            {
                _hasSignal = true;
            }

            PushEnergy(energy);

            // Snapshot raw waveform for Paint() — eliminates lock contention on render thread
            for (int i = 0; i < RawWaveformSamples; i++)
            {
                int idx = (_writePos - RawWaveformSamples + i + BufferSize) % BufferSize;
                _displayRawBuf[i] = _sampleBuffer[idx];
            }
        }

        private void DetectMusicBPM()
        {
            if (_energyCount < 160) return;           // increased from 130 — need more context

            // ────────────────────────────────────────────────
            // Onset strength calculation — improved noise rejection
            // ────────────────────────────────────────────────
            int onsetsCount = 0;
            float maxOnset = 0f;

            const int avgWindow = 7;  // slightly larger → more stable local average

            for (int i = avgWindow; i < _energyCount; i++)
            {
                float sumPrev = 0f;
                for (int w = 1; w <= avgWindow; w++)
                    sumPrev += EnergyAt(i - w);

                float localAvg = sumPrev / avgWindow;

                float diff = EnergyAt(i) - localAvg;

                // Stricter gate — prevents noise from creating fake onsets
                if (diff < 0.004f) diff = 0f;                   // was 0.002f — raise this

                float val = Math.Max(0f, diff);

                // Cheap logarithmic compression — crushes small noise, preserves strong onsets
                val = (float)Math.Max(0f, Math.Log(1f + 35f * val) - 0.10f);

                _onsetsBuf[onsetsCount++] = val;
                if (val > maxOnset) maxOnset = val;
            }

            // ────────────────────────────────────────────────
            // Sparse peak detection — stricter to avoid false fast tempos
            // ────────────────────────────────────────────────
            int sparseLag = 0;
            bool hasPeaks = maxOnset > 0.012f;  // slightly higher than before

            int peaksCount = 0;

            if (hasPeaks)
            {
                for (int i = 3; i < onsetsCount - 3; i++)  // wider neighborhood
                {
                    if (_onsetsBuf[i] > maxOnset * 0.50f          // was 0.35 — much stricter
                        && _onsetsBuf[i] > 0.09f                  // absolute floor — kills noise peaks
                        && _onsetsBuf[i] >= _onsetsBuf[i - 1]
                        && _onsetsBuf[i] >= _onsetsBuf[i - 2]
                        && _onsetsBuf[i] >= _onsetsBuf[i - 3]
                        && _onsetsBuf[i] > _onsetsBuf[i + 1]
                        && _onsetsBuf[i] > _onsetsBuf[i + 2]
                        && _onsetsBuf[i] > _onsetsBuf[i + 3])
                    {
                        _peaksBuf[peaksCount++] = i;
                        i += 4;  // skip more aggressively to avoid double-counting same event
                    }
                }

                if (peaksCount >= 4)  // need more evidence
                {
                    int intervalsCount = 0;
                    for (int k = 0; k < peaksCount - 1; k++)
                    {
                        int delta = _peaksBuf[k + 1] - _peaksBuf[k];
                        if (delta > 6) _intervalsBuf[intervalsCount++] = delta;  // was 8 — slightly lower
                    }

                    if (intervalsCount > 0)
                    {
                        // Find mode (most frequent value) without GroupBy/LINQ allocation
                        int bestVal = 0, bestCount = 0;
                        for (int a = 0; a < intervalsCount; a++)
                        {
                            int cnt = 0, v = _intervalsBuf[a];
                            for (int b = 0; b < intervalsCount; b++)
                                if (_intervalsBuf[b] == v) cnt++;
                            if (cnt > bestCount) { bestCount = cnt; bestVal = v; }
                        }
                        if (bestCount >= 2) sparseLag = bestVal;
                    }
                }
            }

            // ────────────────────────────────────────────────
            // Autocorrelation setup — realistic music range
            // ────────────────────────────────────────────────
            const int minBPM = 40;
            const int maxBPM = 260;

            float timePerFrame = (float)ScanInterval / _sampleRate;

            int minLag = Math.Max(10, (int)(60f / maxBPM / timePerFrame));   // hard floor ~ lag 10 ≈ 260 BPM
            int maxLag = (int)(60f / minBPM / timePerFrame);
            maxLag = Math.Min(_energyCount / 2, maxLag);
            maxLag = Math.Min(maxLag, _lagCorBuf.Length - 1);

            if (minLag >= maxLag) return;

            // Clear only the range we'll use — no Dictionary allocation
            Array.Clear(_lagCorBuf, minLag, maxLag - minLag + 1);

            float maxCorrelation = 0f;
            int bestLag = 0;

            for (int lag = minLag; lag <= maxLag; lag++)
            {
                float correlation = 0f, normA = 0f, normB = 0f;
                int count = 0;

                for (int i = 0; i < onsetsCount - lag; i++)
                {
                    float a = _onsetsBuf[i];
                    float b = _onsetsBuf[i + lag];
                    correlation += a * b;
                    normA += a * a;
                    normB += b * b;
                    count++;
                }

                if (count > 0 && normA > 0 && normB > 0)
                    correlation /= (float)Math.Sqrt(normA * normB);
                else
                    correlation = 0f;

                float candidateBPM = 60f / (lag * timePerFrame);

                // Sparse lag bonus — but limited so it can't override strong real peaks
                if (sparseLag > 0)
                {
                    if (Math.Abs(lag - sparseLag) <= 1) correlation += 0.25f * (1f - correlation);
                    if (Math.Abs(lag - (sparseLag / 2)) <= 1) correlation += 0.18f * (1f - correlation);
                    if (Math.Abs(lag - (sparseLag * 2)) <= 1) correlation += 0.12f * (1f - correlation);
                }

                // Stronger prior toward 90–160 BPM range
                float logBPM = (float)Math.Log(candidateBPM / 115f);
                float tempoWeight = (float)Math.Exp(-0.5f * Math.Pow(logBPM / 0.9f, 2));  // tighter Gaussian

                if (candidateBPM > 220f) tempoWeight *= 0.45f;           // penalize very fast tempos
                if (candidateBPM < 60f) tempoWeight *= 0.60f;

                correlation *= tempoWeight;

                _lagCorBuf[lag] = correlation;

                if (correlation > maxCorrelation)
                {
                    maxCorrelation = correlation;
                    bestLag = lag;
                }
            }

            // Prefer half-time if it has decent correlation
            if (bestLag > minLag * 1.4f)
            {
                int lagHalf = bestLag / 2;
                float peakHalf = 0f;
                int bestHalf = 0;

                for (int l = lagHalf - 3; l <= lagHalf + 3; l++)
                {
                    if (l >= minLag && l <= maxLag && _lagCorBuf[l] > peakHalf)
                    {
                        peakHalf = _lagCorBuf[l];
                        bestHalf = l;
                    }
                }

                if (peakHalf > maxCorrelation * 0.45f)
                {
                    bestLag = bestHalf;
                    maxCorrelation = peakHalf;
                }
            }

            // ────────────────────────────────────────────────
            // Rejection gates — this is what kills 220+ ghosts
            // ────────────────────────────────────────────────
            if (maxCorrelation < 0.32f || peaksCount < 5)
                return;  // was 0.28 — slightly higher + require more peaks

            if (bestLag < 12 && maxCorrelation < 0.48f)  // very fast + not super confident → ignore
                return;

            // ────────────────────────────────────────────────
            // Fractional lag + BPM calculation
            // ────────────────────────────────────────────────
            float fractionalLag = bestLag;

            if (bestLag > minLag && bestLag < maxLag)
            {
                float yLeft   = _lagCorBuf[bestLag - 1];
                float yRight  = _lagCorBuf[bestLag + 1];
                float yCenter = maxCorrelation;
                float denom   = yLeft - 2f * yCenter + yRight;
                if (Math.Abs(denom) > 0.0001f)
                    fractionalLag += (yLeft - yRight) / (2f * denom);
            }

            float periodInSeconds = fractionalLag * timePerFrame;
            float detectedBPM = 60f / periodInSeconds;
            detectedBPM = Math.Clamp(detectedBPM, minBPM, maxBPM);

            // ────────────────────────────────────────────────
            // Blending & confidence update
            // ────────────────────────────────────────────────
            if (_confidence > 0.55f)
            {
                if (Math.Abs(detectedBPM - _currentBPM) > _currentBPM * 0.28f)
                    _confidence *= 0.45f;
            }
            else if (_currentBPM > 0f && Math.Abs(detectedBPM - _currentBPM) > _currentBPM * 0.40f)
            {
                _currentBPM = detectedBPM;
                _bpmHead = 0; _bpmCount = 0;
                _confidence = 0.18f;
            }

            _currentBPM = _currentBPM * 0.68f + detectedBPM * 0.32f;  // slightly faster adaptation

            PushBpm(_currentBPM);

            // Confidence components — manual loops, no LINQ allocation
            float avgEnergy = 0f;
            for (int i = 0; i < _energyCount; i++) avgEnergy += EnergyAt(i);
            if (_energyCount > 0) avgEnergy /= _energyCount;

            float correlationConfidence = 0f;
            if (avgEnergy > 0.0001f)
                correlationConfidence = Math.Min(1f, maxCorrelation / (avgEnergy * avgEnergy * 1.5f));  // less explosive

            float stabilityConfidence = 1f;
            if (_bpmCount >= 12)
            {
                int startIdx = _bpmCount - 12;
                float avg = 0f;
                for (int i = startIdx; i < _bpmCount; i++) avg += BpmAt(i);
                avg /= 12f;
                float variance = 0f;
                for (int i = startIdx; i < _bpmCount; i++) variance += Math.Abs(BpmAt(i) - avg);
                variance /= 12f;
                stabilityConfidence = Math.Max(0f, 1f - variance / 12f);  // tighter tolerance
            }

            _confidence = correlationConfidence * 0.35f + stabilityConfidence * 0.65f;

            if (_confidence > 0.78f && _bpmCount >= 24)
            {
                float sum = 0f;
                int startLock = _bpmCount - 18;
                for (int i = startLock; i < _bpmCount; i++) sum += BpmAt(i);
                _lockedBPM = sum / 18f;
            }

            // Final display update
            if (_bpmCount > 10)
            {
                float sum = 0f;
                int startDisp = _bpmCount - 10;
                for (int i = startDisp; i < _bpmCount; i++) sum += BpmAt(i);
                var maybe = (float)Math.Round(sum / 10f);

                _displayConfidence = _confidence * 100f;

                if (_displayConfidence > 90) //can start assuming hehe
                {
                    if (maybe % 10 == 9 || maybe % 10 == 4)
                    {
                        maybe += 1; //199=>200
                    }
                    else
                    if (maybe % 10 == 8 || maybe % 10 == 3)
                    {
                        maybe += 2;
                    }
                    else
                    if (maybe % 10 == 6 || maybe % 10 == 1)
                    {
                        maybe -= 1;
                    }
                    else
                    if (maybe % 10 == 2 || maybe % 10 == 7)
                    {
                        maybe -= 2;
                    }
                }
                _displayBPM = maybe;
            }
        }

        /// <summary>
        /// Not used in drawn mode — rendering is driven by DrawnUI's Paint() pipeline.
        /// </summary>
        public bool Render(SKCanvas canvas, SKRect viewport, float scale)
        {
            return false;
        }
    }
}
