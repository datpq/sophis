using ExpressoReporting.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms;

namespace ExpressoReporting.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MenuPage : ContentPage
    {
        MainPage RootPage { get => Application.Current.MainPage as MainPage; }

        private ObservableCollection<HomeMenuItem> _menuItems;
        public ObservableCollection<HomeMenuItem> MenuItems
        {
            get => _menuItems;
            set
            {
                _menuItems = value;
                OnPropertyChanged(nameof(MenuItems));
            }
        }

        public MenuPage()
        {
            InitializeComponent();

            RefreshMenu();

            ListViewMenu.ItemSelected += async (sender, e) =>
            {
                if (e.SelectedItem == null)
                    return;

                var id = (int)((HomeMenuItem)e.SelectedItem).Id;
                await RootPage.NavigateFromMenu(id);
            };

            BindingContext = this;
        }

        public void RefreshMenu()
        {
            if (App.IsUserLoggedIn)
            {
                MenuItems =  new ObservableCollection<HomeMenuItem>()
                {
                    //new HomeMenuItem {Id = MenuItemType.Browse, Title="Browse" },
                    new HomeMenuItem {Id = MenuItemType.Expresso, Title = res.AppTitle },
                    new HomeMenuItem {Id = MenuItemType.Logout, Title = res.MnuLogout },
                    new HomeMenuItem {Id = MenuItemType.About, Title = res.MnuAbout }
                };
            }
            else
            {
                MenuItems = new ObservableCollection<HomeMenuItem>()
                {
                    //new HomeMenuItem {Id = MenuItemType.Browse, Title="Browse" },
                    new HomeMenuItem {Id = MenuItemType.Login, Title = res.MnuLogin },
                    new HomeMenuItem {Id = MenuItemType.About, Title = res.MnuAbout }
                };
            }
            ListViewMenu.SelectedItem = MenuItems[0];
        }
    }
}