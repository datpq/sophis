using System;
using System.Windows.Input;

using Xamarin.Forms;

namespace ExpressoReporting.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";

            OpenWebCommand = new Command(() => Device.OpenUri(new Uri("http://efficiency-mc.com/")));
        }

        public ICommand OpenWebCommand { get; }
    }
}