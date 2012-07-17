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

namespace BencinaChile
{
    public partial class PriceControl : UserControl
    {

        public PriceControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the MyProperty value.
        /// </summary>
        public string Price
        {
            get { return (string)GetValue(PriceProperty); }
            set { SetValue(PriceProperty, value); }
        }
        
        /// <summary>
        /// Dependency field of MyProperty property.
        /// </summary>
        public static readonly DependencyProperty PriceProperty =
                DependencyProperty.Register("Price",
                         typeof(string),
                         typeof(PriceControl),
                         new PropertyMetadata(
        new PropertyChangedCallback(OnPricePropertyChanged)));

        private static void OnPricePropertyChanged(DependencyObject sender,
                    DependencyPropertyChangedEventArgs args)
        {
           var priceControl = (PriceControl)sender;
           priceControl.price.Text = args.NewValue as String;
        }

        /// <summary>
        /// Gets or sets the MyProperty value.
        /// </summary>
        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        /// <summary>
        /// Dependency field of MyProperty property.
        /// </summary>
        public static readonly DependencyProperty CaptionProperty =
                DependencyProperty.Register("Caption",
                         typeof(string),
                         typeof(PriceControl),
                         new PropertyMetadata(
        new PropertyChangedCallback(OnCaptionPropertyChanged)));

        private static void OnCaptionPropertyChanged(DependencyObject sender,
                    DependencyPropertyChangedEventArgs args)
        {
            var priceControl = (PriceControl)sender;
            priceControl.caption.Text = args.NewValue as String;
        }
    }
}
