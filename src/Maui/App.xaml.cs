using Plugin.Maui.AppRating;
using SolTempo.Helpers;
using SolTempo.UI;
using System.Reflection;

namespace SolTempo
{
    public partial class App : Application
    {
        private DateTime? _sessionStart;

        public App()
        {
            Super.SetLocale("en"); //todo add localization support

            InitializeComponent();

#if ANDROID
            Super.SetNavigationBarColor(Colors.Black, Colors.Black, false);    
#endif

        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            return new Window(new AudioPage());
        }

        void StartSession()
        {
            _sessionStart = DateTime.UtcNow;
        }

        void EndSession()
        {
            if (_sessionStart.HasValue)
            {
                var elapsed = (long)(DateTime.UtcNow - _sessionStart.Value).TotalSeconds;
                UserSettings.Current.TotalUsageSeconds += elapsed;
                UserSettings.Save();
                _sessionStart = null;
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            StartSession();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                DeviceDisplay.Current.KeepScreenOn = true;
            });
        }


        protected override void OnResume()
        {
            base.OnResume();
            StartSession();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                DeviceDisplay.Current.KeepScreenOn = true;
            });
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            EndSession();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                DeviceDisplay.Current.KeepScreenOn = false;
            });
        }

        public void SetMainPage(Page page)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                MainPage = page;
            });
        }

        public static App Instance => App.Current as App;
    }




}
