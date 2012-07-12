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
        // The GPS object
        GeoCoordinateWatcher gcw;

        // The geocode object
        GeocodeServiceClient geocodeService = null;

        // Local properties for global access
        GeoCoordinate currentLocation;
        Station selectedStation;
        StationsViewModel stationViewModel;
        
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Start the map
            map1.Mode = new RoadMode();

            // TODO: Start map centered at the last location

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Don't do anything if we already have the data
            if (stationViewModel != null) return;
            
            // Prevent the creation of another watcher
            if (gcw == null)
                gcw = new GeoCoordinateWatcher(GeoPositionAccuracy.High);

            // Start the GPS
            gcw.Start();

            // Set the loading in with a GPS message
            GlobalLoading.Instance.IsLoading = true;
            GlobalLoading.Instance.SetText("Buscando ubicación...");

            gcw.StatusChanged += (sender, ev) =>
            {
                // We've got a location from the gps
                if (ev.Status == GeoPositionStatus.Ready)
                {
                    // Stop the gps
                    gcw.Stop();

                    // Set the loading of when the gps is ready
                    GlobalLoading.Instance.IsLoading = false;
                    GlobalLoading.Instance.SetText("");

                    // Save the current location for later usage
                    currentLocation = new GeoCoordinate(gcw.Position.Location.Latitude, gcw.Position.Location.Longitude);

                    var loadContext = new LocationLoadContext(DateTime.Now.Ticks.ToString());
                    loadContext.Latitude = currentLocation.Latitude;
                    loadContext.Longitude = currentLocation.Longitude;

                    // Try to get and address reference to the location
                    ReverseGeocodeRequest reverseGeocodeRequest = new ReverseGeocodeRequest();

                    // Set the credentials using a valid Bing Maps key
                    reverseGeocodeRequest.Credentials = new Credentials();
                    reverseGeocodeRequest.Credentials.ApplicationId = "AoCLhSPbKx0T0j_OqgRICrBA6z9VvF4ST9VrM-zd8XW13rE63U8ZPzvrcFdEpz_N";

                    reverseGeocodeRequest.Location = currentLocation;

                    geocodeService = new GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
                    geocodeService.ReverseGeocodeCompleted += (sen, evarg) =>
                    {
                        // The the address location in the address control
                        address.Text = String.Format("{0}, {1}", evarg.Result.Results[0].Address.Locality, evarg.Result.Results[0].Address.AdminDistrict).ToUpper();
                    };
                    geocodeService.ReverseGeocodeAsync(reverseGeocodeRequest);
                    
                    // Center the map and add a pushpin in my current location
                    map1.SetView(currentLocation, 15);
                    map1.Children.Add(new Pushpin() {
                        Location = currentLocation,
                        Content = "Tú",
                        Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush
                    });                   

                    // Load data context from bencinas.satelinx api
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