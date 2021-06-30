using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressoReporting.Models
{
    public enum MenuItemType
    {
        Browse,
        Login,
        Logout,
        Expresso,
        About
    }
    public class HomeMenuItem
    {
        public MenuItemType Id { get; set; }

        public string Title { get; set; }
    }
}
