using System.Reflection;

namespace SolTempo
{
    public partial class App : Application
    {
        public App()
        {
            Super.SetLocale("en"); //todo add localization support

            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            base.OnStart();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                DeviceDisplay.Current.KeepScreenOn = true;
            });

        }


        protected override void OnResume()
        {
            base.OnResume();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                DeviceDisplay.Current.KeepScreenOn = true;
            });

        }

        protected override void OnSleep()
        {
            base.OnSleep();

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
