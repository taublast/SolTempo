using SolTempo.Resources.Strings;
using System.Globalization;

namespace SolTempo.Helpers
{
    /// <summary>
    /// The UI language: the one picked in Settings, else the device language when the app ships it,
    /// else English. Words come from ResStrings. A switch at runtime rebuilds the page
    /// (AudioPage.RebuildForLanguage), so every label is created again with the new words.
    /// Fonts never change per language: labels showing translated words carry
    /// FontFamilyFallback = AppFonts.DefaultLatin for the letters Neucha lacks (sr, it, pt).
    /// </summary>
    public static class AppLanguage
    {
        /// <summary>Languages the app ships, each named in itself.</summary>
        public static readonly (string Code, string Name)[] Enabled =
        {
            ("en", "English"), ("ru", "Русский"), ("fr", "Français"), ("de", "Deutsch"),
            ("es", "Español"), ("it", "Italiano"), ("pt", "Português"), ("sr", "Srpski"),
        };

        /// <summary>
        /// Culture behind a code where the bare code is not the one shipped: Serbian ships in Latin script
        /// (ResStrings.sr-Latn), Portuguese as Brazilian (ResStrings.pt through pt-BR).
        /// </summary>
        private static string CultureOf(string lang) => lang switch
        {
            "sr" => "sr-Latn-RS",
            "pt" => "pt-BR",
            _ => lang,
        };

        /// <summary>Code of the language the UI is in now.</summary>
        public static string Current { get; private set; } = "en";

        /// <summary>The current language named in itself, for the Settings row.</summary>
        public static string CurrentName => Enabled.First(x => x.Code == Current).Name;

        /// <summary>At startup, before any UI: the language picked in Settings, else the device's.</summary>
        public static void ApplySaved()
        {
            var settings = UserSettings.Current;
            Set(settings.LangChosen ? settings.Lang : DeviceLanguage());
        }

        /// <summary>Picked in Settings: remembered and applied. The caller rebuilds the UI.</summary>
        public static void Choose(string lang)
        {
            UserSettings.Current.LangChosen = true;
            Set(lang);
            UserSettings.Save();
        }

        /// <summary>Two-letter code of the device language when the app ships it, else English.</summary>
        public static string DeviceLanguage()
        {
            var device = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            return Enabled.Any(x => x.Code == device) ? device : Enabled[0].Code;
        }

        private static void Set(string lang)
        {
            if (!Enabled.Any(x => x.Code == lang))
                lang = Enabled[0].Code;

            Current = lang;
            UserSettings.Current.Lang = lang;

            // ResStrings.Culture answers on every thread; Super.SetLocale sets the calling one only
            ResStrings.Culture = CultureInfo.CreateSpecificCulture(CultureOf(lang));
            Super.SetLocale(CultureOf(lang));
        }
    }
}
