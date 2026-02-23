using AppoMobi.Maui.Gestures;
using DrawnUi.Camera;
using MusicNotes.UI;

namespace MusicNotes.Audio
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
                    Margin = new(0,166,0,0),
                    FontSize = 24,
                    FontFamily = AppFonts.Default,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center,
                    UseCache = SkiaCacheType.Operations,
                }.Assign(out _labelBpmUnit),

                new SkiaLabel
                {
                    FontSize = 19,
                    Margin = new(0,200,0,0),
                    FontFamily = AppFonts.Default,
                    TextColor = Colors.LimeGreen,
                    HorizontalOptions = LayoutOptions.Center,
                    UseCache = SkiaCacheType.Operations,
                    IsVisible = false,
                }.Assign(out _labelConfidence),

                new SkiaLabel
                {
                    Margin = new Thickness(16,40),
                    Text = "Play music to detect tempo",
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

            // Swap waveform snapshot atomically from audio thread
            if (System.Threading.Interlocked.CompareExchange(ref _swapRequested, 0, 1) == 1)
            {
                _displayEnergyWave = new List<float>(_energyHistory);
            }

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
                    Color = SKColors.Magenta,
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
            _labelConfidence.IsVisible = false;
            _labelNoSignal.IsVisible = true;

            Update();
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
                        System.Threading.Interlocked.Exchange(ref _swapRequested, 1);
                        Update();
                    }

                    _samplesAddedSinceLastScan = 0;
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
                _labelBpm.Text = $"{_displayBPM:F0}";
                _labelBpm.TextColor = _hasSignal ? Colors.White : Colors.Gray;

                if (_displayConfidence > 20)
                {
                    Color confidenceColor = _displayConfidence > 70 ? Colors.LimeGreen :
                                           _displayConfidence > 40 ? Colors.Yellow : Colors.Orange;
                    _labelConfidence.TextColor = confidenceColor;
                    _labelConfidence.Text = $"Confidence: {_displayConfidence:F0}%";
                    _labelConfidence.IsVisible = true;
                }
                else
                {
                    _labelConfidence.IsVisible = false;
                }
            }
            else
            {
                _labelNoSignal.IsVisible = true;
                _labelBpm.IsVisible = false;
                _labelBpmUnit.IsVisible = false;
                _labelConfidence.IsVisible = false;
            }
        }

        private void ComputeEnergyFrame()
        {
            float energyLow = 0;
            float energyMidHigh = 0;

            int windowSize = ScanInterval;
            float alphaLow = 0.05f;

            for (int i = 0; i < windowSize; i++)
            {
                int idx = (_writePos - windowSize + i + BufferSize) % BufferSize;
                float val = _sampleBuffer[idx];

                _lowPassState = _lowPassState + alphaLow * (val - _lowPassState);
                energyLow += _lowPassState * _lowPassState;

                float diff = val - _prevVal;
                _prevVal = val;
                energyMidHigh += diff * diff;
            }

            energyLow = (float)Math.Sqrt(energyLow / windowSize);
            energyMidHigh = (float)Math.Sqrt(energyMidHigh / windowSize);

            float energy = energyLow + energyMidHigh;

            if (_noiseFloor < 0.0001f) _noiseFloor = 0.0001f;
            _noiseFloor = _noiseFloor * 0.999f + energy * 0.001f;

            float silenceThreshold = Math.Max(UseGain ? 0.0005f : 0.0002f, _noiseFloor * 0.8f);

            if (energy < silenceThreshold)
            {
                energy = 0;
                if (_hasSignal) { _hasSignal = false; }
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
            if (_energyHistory.Count < 130) return;

            List<float> onsets = new List<float>();
            float maxOnset = 0;

            int avgWindow = 6;

            for (int i = avgWindow; i < _energyHistory.Count; i++)
            {
                float sum = 0;
                for (int w = 1; w <= avgWindow; w++)
                    sum += _energyHistory[i - w];
                float localAvg = sum / avgWindow;

                float diff = _energyHistory[i] - localAvg;
                if (diff < 0.002f) diff = 0;

                float val = Math.Max(0, diff);
                onsets.Add(val);

                if (val > maxOnset) maxOnset = val;
            }

            int sparseLag = 0;
            bool hasPeaks = (maxOnset > 0.01f);

            if (hasPeaks)
            {
                List<int> peakIndices = new List<int>();

                for (int i = 2; i < onsets.Count - 2; i++)
                {
                    if (onsets[i] > (maxOnset * 0.35f)
                        && onsets[i] >= onsets[i - 1] && onsets[i] >= onsets[i - 2]
                        && onsets[i] > onsets[i + 1] && onsets[i] > onsets[i + 2])
                    {
                        peakIndices.Add(i);
                        i += 2;
                    }
                }

                if (peakIndices.Count >= 3)
                {
                    List<int> intervals = new List<int>();
                    for (int k = 0; k < peakIndices.Count - 1; k++)
                    {
                        int delta = peakIndices[k + 1] - peakIndices[k];
                        if (delta > 8) intervals.Add(delta);
                    }

                    var bestGroup = intervals
                        .GroupBy(x => x)
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault();

                    if (bestGroup != null && bestGroup.Count() >= 2)
                    {
                        sparseLag = bestGroup.Key;
                    }
                }
            }

            int minBPM = 40;
            int maxBPM = 220;
            float timePerFrame = (float)ScanInterval / _sampleRate;

            int minLag = (int)(60f / maxBPM / timePerFrame);
            int maxLag = (int)(60f / minBPM / timePerFrame);

            maxLag = Math.Min(_energyHistory.Count / 2, maxLag);

            if (minLag >= maxLag)
                return;

            float maxCorrelation = 0;
            int bestLag = 0;
            Dictionary<int, float> lagCorrelations = new Dictionary<int, float>();

            for (int lag = minLag; lag <= maxLag; lag++)
            {
                float correlation = 0;
                float normA = 0;
                float normB = 0;
                int count = 0;

                for (int i = 0; i < onsets.Count - lag; i++)
                {
                    float valA = onsets[i];
                    float valB = onsets[i + lag];

                    correlation += valA * valB;
                    normA += valA * valA;
                    normB += valB * valB;
                    count++;
                }

                if (count > 0 && normA > 0 && normB > 0)
                {
                    correlation = correlation / (float)Math.Sqrt(normA * normB);
                }

                float lagTime = lag * timePerFrame;
                float candidateBPM = 60f / lagTime;

                if (sparseLag > 0)
                {
                    if (Math.Abs(lag - sparseLag) <= 1) correlation += 0.4f;
                    if (Math.Abs(lag - (sparseLag / 2)) <= 1) correlation += 0.3f;
                    if (Math.Abs(lag - (sparseLag * 2)) <= 1) correlation += 0.2f;
                }

                float logBPM = (float)Math.Log(candidateBPM / 120f);
                float tempoWeight = (float)Math.Exp(-0.5f * Math.Pow(logBPM / 1.4f, 2));

                if (correlation > 0.6f) tempoWeight = 1.0f; else correlation *= tempoWeight;

                lagCorrelations[lag] = correlation;

                if (correlation > maxCorrelation)
                {
                    maxCorrelation = correlation;
                    bestLag = lag;
                }
            }

            if (bestLag > minLag * 1.5f)
            {
                int lag2x = bestLag / 2;

                float peak2x = 0;
                int bestLag2x = 0;

                for (int l = lag2x - 2; l <= lag2x + 2; l++)
                {
                    if (lagCorrelations.TryGetValue(l, out float val) && val > peak2x)
                    {
                        peak2x = val;
                        bestLag2x = l;
                    }
                }

                if (peak2x > (maxCorrelation * 0.40f))
                {
                    bestLag = bestLag2x;
                    maxCorrelation = peak2x;
                }
            }

            if (bestLag > 0 && maxCorrelation > 0.15f)
            {
                float fractionalLag = bestLag;
                if (bestLag > minLag && bestLag < maxLag)
                {
                    if (lagCorrelations.TryGetValue(bestLag - 1, out float yLeft) &&
                        lagCorrelations.TryGetValue(bestLag + 1, out float yRight))
                    {
                        float yCenter = maxCorrelation;
                        float denominator = (yLeft - 2 * yCenter + yRight);
                        if (Math.Abs(denominator) > 0.0001f)
                        {
                            float offset = (yLeft - yRight) / (2 * denominator);
                            fractionalLag = bestLag + offset;
                        }
                    }
                }

                float periodInSeconds = fractionalLag * timePerFrame;
                float detectedBPM = 60f / periodInSeconds;

                detectedBPM = Math.Clamp(detectedBPM, minBPM, maxBPM);

                if (_confidence > 0.5f)
                {
                    if (Math.Abs(detectedBPM - _currentBPM) > (_currentBPM * 0.25f))
                    {
                        _confidence *= 0.5f;
                    }
                }
                else
                {
                    if (_currentBPM > 0 && Math.Abs(detectedBPM - _currentBPM) > (_currentBPM * 0.35f))
                    {
                        _currentBPM = detectedBPM;
                        _bpmHistory.Clear();
                        _confidence = 0.2f;
                    }
                }

                _currentBPM = _currentBPM * 0.7f + detectedBPM * 0.3f;

                _bpmHistory.Add(_currentBPM);
                if (_bpmHistory.Count > 30)
                    _bpmHistory.RemoveAt(0);

                float avgEnergy = _energyHistory.Average();
                float correlationConfidence = 0;
                if (avgEnergy > 0)
                    correlationConfidence = Math.Min(1.0f, maxCorrelation / (avgEnergy * avgEnergy) * 3f);

                float stabilityConfidence = 1.0f;
                if (_bpmHistory.Count >= 10)
                {
                    float avgBPM = _bpmHistory.Skip(_bpmHistory.Count - 10).Average();
                    float variance = _bpmHistory.Skip(_bpmHistory.Count - 10).Select(b => Math.Abs(b - avgBPM)).Average();
                    stabilityConfidence = Math.Max(0, 1.0f - variance / 15f);
                }

                _confidence = (correlationConfidence * 0.3f + stabilityConfidence * 0.7f);

                if (_confidence > 0.75f && _bpmHistory.Count >= 20)
                {
                    _lockedBPM = _bpmHistory.Skip(_bpmHistory.Count - 15).Average();
                }
            }

            if (_bpmHistory.Count > 8)
            {
                var recentBPMs = _bpmHistory.Skip(_bpmHistory.Count - 8).ToList();
                _displayBPM = recentBPMs.Average();
                _displayConfidence = _confidence * 100;
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
