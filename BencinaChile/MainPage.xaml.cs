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
using System.Device.Location;
using Microsoft.Phone.Reactive;

namespace BencinaChile
{
    public partial class MainPage : PhoneApplicationPage
    {
        GeoCoordinateWatcher gcw;
        
        // Constructor
        public MainPage()
        {
            InitializeComponent();


        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (gcw == null)
                gcw = new GeoCoordinateWatcher(GeoPositionAccuracy.High);

            gcw.Start();

            gcw.StatusChanged += (sender, ev) =>
            {
                if (ev.Status == GeoPositionStatus.Ready)
                {
                    System.Diagnostics.Debug.WriteLine(gcw.Position.Location.Latitude.ToString() + ", " + gcw.Position.Location.Longitude.ToString());

                    // Stop the gps
                    gcw.Stop();
                }
            };
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (gcw != null)
                gcw.Stop();
        }
    }
}