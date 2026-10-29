using DrawnUi.Models;
using SolTempo.Helpers;
using SolTempo.Resources.Strings;
using System.Globalization;

namespace SolTempo.UI;

 

public partial class HelpPopup : AnimatedPopup
{
    public HelpPopup()
    {
        InitializeComponent();
        
        SetAnimatable(Animated);

        // screen readers: a heading, the help text as one text block, the two credit links, the close button
        LinkMaui.WithAccessibility(Aria.RoleLink, ".NET MAUI", hint: null);
        LinkDrawnUi.WithAccessibility(Aria.RoleLink, "DrawnUI", hint: null);
        BtnClose.WithAccessibilityButton(ResStrings.BtnClose);

        _ = LoadHelpContent();
    }

 

    private async Task LoadHelpContent()
    {
        try
        {
            var lang = AppLanguage.Current;
            var content = await TryLoadFile($"Markdown/help.{lang}.md");

            if (content == null && lang != "en")
            {
                content = await TryLoadFile("Markdown/help.en.md");
            }

            if (content != null)
            {
                HelpContent.Text = content;
                HelpContent.WithAccessibilityText(ToSpokenText(content));
                HelpContent.NotifyAccessibility();
            }
        }
        catch (Exception ex)
        {
            Super.Log(ex);
        }
    }

    /// <summary>
    /// The help is markdown; a screen reader must not read its marks aloud: headings and list marks,
    /// bold/italic stars, rules, and links keep only their text.
    /// </summary>
    private static string ToSpokenText(string markdown)
    {
        var text = System.Text.RegularExpressions.Regex.Replace(markdown, @"\[([^\]]+)\]\([^\)]*\)", "$1");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"(?m)^\s*(#+|-{3,}|[-*])\s?", "");
        return text.Replace("**", "").Replace("__", "");
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

    private void HelpContent_OnLinkTapped(object sender, string url)
    {
        OpenLink(url);
    }

    private void TappedDrawnUi(object sender, ControlTappedEventArgs e)
    {
        OpenLink("https://drawnui.net");
    }


    private void TappedMaui(object sender, ControlTappedEventArgs e)
    {
        OpenLink("https://dotnet.microsoft.com/en-us/apps/maui");
    }

    private void TappedClose(object sender, ControlTappedEventArgs e)
    {
        Close();
    }

    void OpenLink(string url)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await Launcher.Default.OpenAsync(url);
            }
            catch (Exception e)
            {
                Super.Log($"Open url `{url}` failed: {e}");
            }
        });
    }
}
