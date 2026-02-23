using MusicNotes.Audio;
using MusicNotes.Helpers;
using MusicNotes.UI;


namespace ShadersCamera.Views;

public partial class SettingsPopup : AnimatedPopup
{
    private readonly AudioPage _parentPage;

    private bool isInitializing;

    public override void Show()
    {
        _ = UpdateControls();

        base.Show();
    }

    public override void Close()
    {
        base.Close();

        UserSettings.FillFromHardware(Recorder);
        UserSettings.FillFromNotes(Notes);
        UserSettings.Save();
    }

    AudioRecorder Recorder => _parentPage?.Recorder;

    AudioInstrumentTuner Notes => _parentPage?.NotesModule;

    public SettingsPopup(AudioPage page)
    {
        InitializeComponent();

        SetAnimatable(Animated);

        _parentPage = page;

        _ = UpdateControls();
    }




    public override void OnWillDisposeWithChildren()
    {
        base.OnWillDisposeWithChildren();

        _exit?.Dispose();
        _exit = null;
        _entrance?.Dispose();
        _entrance = null;
    }


    async Task<string> GetAudioSourceText(AudioRecorder CameraControl)
    {
        if (CameraControl.AudioDeviceIndex >= 0)
        {
            var arrayDevices = await CameraControl.GetAvailableAudioDevicesAsync();
            if (arrayDevices.Count > 0)
            {
                var device = arrayDevices[CameraControl.AudioDeviceIndex];
                return $"{device}";
            }
        }

        return "Default";
    }

    string GetNotationText(int value)
    {
        switch (value)
        {
        case 0:
        return "Letters";
        case 1:
        return "European";
        case 2:
        return "American";
        case 3:
        return "Cyrillic";
        case 4:
        return "Numbers";
        default:
        return "Default";
        }
    }

    async Task UpdateControls()
    {
        isInitializing = true;

        //switches
        SwitchGain.IsToggled = Recorder.UseGain;

        //selectors
        LabelSource.Text = await GetAudioSourceText(Recorder);

        SwitchSemi.IsToggled = Notes.UseSemiNotes;
        LabelNotation.Text = GetNotationText(Notes.Notation);
        LabelNotesMode.Text = GetVoiceModeText(Notes.VoiceMode ? 0 : 1);

        isInitializing = false;
    }


    private void UseGainSwitch_OnToggled(object sender, bool value)
    {
        if (isInitializing)
            return;

        Recorder.UseGain = value;
    }



    private void SelectAudioSource(object sender, ControlTappedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                var inputDevices = await Recorder.GetAvailableAudioDevicesAsync();

                if (inputDevices?.Count > 0)
                {
                    // Prefix the list with "System Default" option
                    var options = new string[inputDevices.Count + 1];
                    options[0] = "System Default";
                    for (int i = 0; i < inputDevices.Count; i++)
                    {
                        options[i + 1] = inputDevices[i];
                    }

                    var result = await _parentPage.DisplayActionSheet("Select Audio Source", "Cancel", null, options);

                    if (!string.IsNullOrEmpty(result) && result != "Cancel")
                    {
                        if (result == "System Default")
                        {
                            Recorder.AudioDeviceIndex = -1;
                        }
                        else
                        {
                            // Find the index in our original list
                            // Careful: option list was shifted by 1
                            int selectedIndex = -1;
                            for (int i = 0; i < inputDevices.Count; i++)
                            {
                                if (inputDevices[i] == result)
                                {
                                    selectedIndex = i;
                                    break;
                                }
                            }

                            if (selectedIndex >= 0)
                            {
                                Recorder.AudioDeviceIndex = selectedIndex;
                            }
                        }

                        LabelSource.Text = result;
                    }
                }
                else
                {
                    _parentPage.ShowAlert("No Input Devices", "No audio input devices found.");
                }
            }
            catch (Exception ex)
            {
                _parentPage.ShowAlert("Error", $"Error getting audio devices: {ex.Message}");
            }
        });
    }

    private void AudioPageSettings_OnTapped(object sender, ControlTappedEventArgs e)
    {
        Close();
    }

    private void UseSemiSwitch_OnToggled(object sender, bool value)
    {
        if (isInitializing)
            return;

        Notes.UseSemiNotes = value;
    }

    private void SelectNotation(object sender, ControlTappedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {

                {
                    // Prefix the list with "System Default" option
                    var options = new string[5];
                    for (int i = 0; i < 5; i++)
                    {
                        options[i] = GetNotationText(i);
                    }

                    var result = await _parentPage.DisplayActionSheet("Select Notation", "Cancel", null, options);

                    if (!string.IsNullOrEmpty(result) && result != "Cancel")
                    {
                        int selectedIndex = -1;
                        for (int i = 0; i < options.Length; i++)
                        {
                            if (options[i] == result)
                            {
                                selectedIndex = i;
                                break;
                            }
                        }

                        if (selectedIndex >= 0)
                        {
                            Notes.Notation = selectedIndex;
                        }

                        LabelNotation.Text = result;
                        
                        Notes.Reset();
                    }
                }
            }
            catch (Exception ex)
            {
                Super.Log(ex);
            }
        });
    }

    string GetVoiceModeText(int value)
    {
        switch (value)
        {
            case 1:
                return "Voice 80–1100 Hz";

            default:
            case 0:
                return "Instruments 60–1600 Hz";
        }
    }

    private void SelectNotesMode(object sender, ControlTappedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {

                {
                    // Prefix the list with "System Default" option
                    var options = new string[2];
                    options[0] = GetVoiceModeText(0);
                    options[1] = GetVoiceModeText(1);

                    var result = await _parentPage.DisplayActionSheet("Select Detection Mode", "Cancel", null, options);

                    if (!string.IsNullOrEmpty(result) && result != "Cancel")
                    {
                        int selectedIndex = -1;
                        for (int i = 0; i < options.Length; i++)
                        {
                            if (options[i] == result)
                            {
                                selectedIndex = i;
                                break;
                            }
                        }

                        if (selectedIndex >= 0)
                        {
                            Notes.VoiceMode = selectedIndex == 0;
                        }

                        LabelNotesMode.Text = result;
                    }
                }
            }
            catch (Exception ex)
            {
                Super.Log(ex);
            }
        });
    }

    private void TappedClose(object sender, ControlTappedEventArgs e)
    {
        Close();
    }

}
