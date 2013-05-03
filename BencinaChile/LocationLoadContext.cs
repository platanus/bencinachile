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

namespace BencinaChile
{
    public class LocationLoadContext : LoadContext
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string UniqueId { get; set; }
        public string OsVersion { get; set; }

        public string ComputedAddress {
            get {
                return (string)Identity;
            }            
        }

        public LocationLoadContext(string week)
            : base(week) {
 
        }
    }
}
