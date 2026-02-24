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

        // ---- Waveform paints (lazy-init in Paint) ----
        private SKPaint _paintEnergyWave;
        private SKPaint _paintWaveform;

        // ---- Audio processing state (identical to AudioMusicBPMDetector) ----
        private const int BufferSize = 8192;
        private float[] _sampleBuffer = new float[BufferSize];
        private int _writePos = 0;
        private int _samplesAddedSinceLastScan = 0;
        private int _sampleRate = 44100;
        private const int ScanInterval = 1024;

        private List<float> _energyHistory = new List<float>();
        private const int MaxEnergyHistory = 800;

        private float _currentBPM = 0;
        private List<float> _bpmHistory = new List<float>();
        private bool _hasSignal = false;
        private float _confidence = 0;
        private float _lockedBPM = 0;

        private long _clockMs = 0;
        private float _noiseFloor = 0.005f;

        private float _displayBPM = 0;
        private float _displayConfidence = 0;
        private int _swapRequested = 0;
        private List<float> _displayEnergyWave = new List<float>();

        // Multi-band filter state
        private float _lowPassState = 0;
        private float _prevVal = 0;
        private int _framesSinceLastDetect = 0;

        public bool UseGain { get; set; } = true;
        public int Skin { get; set; } = 0;

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
                    Text = "Tap to reset tempo",
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

            _displayEnergyWave = new List<float>(_energyHistory);

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

            // Energy waveform (below the center label area)
            var wave = _displayEnergyWave;
            if (wave.Count > 1)
            {
                float waveHeight = 60 * scale;
                float waveY = centerY + 130 * scale;
                float step = width / Math.Max(1, wave.Count);
                float maxEnergy = 0;
                for (int i = 0; i < wave.Count; i++)
                    if (wave[i] > maxEnergy) maxEnergy = wave[i];

                if (maxEnergy > 0)
                {
                    using var path = new SKPath();
                    bool first = true;
                    for (int i = 0; i < wave.Count; i++)
                    {
                        float val = wave[i] / maxEnergy;
                        float x = left + i * step;
                        float y = waveY - val * waveHeight;
                        if (first)
                        {
                            path.MoveTo(x, y); first = false;
                        }
                        else path.LineTo(x, y);
                    }
                    canvas.DrawPath(path, _paintEnergyWave);
                }
            }

            // Raw waveform at bottom
            lock (_sampleBuffer)
            {
                float waveHeight = 40 * scale;
                float waveY = top + height - 70 * scale;
                int samples = Math.Min(150, BufferSize / 4);
                float step = width / samples;

                using var path = new SKPath();
                bool first = true;
                for (int i = 0; i < samples; i++)
                {
                    int idx = (_writePos - samples + i + BufferSize) % BufferSize;
                    float val = _sampleBuffer[idx];
                    float x = left + i * step;
                    float y = waveY + val * waveHeight * 0.5f;
                    if (first) { path.MoveTo(x, y); first = false; }
                    else path.LineTo(x, y);
                }
                canvas.DrawPath(path, _paintWaveform);
            }
        }

        public override void OnWillDisposeWithChildren()
        {
            base.OnWillDisposeWithChildren();
            _paintEnergyWave?.Dispose();
            _paintEnergyWave = null;
            _paintWaveform?.Dispose();
            _paintWaveform = null;
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

                _energyHistory.Clear();
                _bpmHistory.Clear();
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
                _displayEnergyWave.Clear();
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
            if (_displayBPM > 0)
            {
  
                _labelNoSignal.IsVisible = false;
                _labelBpm.IsVisible = true;
                _labelBpmUnit.IsVisible = true;

                if (_displayConfidence > 99)
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
                    _labelConfidence.Text = $"Thinking..";
                }
            }
            else
            {
                _labelNoSignal.IsVisible = true;
                _labelBpm.IsVisible = false;
                _labelBpmUnit.IsVisible = false;

                _labelConfidence.TextColor = Colors.Orange;
                _labelConfidence.Text = $"Listening..";
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
                if (_energyHistory.Count > 40 && _energyHistory.TakeLast(30).All(e => e <= silenceThreshold * 1.4f))
                {
                    _bpmHistory.Clear();
                    _currentBPM = 0f;
                    _lockedBPM = 0f;
                    _confidence = 0f;
                }
            }
            else
            {
                _hasSignal = true;
            }

            _energyHistory.Add(energy);
            if (_energyHistory.Count > MaxEnergyHistory)
                _energyHistory.RemoveAt(0);
        }

        private void DetectMusicBPM()
        {
            if (_energyHistory.Count < 160) return;           // increased from 130 — need more context

            // ────────────────────────────────────────────────
            // Onset strength calculation — improved noise rejection
            // ────────────────────────────────────────────────
            List<float> onsets = new List<float>();
            float maxOnset = 0f;

            const int avgWindow = 7;  // slightly larger → more stable local average

            for (int i = avgWindow; i < _energyHistory.Count; i++)
            {
                float sumPrev = 0f;
                for (int w = 1; w <= avgWindow; w++)
                    sumPrev += _energyHistory[i - w];

                float localAvg = sumPrev / avgWindow;

                float diff = _energyHistory[i] - localAvg;

                // Stricter gate — prevents noise from creating fake onsets
                if (diff < 0.004f) diff = 0f;                   // was 0.002f — raise this

                float val = Math.Max(0f, diff);

                // Cheap logarithmic compression — crushes small noise, preserves strong onsets
                val = (float)Math.Max(0f, Math.Log(1f + 35f * val) - 0.10f);

                onsets.Add(val);
                if (val > maxOnset) maxOnset = val;
            }

            // ────────────────────────────────────────────────
            // Sparse peak detection — stricter to avoid false fast tempos
            // ────────────────────────────────────────────────
            int sparseLag = 0;
            bool hasPeaks = maxOnset > 0.012f;  // slightly higher than before

            List<int> peakIndices = new List<int>();

            if (hasPeaks)
            {
                for (int i = 3; i < onsets.Count - 3; i++)  // wider neighborhood
                {
                    if (onsets[i] > maxOnset * 0.50f          // was 0.35 — much stricter
                        && onsets[i] > 0.09f                  // absolute floor — kills noise peaks
                        && onsets[i] >= onsets[i - 1]
                        && onsets[i] >= onsets[i - 2]
                        && onsets[i] >= onsets[i - 3]
                        && onsets[i] > onsets[i + 1]
                        && onsets[i] > onsets[i + 2]
                        && onsets[i] > onsets[i + 3])
                    {
                        peakIndices.Add(i);
                        i += 4;  // skip more aggressively to avoid double-counting same event
                    }
                }

                if (peakIndices.Count >= 4)  // need more evidence
                {
                    var intervals = new List<int>();
                    for (int k = 0; k < peakIndices.Count - 1; k++)
                    {
                        int delta = peakIndices[k + 1] - peakIndices[k];
                        if (delta > 6) intervals.Add(delta);  // was 8 — slightly lower
                    }

                    if (intervals.Count > 0)
                    {
                        var bestGroup = intervals
                            .GroupBy(x => x)
                            .OrderByDescending(g => g.Count())
                            .FirstOrDefault();

                        if (bestGroup?.Count() >= 2)
                            sparseLag = bestGroup.Key;
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
            maxLag = Math.Min(_energyHistory.Count / 2, maxLag);

            if (minLag >= maxLag) return;

            float maxCorrelation = 0f;
            int bestLag = 0;
            var lagCorrelations = new Dictionary<int, float>();

            for (int lag = minLag; lag <= maxLag; lag++)
            {
                float correlation = 0f, normA = 0f, normB = 0f;
                int count = 0;

                for (int i = 0; i < onsets.Count - lag; i++)
                {
                    float a = onsets[i];
                    float b = onsets[i + lag];
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

                lagCorrelations[lag] = correlation;

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
                    if (lagCorrelations.TryGetValue(l, out float val) && val > peakHalf)
                    {
                        peakHalf = val;
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
            if (maxCorrelation < 0.32f || peakIndices.Count < 5)
                return;  // was 0.28 — slightly higher + require more peaks

            if (bestLag < 12 && maxCorrelation < 0.48f)  // very fast + not super confident → ignore
                return;

            // ────────────────────────────────────────────────
            // Fractional lag + BPM calculation
            // ────────────────────────────────────────────────
            float fractionalLag = bestLag;

            if (bestLag > minLag && bestLag < maxLag)
            {
                if (lagCorrelations.TryGetValue(bestLag - 1, out float yLeft) &&
                    lagCorrelations.TryGetValue(bestLag + 1, out float yRight))
                {
                    float yCenter = maxCorrelation;
                    float denom = yLeft - 2f * yCenter + yRight;
                    if (Math.Abs(denom) > 0.0001f)
                    {
                        float offset = (yLeft - yRight) / (2f * denom);
                        fractionalLag += offset;
                    }
                }
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
                _bpmHistory.Clear();
                _confidence = 0.18f;
            }

            _currentBPM = _currentBPM * 0.68f + detectedBPM * 0.32f;  // slightly faster adaptation

            _bpmHistory.Add(_currentBPM);
            if (_bpmHistory.Count > 40)  // longer history → more stable average
                _bpmHistory.RemoveAt(0);

            // Confidence components
            float avgEnergy = _energyHistory.Count > 0 ? _energyHistory.Average() : 0f;
            float correlationConfidence = 0f;
            if (avgEnergy > 0.0001f)
                correlationConfidence = Math.Min(1f, maxCorrelation / (avgEnergy * avgEnergy * 1.5f));  // less explosive

            float stabilityConfidence = 1f;
            if (_bpmHistory.Count >= 12)
            {
                var recent = _bpmHistory.Skip(_bpmHistory.Count - 12).ToList();
                float avg = recent.Average();
                float variance = recent.Select(b => Math.Abs(b - avg)).Average();
                stabilityConfidence = Math.Max(0f, 1f - variance / 12f);  // tighter tolerance
            }

            _confidence = correlationConfidence * 0.35f + stabilityConfidence * 0.65f;

            if (_confidence > 0.78f && _bpmHistory.Count >= 24)
                _lockedBPM = _bpmHistory.Skip(_bpmHistory.Count - 18).Average();

            // Final display update
            if (_bpmHistory.Count > 10)
            {
                var recentBPMs = _bpmHistory.Skip(_bpmHistory.Count - 10).ToList();
                _displayBPM = recentBPMs.Average();
                _displayConfidence = _confidence * 100f;
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
