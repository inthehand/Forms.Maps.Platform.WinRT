using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace MapsTest
{
    public partial class MapPage : ContentPage
    {
        public MapPage()
        {
            InitializeComponent();

            // Add a sample pushpin to the map
            MyMap.Pins.Add(new Xamarin.Forms.Maps.Pin() { Label = "Cheese", Position = new Xamarin.Forms.Maps.Position(52.4817055, -1.9065627) });

            // Move the view to surround the pushpin
            MyMap.MoveToRegion(Xamarin.Forms.Maps.MapSpan.FromCenterAndRadius(new Xamarin.Forms.Maps.Position(52.4817055, -1.9065627), Xamarin.Forms.Maps.Distance.FromKilometers(2)));

            Task.Run(async () =>
            {
                await Task.Delay(10000);
                Device.BeginInvokeOnMainThread(() =>
                {
                    // add an additional pushpin to simlulate user action
                    MyMap.Pins.Add(new Xamarin.Forms.Maps.Pin() { Label = "Toast", Position = new Xamarin.Forms.Maps.Position(52.479014, -1.9039143) });
                });

                await Task.Delay(10000);
                Device.BeginInvokeOnMainThread(() =>
                {
                    // add an additional pushpin to simlulate user action
                    MyMap.Pins.Add(new Xamarin.Forms.Maps.Pin() { Label = "Cup of Tea", Position = new Xamarin.Forms.Maps.Position(52.4804469, -1.9019083) });
                });

            });
        }
    }
}
