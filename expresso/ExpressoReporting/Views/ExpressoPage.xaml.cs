using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using ExpressoReporting.DataModel;
using ExpressoReporting.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressoReporting.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ExpressoPage : ContentPage
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();
        private readonly IList<Entry> _lstEntries = new List<Entry>();

        public ExpressoPage()
        {
            InitializeComponent();

            PickerReports.ItemsSource = null;
            if (App.IsUserLoggedIn)
            {
                Logger.Debug("ExpressoPage.BEGIN");
                UserDialogs.Instance.ShowLoading(res.Processing);
                Task.Run(() => App.SophisStore.GetReportsAsync()).ContinueWith(task =>
                {
                    PickerReports.ItemsSource = task.Result == null ? null : new ObservableCollection<Report>(task.Result);
                    UserDialogs.Instance.HideLoading();//must be called before setting SelectedIndex
                    Logger.Debug("ExpressoPage.END");
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void PickerReports_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            CmdGenerate.IsEnabled = CmdView.IsEnabled = PickerReports.SelectedIndex >= 0;
            if (!(PickerReports.SelectedItem is Report selectedReport)) return;
            var rowToRemove = Grid.Children.Where(x => Grid.GetRow(x) > 0).ToList();
            foreach (var child in rowToRemove)
            {
                Grid.Children.Remove(child);
            }

            while (Grid.RowDefinitions.Count > 1)
            {
                Grid.RowDefinitions.RemoveAt(1);
            }

            _lstEntries.Clear();
            var i = 0;
            foreach (var param in selectedReport.Parameters)
            {
                i++;
                var entry = new Entry
                {
                    PlaceholderColor = PickerReports.TitleColor, //(Color)Application.Current.Resources["PrimaryColor"]
                    TextColor = PickerReports.TextColor, //(Color)Application.Current.Resources["SecondaryColor"]
                    Placeholder = param.Name
                };
                Grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Grid.SetColumn(entry, 0);
                Grid.SetRow(entry, i);
                Grid.Children.Add(entry);
                _lstEntries.Add(entry);
            }
        }

        private async void CmdGenerate_OnClicked(object sender, EventArgs e)
        {
            try
            {
                if (!(PickerReports.SelectedItem is Report selectedReport)) return;
                Logger.Debug($"CmdGenerate_OnClicked.BEGIN(selectedReport={selectedReport.Name})");

                UserDialogs.Instance.ShowLoading(res.Processing);
                for (var i = 0; i < selectedReport.Parameters.Count(); i++)
                {
                    selectedReport.Parameters.ElementAt(i).Value = _lstEntries[i].Text;
                }
                var filePath = await App.SophisStore.GenerateReport(selectedReport);
                await Navigation.PushModalAsync(new ReportViewerPage(filePath));
            }
            catch (Exception ex)
            {
                App.DisplayMsgError(ex.Message);
                Logger.Error(ex.ToString());
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
                Logger.Debug("CmdGenerate_OnClicked.END");
            }
        }

        private async void CmdView_OnClicked(object sender, EventArgs e)
        {
            try
            {
                if (!(PickerReports.SelectedItem is Report selectedReport)) return;
                Logger.Debug($"CmdView_OnClicked.BEGIN(selectedReport={selectedReport.Name})");

                var filePath = await App.SophisStore.GetLastGeneratedReport(selectedReport.Name);
                await Navigation.PushModalAsync(new ReportViewerPage(filePath));
            }
            catch (Exception ex)
            {
                App.DisplayMsgError(ex.Message);
                Logger.Error(ex.ToString());
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
                Logger.Debug("CmdView_OnClicked.END");
            }
        }
    }
}