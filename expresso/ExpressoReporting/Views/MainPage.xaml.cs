using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using ExpressoReporting.Models;
using ExpressoReporting.Services;

namespace ExpressoReporting.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : MasterDetailPage
    {
        private static readonly ILogger Logger = DependencyService.Get<ILogManager>().GetLog();

        Dictionary<int, NavigationPage> MenuPages = new Dictionary<int, NavigationPage>();
        MenuPage MenuPage { get => Master as MenuPage; }

        public MainPage()
        {
            InitializeComponent();

            MasterBehavior = MasterBehavior.Popover;

            if (App.IsUserLoggedIn)
            {
                Detail = new NavigationPage(new ExpressoPage());
            }
            else
            {
                Detail = new NavigationPage(new LoginPage());
            }

            //MenuPages.Add((int)MenuItemType.Browse, (NavigationPage)Detail);
        }

        public async Task NavigateFromMenu(int id)
        {
            if (!MenuPages.ContainsKey(id))
            {
                switch (id)
                {
                    case (int)MenuItemType.Browse:
                        MenuPages.Add(id, new NavigationPage(new ItemsPage()));
                        break;
                    case (int)MenuItemType.Login:
                        MenuPages.Add(id, new NavigationPage(new LoginPage()));
                        break;
                    case (int)MenuItemType.Logout:
                        App.IsUserLoggedIn = false;
                        App.User = null;
                        Settings.IsUserLoggedIn = App.IsUserLoggedIn;
                        Settings.LastUser.Token = null;
                        MenuPage.RefreshMenu();
                        //error when the LoginPage is already open
                        //id = (int)MenuItemType.Login;
                        //MenuPages.Add(id, new NavigationPage(new LoginPage()));
                        break;
                    case (int)MenuItemType.Expresso:
                        MenuPages.Add(id, new NavigationPage(new ExpressoPage()));
                        break;
                    case (int)MenuItemType.About:
                        MenuPages.Add(id, new NavigationPage(new AboutPage()));
                        break;
                }
            }

            if (!MenuPages.ContainsKey(id)) return;

            var newPage = MenuPages[id];

            if (newPage != null && Detail != newPage)
            {
                Detail = newPage;

                if (Device.RuntimePlatform == Device.Android)
                    await Task.Delay(100);

                IsPresented = false;
            }
        }
    }
}