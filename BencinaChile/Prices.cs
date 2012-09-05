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

namespace BencinaChile
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Prices
    {
        [JsonProperty(PropertyName = "g93")]
        public int? G93 { get; set; }

        [JsonProperty(PropertyName = "g95")]
        public int? G95 { get; set; }

        [JsonProperty(PropertyName = "g97")]
        public int? G97 { get; set; }

        [JsonProperty(PropertyName = "diesel")]
        public int? Diesel { get; set; }

        [JsonProperty(PropertyName = "kerosene")]
        public int? Kerosene { get; set; }
        
        [JsonProperty(PropertyName = "gnc")]
        public int? Gnc { get; set; }

        [JsonProperty(PropertyName = "glp")]
        public int? Glp { get; set; }


    }
}
