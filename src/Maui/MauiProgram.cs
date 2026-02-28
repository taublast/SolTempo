global using DrawnUi.Draw;
global using SkiaSharp;
using Microsoft.Extensions.Logging;

namespace SolTempo
{
    public static class AppFonts
    {
        public static string Icons = "FontIcons";
        public static string Emogi = "FontEmoji";
        public static string Default = "FontText";
        public static string Bold = "FontTextBold";
        public static string Title = "FontTextTitle";


    }
    public static class MauiProgram
    {

        //LOTTIE anims for permissions are by https://lottiefiles.com/madhu

        public static MauiApp CreateMauiApp()
        {
            //SkiaImageManager.CacheLongevitySecs = 10;
            //SkiaImageManager.LogEnabled = true;

            Super.NavBarHeight = 47;

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("material.ttf", AppFonts.Icons);

                    fonts.AddFont("NotoColorEmoji-Regular.ttf", AppFonts.Emogi);

                    fonts.AddFont("Neucha-Regular.ttf", AppFonts.Default);
                    fonts.AddFont("ShantellSans-Bold.ttf", AppFonts.Bold);
                    fonts.AddFont("NotoSansDisplay-Regular.ttf", AppFonts.Title);
 
                });

            builder.UseDrawnUi(new()
            {
                //portrait
                DesktopWindow = new()
                {
                    Height = 800,
                    Width = 375,
                }

                //landscape
                //DesktopWindow = new()
                //{
                //    Height = 500,
                //    Width = 750,
                //}
            });


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        public static string Build
        {
            get
            {
                return $"Build {AppInfo.Current.BuildString}";
            }
        }
    }
}
