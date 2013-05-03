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
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Shell;
using AgFx;
using AgFx.Controls;

using BencinaChile.GeoCodeService;
using System.ComponentModel;
using Microsoft.Phone.Info;

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

            //Get the unique id and save it
            if (App.Current.Settings.UniqueId == "")
            {
                object DeviceUniqueID;
                byte[] DeviceIDbyte = null;
                if (DeviceExtendedProperties.TryGetValue("DeviceUniqueId", out DeviceUniqueID))
                    DeviceIDbyte = (byte[])DeviceUniqueID;
                string DeviceID = Convert.ToBase64String(DeviceIDbyte);

                App.Current.Settings.UniqueId = DeviceID.ToString();
            }

            // Start the map
            map1.Mode = new RoadMode();

            // TODO: Start map centered at the last location

            StationsPanorama.Loaded += (sender, ev) =>
            {
                // Don't do anything if we already have the data
                if (stationViewModel != null) return;
                
                // Set the Sort Types
                List<SortType> gasSortTypes = new List<SortType>();
                gasSortTypes.Add(new SortType() { Name = "distancia", PropetyKey = "Distance" });
                gasSortTypes.Add(new SortType() { Name = "gasolina 93", PropetyKey = "Prices.G93" });
                gasSortTypes.Add(new SortType() { Name = "gasolina 95", PropetyKey = "Prices.G95" });
                gasSortTypes.Add(new SortType() { Name = "gasolina 97", PropetyKey = "Prices.G97" });
                gasSortTypes.Add(new SortType() { Name = "diesel", PropetyKey = "Prices.Diesel" });

                List<SortType> otherSortTypes = new List<SortType>();
                otherSortTypes.Add(new SortType() { Name = "distancia", PropetyKey = "Distance" });
                otherSortTypes.Add(new SortType() { Name = "parafina", PropetyKey = "Prices.Kerosene" });
                otherSortTypes.Add(new SortType() { Name = "gas natural", PropetyKey = "Prices.Gnc" });
                otherSortTypes.Add(new SortType() { Name = "gas liquado", PropetyKey = "Prices.Glp" });

                // Set the list pickers
                var gasListPicker = FindDescendant<ListPicker>(StationsPanorama.Items[0] as PanoramaItem);
                gasListPicker.ExpansionMode = ExpansionMode.FullScreenOnly;
                gasListPicker.ItemsSource = gasSortTypes;

                var otherListPicker = FindDescendant<ListPicker>(StationsPanorama.Items[1] as PanoramaItem);
                otherListPicker.ExpansionMode = ExpansionMode.FullScreenOnly;
                otherListPicker.ItemsSource = otherSortTypes;
                
                CollectionViewSource gCvs = (CollectionViewSource)Resources["GasStations"];
                CollectionViewSource oCvs = (CollectionViewSource)Resources["OtherStations"];

                // Handle the list picker selection change events
                gasListPicker.SelectionChanged += (send, arg) =>
                {
                    var picker = (ListPicker)send;
                    var newType = (SortType)picker.SelectedItem;
                    if (oCvs.View != null)
                    {
                        SortDescription gasSort = new SortDescription(newType.PropetyKey, ListSortDirection.Ascending);
                        gCvs.SortDescriptions[0] = gasSort;
                        gCvs.View.Refresh();
                    }
                };

                otherListPicker.SelectionChanged += (send, arg) =>
                {
                    var picker = (ListPicker)send;
                    var newType = (SortType)picker.SelectedItem;
                    if (oCvs.View != null)
                    {
                        SortDescription otherSort = new SortDescription(newType.PropetyKey, ListSortDirection.Ascending);
                        oCvs.SortDescriptions[0] = otherSort;
                        oCvs.View.Refresh();
                    }
                };
            };

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var msg = "";

            // Don't do anything if we already have the data
            if (stationViewModel != null || !App.Current.Settings.LocationEnabledSetting)
            {
                // Show a error message instead of the list of gas stations
                msg = "No pudimos encontrar tu posición actual.";
                loadingText1.Text = msg;
                loadingText2.Text = msg;
                return;
            }
            
            // Prevent the creation of another watcher
            if (gcw == null)
                gcw = new GeoCoordinateWatcher(GeoPositionAccuracy.High);

            // Start the GPS
            gcw.Start();

            // Show a error message instead of the list of gas stations
            msg = "Espera mientras buscamos las bombas cercanas...";
            loadingText1.Text = msg;
            loadingText2.Text = msg;

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
                    loadContext.UniqueId = App.Current.Settings.UniqueId;
                    loadContext.OsVersion = System.Environment.OSVersion.Version.Major.ToString() + "." + System.Environment.OSVersion.Version.Minor.ToString();

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

                            loadingText1.Visibility = Visibility.Collapsed;
                            loadingText2.Visibility = Visibility.Collapsed;
                            gasStationList.Visibility = Visibility.Visible;
                            otherStationList.Visibility = Visibility.Visible;
                            
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
                            MessageBox.Show("No pudimos encontrar la información. Revisa tu conección a interent.");
                        }
                    );
                }
                else if (ev.Status == GeoPositionStatus.Disabled)
                {
                    // Show a error message instead of the list of gas stations
                    msg = "No pudimos encontrar tu posición actual.";
                    loadingText1.Text = msg;
                    loadingText2.Text = msg;

                    // Unset the global loading message
                    GlobalLoading.Instance.IsLoading = false;
                    GlobalLoading.Instance.SetText("");
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

        /// <summary>
        /// When the station is taped, moves the map and change the selected pushpin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Does not do anything if there is nothing selected
            if ((sender as ListBox).SelectedIndex == -1) return;

            if ((sender as ListBox).Name == "gasStationList")
            {
                selectedStation = (Station)gasStationList.SelectedItem;
                otherStationList.SelectedIndex = -1;
            }
            else 
            {
                selectedStation = (Station)otherStationList.SelectedItem;
                gasStationList.SelectedIndex = -1;
            }

            if (selectedStation != null && stationViewModel != null)
            {
                // Move the map
                map1.SetView(selectedStation.Location, 15);
                
                // Reset all the pushpins
                foreach (var station in stationViewModel.Stations) {
                    station.PushPin.Background = new SolidColorBrush(Colors.Black);
                }

                // Highlight the selected station pushpin
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
            LabeledMapLocation endSpaceNeedleLML = new LabeledMapLocation(selectedStation.Address, selectedStation.Location);

            bingMapsDirectionsTask.Start = startSpaceNeedleLML;
            bingMapsDirectionsTask.End = endSpaceNeedleLML;

            bingMapsDirectionsTask.Show();
        }

        private void GasStationsCollection_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = (((Station)e.Item).Prices.G93 != null || ((Station)e.Item).Prices.G95 != null || ((Station)e.Item).Prices.G97 != null || ((Station)e.Item).Prices.Diesel != null);
        }

        private void OtherStationsCollection_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = (((Station)e.Item).Prices.Kerosene != null || ((Station)e.Item).Prices.Glp != null || ((Station)e.Item).Prices.Gnc != null);
        }

        public T FindDescendant<T>(DependencyObject obj) where T : DependencyObject
        {
            // Check if this object is the specified type
            if (obj is T)
                return obj as T;

            // Check for children
            int childrenCount = VisualTreeHelper.GetChildrenCount(obj);
            if (childrenCount < 1)
                return null;

            // First check all the children
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                    return child as T;
            }

            // Then check the childrens children
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = FindDescendant<T>(VisualTreeHelper.GetChild(obj, i));
                if (child != null && child is T)
                    return child as T;
            }

            return null;
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private void tap_map(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/MapPage.xaml", UriKind.Relative));   
            
        }

    }


}