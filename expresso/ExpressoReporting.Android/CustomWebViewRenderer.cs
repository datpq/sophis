using ExpressoReporting.Droid;
using ExpressoReporting.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomWebView), typeof(CustomWebViewRenderer))]
namespace ExpressoReporting.Droid
{
    public class CustomWebViewRenderer : WebViewRenderer
    {
        public CustomWebViewRenderer(Android.Content.Context context) : base(context) {}

        //protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "Uri")
        //    {
        //        var customWebView = sender as CustomWebView;
        //        Control.Settings.AllowUniversalAccessFromFileURLs = true;
        //        Control.LoadUrl($"file:///android_asset/pdfjs/web/viewer.html?file={customWebView?.Uri}");
        //    }
        //}

        protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                var customWebView = Element as CustomWebView;
                Control.Settings.AllowUniversalAccessFromFileURLs = true;
                //Control.LoadUrl($"file:///android_asset/pdfjs/web/viewer.html?file=file:///android_asset/Content/{WebUtility.UrlEncode(customWebView.Uri)}");
                Control.LoadUrl($"file:///android_asset/pdfjs/web/viewer.html?file={customWebView?.Uri}");
            }
        }
    }
}