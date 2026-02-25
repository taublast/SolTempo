using AppoMobi.Specials;
using System.Diagnostics;
using DrawnUi.Controls;
using DrawnUi.Models;
using DrawnUi.Views;
using SolTempo.Audio;
using SolTempo.Effects;
using SolTempo.Helpers;
using SolTempo.Resources.Strings;
using ShadersCamera.Views;

namespace SolTempo.UI;

public partial class AudioPage
{
    Canvas Canvas;

    private MutliImages _background;
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
    private int _currentMode = 0; // 0=Notes, 2=MusicBPM
    private TintedShape _modeButton;
    private SkiaLabel _modeButtonIcon;
    private bool _isLayoutLandscape;
    private SkiaShape _captureButtonOuter;
    public AudioNotesDetector NotesModule;
    private SkiaControl _musicNotesWrapper;
    private AudioMusicBPM _musicBPMDetector;
    private SkiaControl _musicBPMDetectorWrapper;
    private AudioVisualizer _equalizer;
    private SettingsPopup _settingsPopup;
    private HelpPopup _helpPopup;
    private LayerWIthEffects _mainStack;

    private AchievementEngine _achievementEngine;
    private SkiaShape _achievementBanner;
    private SkiaLabel _achievementBannerLabel;
    private CancellationTokenSource _bannerCts;

    private object lockNav = new();

    private bool IsBusy { get; set; }

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            if (Recorder != null)
            {
                Recorder?.Stop();
                AttachHardware(false);
                Recorder = null;
            }

            this.Content = null;
            Canvas?.Dispose();

        }

        base.Dispose(isDisposing);
    }

    /// <summary>
    /// This will be called by HotReload
    /// </summary>
    public override void Build()
    {
        if (Recorder != null)
        {
            Recorder?.Stop();
            AttachHardware(false);
            Recorder = null;
        }

        Canvas?.Dispose();

        CreateContent();
    }

    public AudioPage()
    {
        Title = ResStrings.AppTitle;

        ToolbarItems.Add(new ToolbarItem
        {
            Text = "Share App",
            Order = ToolbarItemOrder.Secondary,
            Priority = 0,
            Command = new Command(OnShareMenuTapped)
        });

        ToolbarItems.Add(new ToolbarItem
        {
            Text = "Privacy Policy",
            Order = ToolbarItemOrder.Secondary,
            Priority = 1,
            Command = new Command(OnPrivacyMenuTapped)
        });
    }

    private async void OnShareMenuTapped()
    {
        await Share.RequestAsync(new ShareTextRequest
        {
            Title = ResStrings.AppTitle,
            Text = "Check out SolTempo – music notes & BPM detector!\nhttps://github.com/taublast/SolTempo"
        });
    }

    private async void OnPrivacyMenuTapped()
    {
        await DisplayAlert(
            "Privacy Policy",
            "SolTempo does not collect, store, or share any personal data. All audio processing happens locally on your device.",
            ResStrings.BtnOk);
    }

    private void CreateContent()
    {
        bool isSimulator = false;
        //SkiaLayout mainStack = null;

        //change here for convinient HotReload
        float buttonIconFontSize = 26;

        //if (mainStack == null)
        {
            _mainStack = new LayerWIthEffects
            {
                BackgroundColor = Colors.Black,
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

                        new MutliImages().Assign(out _background),
                        
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

                                new AudioNotesDetector()
                                {
                                    Margin = new (16,16,16,0),
                                    HorizontalOptions = LayoutOptions.Fill,
                                    VerticalOptions = LayoutOptions.Fill,
                                }.Assign(out NotesModule),

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
                            IsVisible=false,
                            Margin = new (16,16,16,0),
                            HorizontalOptions = LayoutOptions.Fill,
                            HeightRequest = 320,

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

                                new AudioMusicBPM()
                                {
                                    Margin = new (6,16,6,6),
                                    HorizontalOptions = LayoutOptions.Fill,
                                    VerticalOptions = LayoutOptions.Fill,
                                }.Assign(out _musicBPMDetector),

                                //new AudioVisualizer(new AudioMusicBPMDetectorLegacy())
                                //{
                                //    Margin = 1,
                                //    HorizontalOptions = LayoutOptions.Fill,
                                //    VerticalOptions = LayoutOptions.Fill,
                                //}.Assign(out _musicBPMDetector),

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
                                            GlassDepth = 0.75f   // 3D emboss intensity (0.0-2.0+)
                                        }
                                    }
                                },

                                new SkiaRow()
                                {
                                    UseCache = SkiaCacheType.Operations,
                                    Margin = new Thickness(20, 12),
                                    Spacing = 16,
                                    HorizontalOptions = LayoutOptions.Center,
                                    VerticalOptions = LayoutOptions.Center,
                                    Children =
                                    {
                                        // Mode Button
                                        new TintedShape()
                                        {
                                            TintColor = Colors.CornflowerBlue,
                                            UseCache = SkiaCacheType.Image,
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
                                                    FontSize = buttonIconFontSize,
                                                    HorizontalOptions = LayoutOptions.Center,
                                                    VerticalOptions = LayoutOptions.Center,
                                                }
                                                .Assign(out _modeButtonIcon)
                                            },
                                        }
                                        .Assign(out _modeButton)
                                        .OnTapped(me =>
                                        {
                                            lock (lockNav)
                                            {
                                                if (IsBusy)
                                                {
                                                    return;
                                                }

                                                IsBusy = true;

                                                var fx = new TransitionEffect
                                                { 
                                               
                                                    //ShaderSource = @"Shaders\transition_circleopen.sksl", //ellipse cat's eye
                                                    //ShaderSource = @"Shaders\transition_circlecrop.sksl",
                                                    //ShaderSource = @"Shaders\transition_iris.sksl",

                                                    ShaderSource = @"Shaders\transition_ripple.sksl",
                                                    //ShaderSource = @"Shaders\transition_swirl.sksl",
                                                    //ShaderSource = @"Shaders\transition_zoom.sksl",
                                               
                                                    //ShaderSource = @"Shaders\transition_pixelize.sksl", //ERROR
                                               
                                                    //ShaderSource = @"Shaders\transition_fadecolor.sksl",
                                                    //ShaderSource = @"Shaders\transition_colorphase.sksl",
                                                    DurationMs = 600
                                                };
                                                fx.Midpoint  += (s, e) =>
                                                {
                                                    ToggleVisualizerMode();
                                                    fx.AquiredBackground = false;
                                                };
                                                fx.Completed += (s, e) =>
                                                {
                                                    _mainStack.VisualEffects.Remove(fx);
                                                    _mainStack.DisposeObject(fx);

                                                    IsBusy = false;
                                                };

                                                _mainStack.VisualEffects.Add(fx);
                                                fx.Play();
                                            }


                                        }),

                                        // Settings Button
                                        new TintedShape()
                                        {
                                            UseCache = SkiaCacheType.Image,
                                            TintColor = Colors.DarkCyan,
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
                                                    FontSize = buttonIconFontSize,
                                                    HorizontalOptions = LayoutOptions.Center,
                                                    VerticalOptions = LayoutOptions.Center,
                                                },
                                            },
                                        }
                                        .OnTapped(me =>
                                        {
                                            _settingsPopup?.Show();
                                        }),

                                        // Help Button
                                        new TintedShape()
                                        {
                                            UseCache = SkiaCacheType.Image,
                                            //TintColor = Colors.Orange,
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
                                                    FontSize = buttonIconFontSize,
                                                    HorizontalOptions = LayoutOptions.Center,
                                                    VerticalOptions = LayoutOptions.Center,
                                                },
                                            },
                                        }
                                        .OnTapped(me =>
                                        {
                                            _helpPopup?.Show();
                                            //TriggerCelebration();
                                        }),

                       
                                    }
                                }
                            },
                        },

                        new SettingsPopup(this)
                        {
                            IsVisible=false,
                        }.Assign(out _settingsPopup),

                        new HelpPopup()
                        {
                            IsVisible=false,
                        }.Assign(out _helpPopup)
                    }
            };
        }

        // Main layer that contains both the main stack and preview overlay
        var rootLayer = new SkiaLayer
        {
            VerticalOptions = LayoutOptions.Fill,
            Children =
            {
                _mainStack,
                _previewOverlay,

                // Achievement banner — floats above everything, fades out automatically
                new SkiaShape()
                {
                    ZIndex = 200,
                    IsVisible = false,
                    CornerRadius = 24,
                    BackgroundColor = Color.FromArgb("#EE000000"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Padding = new(32, 16),
                    Children =
                    {
                        new SkiaLabel()
                        {
                            FontSize = 28,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Colors.White,
                            HorizontalOptions = LayoutOptions.Center,
                        }.Assign(out _achievementBannerLabel)
                    }
                }.Assign(out _achievementBanner),

#if DEBUG
                new SkiaLabelFps()
                {
                    Margin = new(0, 0, 4, 24),
                    ForceRefresh = true,
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
            Gestures = GesturesMode.Lock,
            Content = rootLayer,
        };

        Canvas.WillFirstTimeDraw += (sender, context) =>
        {
            if (Recorder != null)
            {
                Tasks.StartDelayed(TimeSpan.FromMilliseconds(500), () =>
                {
                    Recorder.IsOn = true;
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

        AttachNotesEvents();
        InitAchievements();

        ToggleVisualizerMode(UserSettings.Current.Module);
    }

    private void AttachNotesEvents()
    {
        if (NotesModule == null)
            return;

        NotesModule.NoteChangedDelegate = NotesModuleOnNoteChanged;
        NotesModule.SequenceDetectedDelegate = NotesModuleOnSequenceDetected;
    }

    private static string ToNote(char letter)
    {
        return char.ToUpperInvariant(letter) switch
        {
            'C' => "Do",
            'D' => "Re",
            'E' => "Mi",
            'F' => "Fa",
            'G' => "Sol",
            'A' => "La",
            'B' => "Si",
            _ => "?"
        };
    }

    private void NotesModuleOnNoteChanged(int midiNote, char naturalLetter, float frequencyHz)
    {
        Debug.WriteLine($"[Notes] midi={midiNote} ({naturalLetter} = {ToNote(naturalLetter)}) {frequencyHz:F1}Hz");
    }

    private void NotesModuleOnSequenceDetected(NoteSequenceEventKind kind, int consecutiveNotes, ReadOnlySpan<char> lastLetters, ReadOnlySpan<int> lastMidiNotes)
    {
        var letters = new string(lastLetters);
        Debug.WriteLine($"[NotesSequence] {kind} streak={consecutiveNotes} letters={letters}");

        _achievementEngine?.Process(kind);
    }

    protected void LauchTimerStopEffect(int seconds)
    {
        if (TimerUpdateLocked == null)
        {
            TimerUpdateLocked = new RestartingTimer<object>(TimeSpan.FromSeconds(seconds),
                (args) =>
                {
                    _mainStack.EnableConfetti(false);
                });
            TimerUpdateLocked.Start(null);
        }
        else
        {
            TimerUpdateLocked.Restart(null);
        }
    }

    protected RestartingTimer<object> TimerUpdateLocked;

    private void ShowAchievementBanner(string name)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            // Cancel any banner already fading
            _bannerCts?.Cancel();
            _bannerCts = new CancellationTokenSource();
            var cts = _bannerCts;

            _achievementBannerLabel.Text = name;
            _achievementBanner.Opacity = 1;
            _achievementBanner.IsVisible = true;

            try
            {
                await Task.Delay(2000, cts.Token);
                await _achievementBanner.FadeToAsync(0, 600, Easing.CubicOut, cts);
                _achievementBanner.IsVisible = false;
            }
            catch (OperationCanceledException)
            {
                // A new achievement interrupted — it will take over visibility
            }
        });
    }

    private void InitAchievements()
    {
        _achievementEngine = new AchievementEngine();

        // Local helper keeps Name in one place and wires the banner automatically
        void Register(NoteSequenceEventKind trigger, string name, Action visualAction)
        {
            _achievementEngine.Register(new AchievementDefinition
            {
                Trigger = trigger,
                Name = name,
                OnAchieved = () =>
                {
                    ShowAchievementBanner(name);
                    visualAction();
                }
            });
        }

        Register(NoteSequenceEventKind.ThreeNoteRunAscending, "Going Up!",
            () =>
            {
                _mainStack.EnableConfetti(true);
                LauchTimerStopEffect(2);
            });

        Register(NoteSequenceEventKind.ThreeNoteRunDescending, "Scale Down!",
            () =>
            {
                _mainStack.EnableConfetti(true);
                LauchTimerStopEffect(2);
            });

        Register(NoteSequenceEventKind.SevenConsecutiveNotes, "7-Note Streak!",
            () =>
            {
                _mainStack.EnableConfetti(true);
                LauchTimerStopEffect(4);
            });

        Register(NoteSequenceEventKind.FourteenConsecutiveNotes, "Perfect Streak!",
            () =>
            {
                _mainStack.EnableConfetti(true);
                LauchTimerStopEffect(6);
                TriggerCelebration();
            });
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
        if (_background == null)
            return;

        var fx = new CelebrationEffect
        {
            UseContext = true,
            UseBackground = PostRendererEffectUseBackgroud.Never,
            BlendMode = SKBlendMode.Plus,
            ShaderSource = @"Shaders\celebrate_starburst.sksl",
            //DurationMs = 2500,
            //ShaderSource = @"Shaders\celebrate_waves.sksl",
            Center = new SKPoint(0.5f, 0.33f),
            DurationMs = 5000,
            //ShaderSource = @"Shaders\celebrate_confetti.sksl",
            //DurationMs = 3500,
            //ShaderSource = @"Shaders\celebrate_confetti_burst.sksl",
            //DurationMs = 3500,
            //ShaderSource = @"Shaders\celebrate_fireworks.sksl",
            //DurationMs = 3500,
            //ShaderSource = @"Shaders\celebrate_fireworks_launch.sksl",
            //DurationMs = 4000,
            //ShaderSource = @"Shaders\celebrate_fireworks_bloom.sksl",
            //DurationMs = 4000,
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
            _mainStack.VisualEffects.Remove(fx);
            _mainStack.DisposeObject(fx);
            System.Diagnostics.Debug.WriteLine($"Stopped effect {fx.ShaderSource}");
        };

        _mainStack.VisualEffects.Add(fx);
        System.Diagnostics.Debug.WriteLine($"Started effect {fx.ShaderSource}");
        fx.Play();
    }

    public void ShowAlert(string title, string message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await DisplayAlert(title, message, ResStrings.BtnOk);
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
            // Cycle through modes 
            _currentMode = (_currentMode + 1) % 2;
        }

        if (_background == null)
        {
            return;
        }

        switch (_currentMode)
        {
        case 0: // Notes
        _musicBPMDetectorWrapper.IsVisible = false;
        _musicNotesWrapper.IsVisible = true;
        //_modeButton.TintColor = Colors.CornflowerBlue;
        _modeButtonIcon.Text = IconFont.PlaylistMusic;//"🎵";
        _background.ToggleImage(0);

        break;
        case 1:
        _musicNotesWrapper.IsVisible = false;
        _musicBPMDetectorWrapper.IsVisible = true;
        //_modeButton.TintColor = Colors.DeepSkyBlue;
        _modeButtonIcon.Text = IconFont.TimerMusic;//IconFont.Music;//"🎼";
        _background.ToggleImage(1);
        break;
        }

        if (oldMode != _currentMode)
        {
            UserSettings.Current.Module = _currentMode;
            UserSettings.Save();
        }


    }

}