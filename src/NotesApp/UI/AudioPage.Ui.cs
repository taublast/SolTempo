using AppoMobi.Specials;
using DrawnUi.Controls;
using DrawnUi.Views;
using MusicNotes.Audio;
using MusicNotes.Effects;
using MusicNotes.Helpers;
using ShadersCamera.Views;

namespace MusicNotes.UI
{
    public partial class AudioPage
    {
        private SkiaImage _backgroundImage;
        private SkiaShape _takePictureButton;
        private SkiaLabel _statusLabel;
        private SettingsButton _videoRecordButton;
        private SettingsButton _audioCodecButton;
        private SkiaLayer _previewOverlay;
        private SkiaImage _previewImage;
        private SkiaImage _previewThumbnail;
        private SettingsButton _preRecordingToggleButton;
        private SettingsButton _preRecordingDurationButton;
        private SkiaLabel _captionsLabel;
        private SkiaDrawer _settingsDrawer;
        private SkiaViewSwitcher _settingsTabs;
        private SkiaLabel[] _tabLabels;
        //private AudioVisualizer _rhythmDetector;
        //private AudioVisualizer _metronome;
        private int _currentMode = 0; // 0=Notes, 1=DrummerBPM, 2=Metronome, 3=MusicBPM
        private SkiaLabel _modeButtonIcon;

        private bool _isLayoutLandscape;
        private SkiaShape _captureButtonOuter;
        private AudioVisualizer _musicNotes;
        private SkiaControl _musicNotesWrapper;
        private AudioVisualizer _musicBPMDetector;
        private SkiaControl _musicBPMDetectorWrapper;
        private AudioVisualizer _equalizer;
        private AudioPageSettings _settingsPopup;

        private void CreateContent()
        {
            bool isSimulator = false;
            SkiaLayout mainStack = null;

            if (mainStack == null)
            {
                mainStack = new SkiaLayout
                {
                    BackgroundColor = Colors.Black,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    Children =
                    {

                        //new SkiaSvg()
                        //{
                        //    UseCache = SkiaCacheType.Operations,
                        //    Source = @"Svg\54170161_9344048.svg",
                        //    Aspect = TransformAspect.Fill,
                        //    TintColor = Colors.Maroon.WithAlpha(0.2f)
                        //}.Fill(),

                        new SkiaImage()
                        {
                            UseCache = SkiaCacheType.Image,
                            // https://unsplash.com/photos/music-room-with-lights-turned-on-gUK3lA3K7Yo
                            // by https://unsplash.com/@john_matychuk
                            Source = @"Images\musicback.jpg",
                            Aspect = TransformAspect.AspectCover,
                        }.Fill()
                        .Assign(out _backgroundImage),

                        // Fullscreen Camera preview
                        new AudioRecorder()
                        {
                            IsVisible=false,
                        }
                        .Assign(out Recorder),

                        //NOTES
                        new SkiaShape()
                        {
                            Margin = new (16,16,16,0),
                            HorizontalOptions = LayoutOptions.Fill,
                            HeightRequest = 375,
                            CornerRadius = 24,
                            Children =
                            {
                                new SkiaBackdrop()
                                {
                                    //UseCache = SkiaCacheType.Operations,
                                    HorizontalOptions = LayoutOptions.Fill,
                                    VerticalOptions = LayoutOptions.Fill,
                                    Blur = 0,
                                    VisualEffects = new List<SkiaEffect>
                                    {
                                        new GlassBackdropEffect()
                                        {
                                            ShaderSource = @"Shaders\glass.sksl",
                                            BlurStrength = 1.0f,
                                            GlassOpacity = 0.9f,
                                            GlassColor = Colors.Black.WithAlpha(0.33f),
                                            CornerRadius = 24,
                                            GlassDepth = 1.66f
                                        }
                                    }
                                },

                                new AudioVisualizer(new AudioInstrumentTuner())
                                {
                                    Margin = new (16,16,16,0),
                                    HorizontalOptions = LayoutOptions.Fill,
                                    VerticalOptions = LayoutOptions.Fill,
                                }.Assign(out _musicNotes),

                            }
                        }.Assign(out _musicNotesWrapper),
                       
                        //new AudioVisualizer(new AudioRhythmDetector())
                        //{
                        //    Margin = new (16,16,16,0),
                        //    HorizontalOptions = LayoutOptions.Fill,
                        //    HeightRequest = 350,
                        //    BackgroundColor = Color.Parse("#22000000"),
                        //    IsVisible = false,
                        //}.Assign(out _rhythmDetector),

                        //new AudioVisualizer(new AudioMetronome())
                        //{
                        //    Opacity = 0.9,
                        //    Margin = new (16,16,16,0),
                        //    HorizontalOptions = LayoutOptions.Fill,
                        //    HeightRequest = 350,
                        //    BackgroundColor = Color.Parse("#22000000"),
                        //    IsVisible = false,
                        //}.Assign(out _metronome),

                        // BPM MUSIC
                        new SkiaShape()
                        {
                            Margin = new (16,16,16,0),
                            HorizontalOptions = LayoutOptions.Fill,
                            HeightRequest = 350,

                            Children =
                            {
                                new SkiaBackdrop()
                                {
                                    //UseCache = SkiaCacheType.Operations,
                                    HorizontalOptions = LayoutOptions.Fill,
                                    VerticalOptions = LayoutOptions.Fill,
                                    Blur = 0,
                                    VisualEffects = new List<SkiaEffect>
                                    {
                                        new GlassBackdropEffect()
                                        {
                                            ShaderSource = @"Shaders\glass.sksl",
                                            BlurStrength = 1.0f,
                                            GlassOpacity = 0.9f,
                                            GlassColor = Colors.Black.WithAlpha(0.33f),
                                            CornerRadius = 32,
                                            GlassDepth = 1.66f
                                        }
                                    }
                                },

                                new AudioVisualizer(new AudioMusicBPMDetector())
                                {
                                    Margin = 1,
                                    HorizontalOptions = LayoutOptions.Fill,
                                    VerticalOptions = LayoutOptions.Fill,
                                }.Assign(out _musicBPMDetector),

                            }
                        }.Assign(out _musicBPMDetectorWrapper),

                        new AudioVisualizer(new AudioSoundBars())
                        {
                            BackgroundColor = Color.Parse("#22000000"),
                            VerticalOptions = LayoutOptions.End,
                            HorizontalOptions = LayoutOptions.Fill,
                            Margin = new (0,0,0,0),
                            HeightRequest = 240,
                        }.Assign(out _equalizer),


                        // Bottom Menu Bar
                        new SkiaShape()
                        {
                            Type = ShapeType.Rectangle,
                            CornerRadius = 32,
                            //BackgroundColor = Color.FromArgb("#DD000000"),
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.End,
                            Margin = new Thickness(0, 0, 0, 36),
                            Children =
                            {
                                new SkiaBackdrop()
                                {
                                    HorizontalOptions = LayoutOptions.Fill,
                                    VerticalOptions = LayoutOptions.Fill,
                                    Blur = 0,
                                    VisualEffects = new List<SkiaEffect>
                                    {
                                        new GlassBackdropEffect()
                                        {
                                            ShaderSource = @"Shaders\glass.sksl",
                                            BlurStrength = 1.0f,
                                            GlassOpacity = 0.9f,
                                            GlassColor = Colors.White.WithAlpha(0.05f),
                                            CornerRadius = 32,  // Match parent SkiaShape
                                            GlassDepth = 1.4f   // 3D emboss intensity (0.0-2.0+)
                                        }
                                    }
                                },

                                new SkiaRow()
                                {
                                    UseCache = SkiaCacheType.Operations,
                                    Margin = new Thickness(20, 16),
                                    Spacing = 16,
                                    HorizontalOptions = LayoutOptions.Center,
                                    VerticalOptions = LayoutOptions.Center,
                                    Children =
                                    {
                                        // Mode Button
                                        new SkiaShape()
                                        {
                                            Type = ShapeType.Rectangle,
                                            CornerRadius = 12,
                                            BackgroundColor = Color.FromArgb("#3B82F6"),
                                            WidthRequest = 48,
                                            HeightRequest = 48,
                                            Children =
                                            {
                                                new SkiaLabel()
                                                {
                                                    Text = IconFont.Music,
                                                    TextColor = Colors.WhiteSmoke,
                                                    FontFamily = "FontIcons",
                                                    FontSize = 24,
                                                    HorizontalOptions = LayoutOptions.Center,
                                                    VerticalOptions = LayoutOptions.Center,
                                                }
                                                .Assign(out _modeButtonIcon)
                                            },
                                            UseCache = SkiaCacheType.Image,
                                            FillGradient = new SkiaGradient()
                                            {
                                                Type = GradientType.Linear,
                                                Colors = new List<Color>()
                                                {
                                                    Colors.CornflowerBlue.Lighten(0.6f),
                                                    Colors.CornflowerBlue,
                                                    Colors.CornflowerBlue.Darken(0.6f)
                                                },
                                                ColorPositions = new List<double>()
                                                {
                                                    0.0,
                                                    0.2,
                                                    1.0,
                                                }
                                            }
                                        }
                                        .OnTapped(me =>
                                        {

                                           var fx = new TransitionEffect
                                           { 
                                               //ShaderSource = @"Shaders\transition_iris.sksl",
                                               ShaderSource = @"Shaders\transition_ripple.sksl",
                                               //ShaderSource = @"Shaders\transition_swirl.sksl",
                                               //ShaderSource = @"Shaders\transition_zoom.sksl",
                                               DurationMs = 600
                                           };
                                           fx.Midpoint  += (s, e) =>
                                           {
                                               ToggleVisualizerMode();
                                           };
                                           fx.Completed += (s, e) =>
                                           {
                                               mainStack.VisualEffects.Remove(fx);
                                               //mainStack.DisposeObject(fx);
                                           };

                                            mainStack.VisualEffects.Add(fx);
                                           fx.Play(_backgroundImage);

                                        }),

                                        // Settings Button
                                        new SkiaShape()
                                        {
                                            UseCache = SkiaCacheType.Image,
                                            Type = ShapeType.Rectangle,
                                            CornerRadius = 12,
                                            BackgroundColor = Color.FromArgb("#6B7280"),
                                            WidthRequest = 48,
                                            HeightRequest = 48,
                                            Children =
                                            {
                                                new SkiaLabel()
                                                {
                                                    Text = IconFont.Cog,
                                                    TextColor = Colors.WhiteSmoke,
                                                    FontFamily = "FontIcons",
                                                    FontSize = 24,
                                                    HorizontalOptions = LayoutOptions.Center,
                                                    VerticalOptions = LayoutOptions.Center,
                                                },
                                            },
                                            FillGradient = new SkiaGradient()
                                            {
                                                Type = GradientType.Linear,
                                                Colors = new List<Color>()
                                                {
                                                    Colors.DarkCyan.Lighten(0.6f),
                                                    Colors.DarkCyan,
                                                    Colors.DarkCyan.Darken(0.6f)
                                                },
                                                ColorPositions = new List<double>()
                                                {
                                                    0.0,
                                                    0.2,
                                                    1.0,
                                                }
                                            }
                                        }
                                        .OnTapped(me =>
                                        {
                                            _settingsPopup?.Show();
                                            //MainThread.BeginInvokeOnMainThread(() =>
                                            //{
                                            //    var popup = new AudioPageSettingsPopup(this);
                                            //    this.ShowPopup(popup);
                                            //});
                                            //ToggleSettingsDrawer();
                                        }),

                                        // Help Button
                                        new SkiaShape()
                                        {
                                            Type = ShapeType.Rectangle,
                                            CornerRadius = 12,
                                            BackgroundColor = Color.FromArgb("#6B7280"),
                                            WidthRequest = 48,
                                            HeightRequest = 48,
                                            Children =
                                            {
                                                new SkiaLabel()
                                                {
                                                    Text = IconFont.Help,
                                                    //Text = IconFont.CloudQuestion,
                                                    TextColor = Colors.WhiteSmoke,
                                                    FontFamily = "FontIcons",
                                                    FontSize = 24,
                                                    HorizontalOptions = LayoutOptions.Center,
                                                    VerticalOptions = LayoutOptions.Center,
                                                },
                                            },
                                            UseCache = SkiaCacheType.Image,
                                            FillGradient = new SkiaGradient()
                                            {
                                                Type = GradientType.Linear,
                                                Colors = new List<Color>()
                                                {
                                                    Colors.Orange.Lighten(0.6f),
                                                    Colors.Orange,
                                                    Colors.Orange.Darken(0.6f)
                                                },
                                                ColorPositions = new List<double>()
                                                {
                                                    0.0,
                                                    0.2,
                                                    1.0,
                                                }
                                            }
                                        }
                                        .OnTapped(me =>
                                        {
                                            TriggerCelebration();
                                        }),

                                        /*
                                        // Profile Button
                                        new SkiaShape()
                                        {
                                            Type = ShapeType.Rectangle,
                                            CornerRadius = 12,
                                            BackgroundColor = Color.FromArgb("#6B7280"),
                                            WidthRequest = 48,
                                            HeightRequest = 48,
                                            Children =
                                            {
                                                new SkiaRichLabel("👤")
                                                {
                                                    FontSize = 24,
                                                    HorizontalOptions = LayoutOptions.Center,
                                                    VerticalOptions = LayoutOptions.Center,
                                                }
                                            },
                                            UseCache = SkiaCacheType.Image,
                                            FillGradient = new SkiaGradient()
                                            {
                                                Type = GradientType.Linear,
                                                Colors = new List<Color>()
                                                {
                                                    Colors.DarkGrey,
                                                    Colors.DarkOliveGreen,
                                                    Colors.Gray
                                                },
                                                ColorPositions = new List<double>()
                                                {
                                                    0.0,
                                                    0.2,
                                                    1.0,
                                                }
                                            }
                                        }
                                        .OnTapped(me => {  }),
                                        */
                                    }
                                }
                            },
                        },

                        new AudioPageSettings(this)
                        {
                            IsVisible=false
                        }.Assign(out _settingsPopup)
                    }
                };
            }

            // Main layer that contains both the main stack and preview overlay
            var rootLayer = new SkiaLayer
            {
                VerticalOptions = LayoutOptions.Fill,
                Children =
            {
                mainStack,
                _previewOverlay,
#if DEBUG
                new SkiaLabelFps()
                {
                    Margin = new(0, 0, 4, 24),
                    VerticalOptions = LayoutOptions.End,
                    HorizontalOptions = LayoutOptions.End,
                    Rotation = -45,
                    BackgroundColor = Colors.DarkRed,
                    TextColor = Colors.White,
                    ZIndex = 110,
                }
#endif
            }
            };

            Canvas = new Canvas
            {
                RenderingMode = RenderingModeType.Accelerated,
                Gestures = GesturesMode.Enabled,
                Content = rootLayer,
            };

            Canvas.WillFirstTimeDraw += (sender, context) =>
            {
                if (Recorder != null)
                {
                    Tasks.StartDelayed(TimeSpan.FromMilliseconds(500), () =>
                    {
                        Recorder.IsOn = true;
                        // Speech recognition will auto-start/stop based on recording state
                    });
                }
            };

            Content = new Grid() //due to maui layout specifics we are forced to use a Grid as root wrapper
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Children = { Canvas }
            };

            if (Recorder != null)
            {
                // Setup SkiaCamera event handlers and apply user settings to it
                AttachHardware(true);
            }

            ToggleVisualizerMode(UserSettings.Current.Module);
        }

        /// <summary>
        /// Plays a celebration shader over the background image.
        /// Uncomment one ShaderSource to compare variants:
        ///   celebrate_starburst            — golden rays + sparkles               (2500 ms)
        ///   celebrate_waves                — rainbow concentric rings              (3000 ms)
        ///   celebrate_confetti             — tumbling rectangular confetti (rain)  (3500 ms)
        ///   celebrate_confetti_burst       — confetti pops radially from center    (3500 ms)
        ///   celebrate_confetti_cannon      — two corner cannons, streams cross     (3500 ms)
        ///   celebrate_confetti_streamers   — long ribbons flutter as they fall     (3500 ms)
        ///   celebrate_fireworks            — 4 sequential colored bursts           (3500 ms)
        ///   celebrate_fireworks_launch     — rockets with comet trails, then burst (4000 ms)
        ///   celebrate_fireworks_bloom      — 3 big chrysanthemum blooms            (4000 ms)
        ///   celebrate_fireworks_glitter    — 5 dense blinking glitter bursts       (3500 ms)
        ///   celebrate_aurora               — chromatic aurora curtains              (3000 ms)
        ///   celebrate_stars                — twinkling star rain                    (3500 ms)
        /// </summary>
        public void TriggerCelebration()
        {
            if (_backgroundImage == null)
                return;

            var fx = new CelebrationEffect
            {
                ShaderSource = @"Shaders\celebrate_starburst.sksl",
                //DurationMs = 2500,
                //ShaderSource = @"Shaders\celebrate_waves.sksl",
                //DurationMs = 3000,
                //ShaderSource = @"Shaders\celebrate_confetti.sksl",
                //DurationMs = 3500,
                //ShaderSource = @"Shaders\celebrate_confetti_burst.sksl",
                //DurationMs = 3500,
                //ShaderSource = @"Shaders\celebrate_fireworks.sksl",
                //DurationMs = 3500,
                //ShaderSource = @"Shaders\celebrate_fireworks_launch.sksl",
                //DurationMs = 4000,
                //ShaderSource = @"Shaders\celebrate_fireworks_bloom.sksl",
                DurationMs = 5000,
                //ShaderSource = @"Shaders\celebrate_fireworks_glitter.sksl",
                //DurationMs = 3500,
                //ShaderSource = @"Shaders\celebrate_aurora.sksl",
                //DurationMs = 3000,
                //ShaderSource = @"Shaders\celebrate_stars.sksl",
                //DurationMs = 3500,
                //Center = new SKPoint(0.5f, 0.33f), // adjust origin of radial effects
            };

            fx.Completed += (s, e) =>
            {
                _backgroundImage.VisualEffects.Remove(fx);
                System.Diagnostics.Debug.WriteLine($"Stopped effect {fx.ShaderSource}");
            };

            _backgroundImage.VisualEffects.Add(fx);
            System.Diagnostics.Debug.WriteLine($"Started effect {fx.ShaderSource}");
            fx.Play(_backgroundImage);
        }

        public void ShowAlert(string title, string message)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert(title, message, "OK");
            });
        }

        private void ToggleVisualizerMode(int index = -1)
        {
            var oldMode = _currentMode;
            if (index >= 0)
            {
                _currentMode = index;
            }
            else
            {
                // Cycle through modes: 0=Notes, 1=DrummerBPM, 2=Metronome, 3=MusicBPM
                _currentMode = (_currentMode + 1) % 2;
            }

            // Hide all visualizers
            if (_musicNotesWrapper != null)
                _musicNotesWrapper.IsVisible = false;
            //if (_rhythmDetector != null)
            //    _rhythmDetector.IsVisible = false;
            //if (_metronome != null)
            //    _metronome.IsVisible = false;
            if (_musicBPMDetectorWrapper != null)
                _musicBPMDetectorWrapper.IsVisible = false;

            // Show current mode visualizer
            switch (_currentMode)
            {
            case 0: // Notes
            if (_musicNotesWrapper != null)
                _musicNotesWrapper.IsVisible = true;
            if (_modeButtonIcon != null)
                _modeButtonIcon.Text = IconFont.PlaylistMusic;//"🎵";
            break;
            case 1:
            if (_musicBPMDetectorWrapper != null)
                _musicBPMDetectorWrapper.IsVisible = true;
            if (_modeButtonIcon != null)
                _modeButtonIcon.Text = IconFont.TimerMusic;//IconFont.Music;//"🎼";                
            break;
            case 2: // Metronome
            //if (_metronome != null)
            //    _metronome.IsVisible = true;
            if (_modeButtonIcon != null)
                _modeButtonIcon.Text = IconFont.AccountMusic;// "⏱️";
            break;
            case 3: // Music BPM

            // Drummer BPM
            //if (_rhythmDetector != null)
            //    _rhythmDetector.IsVisible = true;
            if (_modeButtonIcon != null)
                _modeButtonIcon.Text = IconFont.DotsCircle; // IconFont.TimerMusic;// IconFont.Metronome;// "🥁";
            break;
            }

            if (oldMode != _currentMode)
            {
                UserSettings.Current.Module = _currentMode;
                UserSettings.Save();
            }
        }

    }
}
