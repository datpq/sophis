using System;
using Acr.UserDialogs;
using ExpressoReporting.DataModel;
using ExpressoReporting.Models;
using ExpressoReporting.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressoReporting.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();
        MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        MenuPage MenuPage { get => RootPage.Master as MenuPage; }

        public LoginPage()
        {
            InitializeComponent();
            TxtUsername.Text = Settings.LastUser?.Name;
        }

        private async void CmdLogin_OnClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtUsername.Text))
            {
                try
                {
                    UserDialogs.Instance.ShowLoading(res.Processing);
                    var user = await App.SophisStore.Login(new User
                    {
                        Name = TxtUsername.Text,
                        Password = TxtPassword.Text
                    });
                    UserDialogs.Instance.HideLoading();
                    if (user != null)
                    {
                        App.IsUserLoggedIn = true;
                        Settings.IsUserLoggedIn = App.IsUserLoggedIn;
                        App.User = user;
                        Settings.LastUser = App.User;
                        TxtPassword.Text = string.Empty;
                        MenuPage.RefreshMenu();
                        //await RootPage.NavigateFromMenu((int)MenuItemType.Expresso);
                    }
                    else
                    {
                        App.DisplayMsgError(res.MsgLoginFailed);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
                finally
                {
                    //UserDialogs.Instance.HideLoading();
                }
            }
            else
            {
                App.DisplayMsgError("");
                TxtUsername.Focus();
            }
        }
    }
}