﻿using Xamarin.Forms;

namespace ExpressoReporting.Views
{
    public class CustomWebView : WebView
    {
        public static readonly BindableProperty UriProperty = BindableProperty.Create(propertyName: "Uri",
            returnType: typeof(string),
            declaringType: typeof(CustomWebView),
            defaultValue: default(string));

        public string Uri
        {
            get => (string)GetValue(UriProperty);
            set => SetValue(UriProperty, value);
        }
    }
}
