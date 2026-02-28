using DrawnUi.Camera;
using DrawnUi.Views;
using SolTempo.Audio;
using SolTempo.Effects;
using SolTempo.Helpers;
using SolTempo.Resources.Strings;
using System.Diagnostics;
 

namespace SolTempo.UI;

public partial class AudioPage : BasePageReloadable, IDisposable
{
    public AudioRecorder Recorder;

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (Recorder != null && Recorder.IsOn)
        {
            Recorder.Stop();
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (Recorder != null && Recorder.IsOn)
        {
            Recorder.Start();
        }

        _equalizer?.Reset();
    }

    private void AttachHardware(bool subscribe)
    {
        if (subscribe)
        {
            AttachHardware(false); //fool proof

            Recorder.StateChanged += RecorderOnStateChanged;
            Recorder.OnError += OnHardwareError;
            Recorder.RecordingSuccess += OnRecordingSuccess;
            Recorder.RecordingProgress += OnRecordingProgress;

            Recorder.OnAudioSample += OnAudioSample;

            UserSettings.ApplyToHardware(Recorder);
            UserSettings.ApplyToNotes(NotesModule);
        }
        else
        {
            if (Recorder != null)
            {
                Recorder.StateChanged -= RecorderOnStateChanged;
                Recorder.OnError -= OnHardwareError;
                Recorder.RecordingSuccess -= OnRecordingSuccess;
                Recorder.RecordingProgress -= OnRecordingProgress;

                Recorder.OnAudioSample -= OnAudioSample;
            }
        }
    }

    private long _lastAudioDispatchMs = 0;

    private void OnAudioSample(AudioSample sample)
    {
        // Single wall-clock gate
        //long nowMs = Environment.TickCount64;
        //if (nowMs - _lastAudioDispatchMs < 16)
        //    return;
        //_lastAudioDispatchMs = nowMs;

        if (_musicNotesWrapper.IsVisible)
            NotesModule.AddSample(sample);

        if (_musicBPMDetectorWrapper.IsVisible)
            _musicBPMDetector?.AddSample(sample);

        if (_equalizer.IsVisible)
            _equalizer.AddSample(sample);

    }
    

    private void RecorderOnStateChanged(object sender, HardwareState state)
    {
        if (state == HardwareState.On)
        {
           
        }
    }

    private int _lastAudioRate;
    private int _lastAudioBits;
    private int _lastAudioChannels;
    private bool _speechEnabled;

 
    private bool _btnStateIsRecording;
    private const int _morphSpeed = 250;

  

    private void OnHardwareError(object sender, string e)
    {
        ShowAlert("Hardware Error", e);
    }

    private async void OnRecordingSuccess(object sender, CapturedVideo capturedVideo)
    {
        try
        {
            Debug.WriteLine($"✅ Media recorded at: {capturedVideo.FilePath}");

            // Use SkiaCamera's built-in MoveVideoToGalleryAsync method (consistent with SaveToGalleryAsync for photos)
            var publicPath = await Recorder.MoveVideoToGalleryAsync(capturedVideo, "FastRepro");

            if (!string.IsNullOrEmpty(publicPath))
            {
                Debug.WriteLine($"✅ Media moved to gallery: {publicPath}");
                ShowAlert("Success", "Video saved to gallery!");
            }
            else
            {
                Debug.WriteLine($"❌ Media not saved, path null");
                ShowAlert("Error", "Failed to save video to gallery");
            }
        }
        catch (Exception ex)
        {
            ShowAlert("Error", $"Media save error: {ex.Message}");
            Debug.WriteLine($"❌ Media save error: {ex}");
        }
    }

    private void OnRecordingProgress(object sender, TimeSpan duration)
    {
        //one day maybe could add recording to this app, skiacamera support that np
    }

    private void ShowPreviewOverlay(SkiaSharp.SKImage image)
    {
        if (_previewImage != null && _previewOverlay != null)
        {
            // Set the captured image to the preview image control
            _previewImage.SetImageInternal(image, false);

            // Show the overlay
            _previewOverlay.IsVisible = true;
        }
    }
           
 

#if DEBUG

    private ShaderEditorPage _editor;
    private SkiaShaderEffect _editableShader;

    public void OpenShaderEditor(SkiaShaderEffect shader)
    {
        _editableShader = shader;
        if (_editableShader != null)
        {
            var code = shader.LoadedCode;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _editor = new ShaderEditorPage(code, ChangeShaderCode);
                OpenPageInNewWindow(_editor, "Shader Editor");
            });
        }
    }

    public void ChangeShaderCode(string code)
    {
        if (_editableShader==null)
        {
            return;
        }

        //do not load from file anymore
        _editableShader.ShaderSource = null;
        //set our own code
        _editableShader.ShaderCode = code;
    }

    public static void OpenPageInNewWindow(ContentPage page,
        string title = "Editor")
    {
#if WINDOWS || MACCATALYST
            var window = new Window(page) { Title = title };
            
            if (page is ShaderEditorPage)
            {
                window.Width = 800;
                window.Height = 800;
                window.MinimumWidth = 600;
                window.MinimumHeight = 400;
                
                var mainWindow = Application.Current?.Windows?.FirstOrDefault();
                if (mainWindow != null)
                {
                    window.X = mainWindow.X + mainWindow.Width + 20; 
                    window.Y = mainWindow.Y;  
                }
            }
            
            Application.Current.OpenWindow(window);
#endif
    }

#endif


    private void TappedSwitchModules()    
    {
        lock (lockNav)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            var fx = new TransitionEffect();
            fx.Midpoint += (s, e) =>
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
    }

    private void TappedSettings()
    {
        _settingsPopup?.Show();
#if DEBUG && WINDOWS
        OpenShaderEditor(shaderGlass);
#endif
    }

    private void TappedHelp()
    {
        //Demo();
        //return;
        
        _helpPopup?.Show();
    }

    void Demo()
    {
        NotesModule?.Demo();
        ShowAchievementBanner(ResStrings.SequenceTwoOctaves);
        PlayConfetti();
        PlayAchievementEffect();
    }
}
