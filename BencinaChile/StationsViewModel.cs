using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using AgFx;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Device.Location;
using System.Globalization;

namespace BencinaChile
{
    [CachePolicy(CachePolicy.CacheThenRefresh, 60 * 15)]
    public class StationsViewModel : ModelItemBase<LocationLoadContext>
    {
        private BatchObservableCollection<Station> stations = new BatchObservableCollection<Station>();
        public BatchObservableCollection<Station> Stations
        {
            get { return stations; }
            set
            {
                stations = value;
                RaisePropertyChanged("Stations");
            }
        }

        #region IDataLoader

        public class TwitterViewModelDataLoader : IDataLoader<LocationLoadContext>
        {
            private const string StationsSearchUriFormat = "http://bencinas.satelinx.com/api/closest?developer_key=a608233473cdad3022a2c4ceeec2508b106e49539fd17e96283e3d181d9955a7&latitude={0}&longitude={1}";

            public LoadRequest GetLoadRequest(LocationLoadContext loadContext, System.Type objectType)
            {
                string uri = String.Format(StationsSearchUriFormat, loadContext.Latitude.ToString(CultureInfo.InvariantCulture), loadContext.Longitude.ToString(CultureInfo.InvariantCulture));
                return new WebLoadRequest(loadContext, new Uri(uri));
            }

            public object Deserialize(LocationLoadContext loadContext, System.Type objectType, System.IO.Stream stream)
            {
                string str;
                using (stream)
                {
                    // Jump to the start position of the stream
                    stream.Seek(0, SeekOrigin.Begin);

                    StreamReader rdr = new StreamReader(stream);
                    str = rdr.ReadToEnd();
                }

                StationsViewModel vm = new StationsViewModel { LoadContext = loadContext };
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.DefaultValueHandling = DefaultValueHandling.Ignore;
                var jarray = JsonConvert.DeserializeObject<Stations>(str, settings);
                var i = 1;
                foreach (var item in jarray.Items)
                {
                    item.SetDistanceFrom(new GeoCoordinate(loadContext.Latitude, loadContext.Longitude));
                }
                
                jarray.Items.Sort(delegate(Station s1, Station s2) { return s1.Distance.CompareTo(s2.Distance); });

                foreach (var item in jarray.Items)
                {
                    item.PositionId = i;
                    vm.Stations.Add(item);
                    i++;
                }
                return vm;
            }
        }

        #endregion
    }
}
