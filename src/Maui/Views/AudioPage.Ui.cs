using AppoMobi.Specials;
using DrawnUi.Controls;
using DrawnUi.Infrastructure;
using DrawnUi.Models;
using DrawnUi.Views;
using Plugin.Maui.AppRating;
using SolTempo.Audio;
using SolTempo.Effects;
using SolTempo.Helpers;
using SolTempo.Resources.Strings;
using System.Diagnostics;

namespace SolTempo.UI;

public partial class AudioPage
{
    Canvas Canvas;

    private MutliImages _background;
    private SkiaShape _takePictureButton;
    private SkiaLabel _statusLabel;
    private SkiaLayer _previewOverlay;
    private SkiaImage _previewImage;
    private SkiaImage _previewThumbnail;
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
    private SkiaShaderEffect shaderGlass;
    private Achievements _achievementEngine;
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
        ReleaseTreeState();

        Canvas?.Dispose();

        CreateContent();
    }

    /// <summary>
    /// What belongs to the current drawn tree and must not outlive it: the audio hardware,
    /// the confetti timer and the banner fade.
    /// </summary>
    private void ReleaseTreeState()
    {
        if (Recorder != null)
        {
            Recorder?.Stop();
            AttachHardware(false);
            Recorder = null;
        }

        TimerUpdateLocked?.Dispose();
        TimerUpdateLocked = null;
        _bannerCts?.Cancel();
        _bannerCts = null;
    }

    /// <summary>
    /// The language changed in Settings: keep what the user set, build the page again with the new words
    /// and fonts, and land back in Settings.
    /// </summary>
    public void RebuildForLanguage()
    {
        if (Recorder != null)
            UserSettings.FillFromHardware(Recorder);
        if (NotesModule != null)
            UserSettings.FillFromNotes(NotesModule);
        UserSettings.Save();

        ReleaseTreeState();
        CreateContent(); // keeps the canvas, replaces its drawn tree

        // Settings reopens once the new tree has been laid out and drawn
        void ShowSettings(object sender, SkiaDrawingContext? context)
        {
            Canvas.WasDrawn -= ShowSettings;
            MainThread.BeginInvokeOnMainThread(() => _settingsPopup?.Show());
        }
        Canvas.WasDrawn += ShowSettings;
    }

    public AudioPage()
    {
        Title = ResStrings.AppTitle;

        //iOS statusbar and bottom insets color
        BackgroundColor = Colors.Black;
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
                            UseCache = SkiaCacheType.None, //glass backdrop inside must copy what is behind on every frame
                            WidthRequest=900,
                            HorizontalOptions = LayoutOptions.Center,
                            Margin = new (16,16,16,0),
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
                                            EdgeOpacity = 0.55f,
                                            EdgeGlow = 0.95f,
                                            Emboss = 9.2f,
                                            BlurStrength = 1.0f,
                                            Opacity = 0.9f,
                                            Tint = Colors.Black.WithAlpha(0.33f),
                                            CornerRadius = 24,
                                            Depth = 1.66f
                                        }.Assign(out shaderGlass) //for dev shader editor
                                    }
                                },

                                new AudioNotesDetector()
                                {
                                    Margin = new (16,16,16,0),
                                    HorizontalOptions = LayoutOptions.Fill,
                                    VerticalOptions = LayoutOptions.Fill,
                                }
                                .Assign(out NotesModule)
                                // one spoken node for the card, updated only for a steady note (AnnounceNote)
                                .WithAccessibility(Aria.RoleStatus, ResStrings.Listening, hint: null)
                                .WithAccessibilityLive(),

                            }
                        }.Assign(out _musicNotesWrapper),

                        // BPM MUSIC
                        new SkiaShape()
                        {
                            IsVisible=false,
                            UseCache = SkiaCacheType.None, //glass backdrop inside must copy what is behind on every frame
                            WidthRequest=900,
                            Margin = new (16,16,16,0),
                            HorizontalOptions = LayoutOptions.Center,
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
                                            Emboss = 9.2f,
                                            BlurStrength = 1.0f,
                                            Opacity = 0.9f,
                                            Tint = Colors.Black.WithAlpha(0.33f),
                                            CornerRadius = 24,
                                            Depth = 1.66f
                                        }
                                    }
                                },

                                new AudioMusicBPM()
                                {
                                    Margin = new (6,16,6,6),
                                    HorizontalOptions = LayoutOptions.Fill,
                                    VerticalOptions = LayoutOptions.Fill,
                                }
                                .Assign(out _musicBPMDetector)
                                // one spoken node for the card, updated only for a settled tempo (AudioMusicBPM)
                                .WithAccessibility(Aria.RoleStatus, ResStrings.Listening, hint: null)
                                .WithAccessibilityLive(),

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
                            UseCache = SkiaCacheType.None, //glass backdrop over the moving equalizer must stay live
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
                                            EdgeOpacity = 0.58f,
                                            EdgeGlow = 0.85f,
                                            Depth = 1.66f,
                                            Emboss = 18.5f, //affects border width
                                            BlurStrength = 1.0f,
                                            Opacity = 0.9f,
                                            Tint = Colors.White.WithAlpha(0.05f),
                                            CornerRadius = 32,
                                        }//.Assign(out shaderGlass) //for dev shader editor
                                    }
                                },

                                new SkiaRow()
                                {
                                    UseCache = SkiaCacheType.GPU,
                                    Padding = new Thickness(20, 12),
                                    Spacing = 16,
                                    HorizontalOptions = LayoutOptions.Center,
                                    VerticalOptions = LayoutOptions.Center,
                                    Children =
                                    {
                                        // Mode Button
                                        new TintedShape()
                                        {
                                            TintColor = Colors.CornflowerBlue,
                                            UseCache = SkiaCacheType.OperationsFull,
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
                                        .WithAccessibilityButton(ResStrings.A11yToBpm) // label follows the module, see ToggleVisualizerMode
                                        .OnTapped(me =>
                                        {
                                            TappedSwitchModules();
                                        }),

                                        // Settings Button
                                        new TintedShape()
                                        {
                                            UseCache = SkiaCacheType.OperationsFull,
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
                                        .WithAccessibilityButton(ResStrings.Settings)
                                        .OnTapped(me =>
                                        {
                                            TappedSettings();
                                        }),

                                        // Help Button
                                        new TintedShape()
                                        {
                                            UseCache = SkiaCacheType.OperationsFull,
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
                                        .WithAccessibilityButton(ResStrings.Help)
                                        .OnTapped(me =>
                                        {
                                            TappedHelp();
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
                    BackgroundColor = Color.FromArgb("#99000000"),
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
                    //ForceRefresh = true,
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

        if (Canvas != null && !Canvas.IsDisposed)
        {
            // language switch: the canvas and its GPU surface stay, only the drawn tree is replaced.
            // The old tree is released by the canvas at the end of a frame, while its GPU context is alive.
            var oldTree = Canvas.Content;
            Canvas.Content = rootLayer;
            if (oldTree != null)
                Canvas.DisposeObject(oldTree);

            AfterTreeCreated();
            StartHardware();
            return;
        }

        Canvas = new Canvas
        {
            RenderingMode = RenderingModeType.Accelerated,
            Gestures = GesturesMode.Lock,
            Content = rootLayer,
        };

        Canvas.WillFirstTimeDraw += (sender, context) =>
        {
            StartHardware();
        };

        Content = new Grid() //due to maui layout specifics we are forced to use a Grid as root wrapper
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            Children = { Canvas }
        };

        AfterTreeCreated();
    }

    private void AfterTreeCreated()
    {
        if (Recorder != null)
        {
            // Setup SkiaCamera event handlers and apply user settings to it
            AttachHardware(true);
        }

        AttachNotesEvents();
        InitAchievements();

        ToggleVisualizerMode(UserSettings.Current.Module);
    }

    private void StartHardware()
    {
        if (Recorder != null)
        {
            Tasks.StartDelayed(TimeSpan.FromMilliseconds(500), () =>
            {
                Recorder.IsOn = true;

                //for fast use in transitions
                SkSl.Precompile(@"Shaders\transition_ripple.sksl", @"Shaders\appear_orb.sksl", @"Shaders\exit_orb.sksl");

#if ANDROID || IOS
                if (UserSettings.Current.TotalUsageSeconds >= 3600 && !UserSettings.Current.RatingRequested)
                {
                    UserSettings.Current.RatingRequested = true;
                    UserSettings.Save();
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        try
                        {
                            await AppRating.Default.PerformInAppRateAsync();
                        }
                        catch (Exception ex)
                        {
                            Super.Log(ex);
                        }
                    });
                }
#endif

            });
        }
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

        AnnounceNote();
    }

    private bool _noteAnnouncePending;
    private string _noteAnnounced;

    /// <summary>
    /// Screen readers hear the note only once it is held: the card's spoken label is set 600 ms after a
    /// note change, from the note shown then, and only when it differs from the last one spoken.
    /// Notes changing faster than that are never spoken, so the reader is not flooded.
    /// </summary>
    private void AnnounceNote()
    {
        if (_noteAnnouncePending)
            return;

        _noteAnnouncePending = true;
        Tasks.StartDelayed(TimeSpan.FromMilliseconds(600), () =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _noteAnnouncePending = false;

                var notes = NotesModule;
                var name = notes?.DetectedNoteName;
                if (string.IsNullOrEmpty(name) || !notes.HasSignal)
                    return;

                var spoken = name.Replace("#", " " + ResStrings.A11ySharp);
                var cents = notes.DetectedCents;
                if (MathF.Abs(cents) <= 50) // tuning is only meaningful within half a semitone of the note
                    spoken += ", " + string.Format(ResStrings.Cents, $"{cents:+0;-0}");
                if (spoken == _noteAnnounced)
                    return;

                _noteAnnounced = spoken;
                notes.AccessibilityLabel = spoken;
                notes.NotifyAccessibility();
            });
        });
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

    private async void ShowAchievementBanner(string name)
    {
        Debug.WriteLine($"IShowAchievementBanner {name}");

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
            // A new achievement interrupted  
            Debug.WriteLine("Interrupted!!!");
        }
    }

    private void InitAchievements()
    {
        _achievementEngine = new Achievements();

        // Local helper keeps Name in one place and wires the banner automatically
        void Register(NoteSequenceEventKind trigger, string name, Action visualAction)
        {
            _achievementEngine.Register(new Achievement
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

        //Register(NoteSequenceEventKind.FourNoteRunAscending, "Rising Well!",
        //    () =>
        //    {

        //    });

        //Register(NoteSequenceEventKind.FourNoteRunDescending, "Descending Well!",
        //    () =>
        //    {

        //    });

        Register(NoteSequenceEventKind.SevenConsecutiveNotes, ResStrings.SequenceOctave,
            () =>
            {
                PlayConfetti();
            });

        Register(NoteSequenceEventKind.FourteenConsecutiveNotes, ResStrings.SequenceTwoOctaves,
            () =>
            {
                PlayAchievementEffect();
            });
    }

    public void PlayConfetti()
    {
        _mainStack.EnableConfetti(true);
        LauchTimerStopEffect(6);
    }

    /// <summary>
    /// Plays an animated shader 
    /// </summary>
    public void PlayAchievementEffect()
    {
        if (_background == null)
            return;

        var fx = new AchievementEffect
        {
        };

        fx.Completed += (s, e) =>
        {
            _background.VisualEffects.Remove(fx);
            _background.DisposeObject(fx); //need to dispose manually
        };

        _background.VisualEffects.Add(fx);
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

        // the mode button says where it takes you
        _modeButton.AccessibilityLabel = _currentMode == 0 ? ResStrings.A11yToBpm : ResStrings.A11yToNotes;
        _modeButton.NotifyAccessibility();

        if (oldMode != _currentMode)
        {
            UserSettings.Current.Module = _currentMode;
            UserSettings.Save();
        }


    }

}