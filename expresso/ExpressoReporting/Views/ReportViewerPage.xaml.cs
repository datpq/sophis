using ExpressoReporting.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressoReporting.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReportViewerPage : ContentPage
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        public ReportViewerPage(string filePath)
        {
            InitializeComponent();
            Logger.Debug($"filePath={filePath}");
            Report.Uri = $"file://{filePath}";
            //Task.Run(() => App.SophisStore.GenerateReport()).ContinueWith(task =>
            //{
            //    Logger.Debug($"Report generated at {task.Result}");
            //    Report.Uri = $"file://{task.Result}";
            //}, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}