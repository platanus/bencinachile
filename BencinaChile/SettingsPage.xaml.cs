using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using BencinaChile.Utilities;

namespace BencinaChile
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void Feedback_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            new EmailComposeTask
            {
                Subject = "Bencina WP - Feedback",
                Body = "Bencina Chile",
                To = "contacto@platan.us"
            }.Show();
        }

        private void Satelinx_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            new WebBrowserTask
            {
                Uri = new Uri("http://bencinas.satelinx.com")               
            }.Show();
        }

        private void Platanus_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            new WebBrowserTask
            {
                Uri = new Uri("http://platan.us")
            }.Show();
        }

        private void EnableLocation_Unchecked(object sender, RoutedEventArgs e)
        {
            App.Current.Settings.LocationEnabledSetting = false;
        }

        private void EnableLocation_Checked(object sender, RoutedEventArgs e)
        {
            App.Current.Settings.LocationEnabledSetting = true;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            EnableLocation.IsChecked = App.Current.Settings.LocationEnabledSetting;
        }
    }
}