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
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Phone.Controls.Maps;
using System.Device.Location;

namespace BencinaChile
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Station
    {
        private GeoCoordinate _location;
        
        [JsonProperty(PropertyName = "brand_name")]
        public string BrandName { get; set; }

        [JsonProperty(PropertyName = "brand_id")]
        public int BrandId { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }
        
        [JsonProperty(PropertyName = "latitude")]
        public double Latitude { private get; set; }

        [JsonProperty(PropertyName = "longitude")]
        public double Longitude { private get; set; }

        [JsonProperty(PropertyName = "prices")]
        public Prices Prices{ get; set; }

        public Pushpin PushPin { get; set; }

        public int PositionId { get; set; }

        public GeoCoordinate Location
        {
            get
            {
                if (_location == null)
                    _location = new GeoCoordinate(Latitude, Longitude);

                return _location;
            }
        }
    }
}
