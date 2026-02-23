using System.Globalization;

namespace ShadersCamera.Views;

 

public partial class HelpPopup : AnimatedPopup
{
    public HelpPopup()
    {
        InitializeComponent();
        
        SetAnimatable(Animated);

        _ = LoadHelpContent();
    }

 

    private async Task LoadHelpContent()
    {
        try
        {
            var lang = "en";// CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var content = await TryLoadFile($"help.{lang}.md");

            if (content == null && lang != "en")
            {
                content = await TryLoadFile("help.en.md");
            }

            if (content != null)
            {
                HelpContent.Text = content;
            }
        }
        catch (Exception ex)
        {
            Super.Log(ex);
        }
    }

    private static async Task<string> TryLoadFile(string filename)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(filename);
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
        catch
        {
            return null;
        }
    }

    private void OnBackgroundTapped(object sender, ControlTappedEventArgs e)
    {
        Close();
    }

    public override void OnWillDisposeWithChildren()
    {
        base.OnWillDisposeWithChildren();

        _exit?.Dispose();
        _exit = null;
        _entrance?.Dispose();
        _entrance = null;
    }
}
