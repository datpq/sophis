using System;
using Xamarin.Forms;
using ExpressoReporting.Services;
using ExpressoReporting.Views;
using System.Threading.Tasks;
using System.Reflection;
using ExpressoReporting.DataModel;
using PCLAppConfig;
using Xamarin.Essentials;

namespace ExpressoReporting
{
    public partial class App : Application
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();
        public static bool IsUserLoggedIn { get; set; }
        public static User User { get; set; }

        public static bool UseMockDataStore = false;
        public static ISophisDataStore SophisStore;

        public App()
        {
            try
            {
                //ConfigurationManager initialization
                var assembly = typeof(App).GetTypeInfo().Assembly;
                ConfigurationManager.Initialise(assembly.GetManifestResourceStream("ExpressoReporting.App.config"));
            }
            catch
            {
                // ignored
            }
            VersionTracking.Track();
            Logger.Info($"ExpressoReporting {VersionTracking.CurrentVersion}({VersionTracking.CurrentBuild}) starting new instance...");
            Logger.Info($"AzureBackendUrl = {ConfigurationManager.AppSettings["AzureBackendUrl"]}");

            AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
                var ex = (Exception)args.ExceptionObject;
                Logger.Error("UnhandledException exception");
                Logger.Error(ex.ToString());
            };

            IsUserLoggedIn = Settings.IsUserLoggedIn;
            if (IsUserLoggedIn)
            {
                User = Settings.LastUser;
            }

            InitializeComponent();

            DependencyService.Register<SophisDataStore>();
            SophisStore = DependencyService.Get<ISophisDataStore>();
            if (UseMockDataStore)
                DependencyService.Register<MockDataStore>();
            else
                DependencyService.Register<AzureDataStore>();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
        public static void DisplayMsgInfo(string message)
        {
            Device.BeginInvokeOnMainThread(async () => {
                await Current.MainPage.DisplayAlert("Information", message, "OK");
            });
        }

        public static void DisplayMsgError(string message)
        {
            Device.BeginInvokeOnMainThread(async () => {
                await Current.MainPage.DisplayAlert("Error", message, "OK");
            });
        }

        public static Task<bool> DisplayMsgQuestion(string message)
        {
            var tcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                var result = await Current.MainPage.DisplayAlert("Error", message, "OK", "Cancel");
                tcs.SetResult(result);
            });
            return tcs.Task;
        }
    }
}
