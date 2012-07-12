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
using System.Windows.Data;
using System.Device.Location;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Reactive;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Shell;
using AgFx;
using AgFx.Controls;

using BencinaChile.GeoCodeService;

namespace BencinaChile
{
    public partial class MainPage : PhoneApplicationPage
    {
        GeoCoordinateWatcher gcw;
        GeocodeServiceClient geocodeService = null;

        GeoCoordinate currentLocation;
        Station selectedStation;
        StationsViewModel stationViewModel;
        
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            map1.Mode = new RoadMode();

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (stationViewModel != null) return;
            
            if (gcw == null)
                gcw = new GeoCoordinateWatcher(GeoPositionAccuracy.High);

            gcw.Start();

            GlobalLoading.Instance.IsLoading = true;
            GlobalLoading.Instance.SetText("Buscando ubicación...");

            gcw.StatusChanged += (sender, ev) =>
            {
                if (ev.Status == GeoPositionStatus.Ready)
                {
                    // Stop the gps
                    gcw.Stop();

                    GlobalLoading.Instance.IsLoading = false;
                    GlobalLoading.Instance.SetText("");


                    var loadContext = new LocationLoadContext(DateTime.Now.Ticks.ToString());
                    loadContext.Latitude = gcw.Position.Location.Latitude;
                    loadContext.Longitude = gcw.Position.Location.Longitude;

                    currentLocation = new GeoCoordinate(gcw.Position.Location.Latitude, gcw.Position.Location.Longitude);
                    map1.SetView(currentLocation, 15);
                    map1.Children.Add(new Pushpin() {
                        Location = currentLocation,
                        Content = "Tú",
                        Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush
                    });

                    geocodeService = new GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
                    geocodeService.ReverseGeocodeCompleted += (sen, evarg) =>
                    {
                        address.Text = String.Format("{0}, {1}", evarg.Result.Results[0].Address.Locality, evarg.Result.Results[0].Address.AdminDistrict).ToUpper();
                    };

                    ReverseGeocodeRequest reverseGeocodeRequest = new ReverseGeocodeRequest();

                    // Set the credentials using a valid Bing Maps key
                    reverseGeocodeRequest.Credentials = new Credentials();
                    reverseGeocodeRequest.Credentials.ApplicationId = "AoCLhSPbKx0T0j_OqgRICrBA6z9VvF4ST9VrM-zd8XW13rE63U8ZPzvrcFdEpz_N";

                    reverseGeocodeRequest.Location = currentLocation;

                    geocodeService.ReverseGeocodeAsync(reverseGeocodeRequest);
                    

                    this.DataContext = DataManager.Current.Load<StationsViewModel>(loadContext,
                        (vm) =>
                        {
                            stationViewModel = vm;
                        
                            foreach(var station in stationViewModel.Stations){
                                var pushpin = new Pushpin();
                                pushpin.Location = station.Location;
                                pushpin.Content = station.PositionId;
                                station.PushPin = pushpin;
                                map1.Children.Add(pushpin);
                            }
                        },
                        (ex) =>
                        {
                            MessageBox.Show("Failed to get data for ");
                        }
                    );
                }
            };
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (gcw != null)
                gcw.Stop();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.Opacity = 0;
            SystemTray.ForegroundColor = Colors.Black;
        }

        private void stationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedStation =  (Station)stationList.SelectedItem;
            if (selectedStation != null)
            {
                map1.SetView(selectedStation.Location, 15);
                foreach (var station in stationViewModel.Stations) {
                    station.PushPin.Background = new SolidColorBrush(Colors.Black);
                }
                selectedStation.PushPin.Background = new SolidColorBrush(Colors.Blue);
            }
        }

        private void open_bing_maps(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (selectedStation == null) return;
            
            BingMapsDirectionsTask bingMapsDirectionsTask = new BingMapsDirectionsTask();

            // You can specify a label and a geocoordinate for the end point.
            GeoCoordinate spaceNeedleLocation = currentLocation;
            // LabeledMapLocation spaceNeedleLML = new LabeledMapLocation("Space Needle", spaceNeedleLocation);

            // If you set the geocoordinate parameter to null, the label parameter is used as a search term.
            LabeledMapLocation startSpaceNeedleLML = new LabeledMapLocation("Tu", currentLocation);
            LabeledMapLocation endSpaceNeedleLML = new LabeledMapLocation("Colon 345, las condes",selectedStation.Location);

            bingMapsDirectionsTask.Start = startSpaceNeedleLML;
            bingMapsDirectionsTask.End = endSpaceNeedleLML;

            bingMapsDirectionsTask.Show();
        }

    }
}