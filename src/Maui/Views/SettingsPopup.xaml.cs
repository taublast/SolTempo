using DrawnUi.Models;
using SolTempo;
using SolTempo.Audio;
using SolTempo.Helpers;
using SolTempo.Resources.Strings;
using SolTempo.UI;


namespace SolTempo.UI;

public partial class SettingsPopup : AnimatedPopup
{
    private readonly AudioPage _parentPage;

    private bool isInitializing; //to lock ui switches at startup

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

    AudioNotesDetector Notes => _parentPage?.NotesModule;

    public SettingsPopup(AudioPage page)
    {
        InitializeComponent();

        SetAnimatable(Animated);

        _parentPage = page;

        // screen readers: titles are headings, rows are buttons that say their value, switches say their purpose
        TitlePitch.WithAccessibility(Aria.RoleHeading, ResStrings.PitchSettings, hint: null);
        TitleAudio.WithAccessibility(Aria.RoleHeading, ResStrings.AudioSettings, hint: null);
        TitleVarious.WithAccessibility(Aria.RoleHeading, ResStrings.Various, hint: null);
        SwitchSemi.WithAccessibilityToggle(ResStrings.SharpNotes);
        SwitchGain.WithAccessibilityToggle(ResStrings.UseGain);
        RowShare.WithAccessibilityButton(ResStrings.ShareApp);
        RowPrivacy.WithAccessibilityButton(ResStrings.Privacy);
        BtnClose.WithAccessibilityButton(ResStrings.BtnClose);

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

        return ResStrings.SourceDefault;
    }

    string GetNotationText(int value)
    {
        switch (value)
        {
        case 0:
        return ResStrings.NotationLetters;
        case 1:
        return ResStrings.NotationEuropean;
        case 2:
        return ResStrings.NotationAmerican;
        case 3:
        return ResStrings.NotationCyrillic;
        case 4:
        return ResStrings.NotationNumbers;
        default:
        return ResStrings.SourceDefault;
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
        LabelLanguage.Text = AppLanguage.CurrentName;

        SetRowAccessibility(RowNotation, ResStrings.Notation, LabelNotation.Text);
        SetRowAccessibility(RowSource, ResStrings.Source, LabelSource.Text);
        SetRowAccessibility(RowLanguage, ResStrings.Language, LabelLanguage.Text);

        isInitializing = false;
    }


    /// <summary>A picker row is read as one button: "Notation: European".</summary>
    static void SetRowAccessibility(SkiaControl row, string name, string value)
    {
        row.WithAccessibilityButton($"{name}: {value}");
        row.NotifyAccessibility();
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
                    options[0] = ResStrings.SystemDefault;
                    for (int i = 0; i < inputDevices.Count; i++)
                    {
                        options[i + 1] = inputDevices[i];
                    }

                    var result = await _parentPage.DisplayActionSheet(ResStrings.SelectAudioSource, ResStrings.BtnCancel, null, options);

                    if (!string.IsNullOrEmpty(result) && result != ResStrings.BtnCancel)
                    {
                        if (result == ResStrings.SystemDefault)
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
                        SetRowAccessibility(RowSource, ResStrings.Source, result);
                    }
                }
                else
                {
                    _parentPage.ShowAlert(ResStrings.NoInputDevices, ResStrings.NoInputDevicesFound);
                }
            }
            catch (Exception ex)
            {
                _parentPage.ShowAlert(ResStrings.Error, string.Format(ResStrings.ErrorAudioDevices, ex.Message));
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

                    var result = await _parentPage.DisplayActionSheet(ResStrings.SelectNotation, ResStrings.BtnCancel, null, options);

                    if (!string.IsNullOrEmpty(result) && result != ResStrings.BtnCancel)
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
                        SetRowAccessibility(RowNotation, ResStrings.Notation, result);
                        
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
 
    private void TappedClose(object sender, ControlTappedEventArgs e)
    {
        Close();
    }

    private void TappedPrivacy(object sender, ControlTappedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await App.Instance.MainPage.DisplayAlert(
                ResStrings.PrivacyPolicy,
                ResStrings.PrivacyText,
                ResStrings.BtnOk);
        });
    }

    private void SelectLanguage(object sender, ControlTappedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                // alphabetical by the name each language has in itself: Latin ones A-Z, then Cyrillic
                var names = AppLanguage.Enabled.Select(x => x.Name).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var result = await _parentPage.DisplayActionSheet(ResStrings.SelectLanguage, ResStrings.BtnCancel, null, names);
                var picked = AppLanguage.Enabled.FirstOrDefault(x => x.Name == result).Code;
                if (picked == null || picked == AppLanguage.Current)
                    return;

                AppLanguage.Choose(picked);

                // every label was built with the old words and fonts: the page is built again and Settings reopens
                _parentPage.RebuildForLanguage();
            }
            catch (Exception ex)
            {
                Super.Log(ex);
            }
        });
    }

    private void TappedShareApp(object sender, ControlTappedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Share.RequestAsync(new ShareTextRequest
            {
                Title = ResStrings.AppTitle,
                Text = ResStrings.ShareText
            });
        });
    }
}
