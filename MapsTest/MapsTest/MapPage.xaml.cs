using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace MapsTest
{
    public partial class MapPage : ContentPage
    {
        public MapPage()
        {
            InitializeComponent();

            // Add a sample pushpin to the map
            MyMap.Pins.Add(new Xamarin.Forms.Maps.Pin() { Label = "Cheese", Position = new Xamarin.Forms.Maps.Position(52.4817055, -1.9065627) });

            foreach(Pin p in MyMap.Pins)
            {
                p.Clicked += P_Clicked;

            }

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

        private async void P_Clicked(object sender, EventArgs e)
        {
            Pin p = sender as Pin;
            Geocoder g = new Geocoder();
            foreach(string s in await g.GetAddressesForPositionAsync(p.Position))
            {
                System.Diagnostics.Debug.WriteLine(s);
                foreach(Position pos in await g.GetPositionsForAddressAsync(s))
                {
                    System.Diagnostics.Debug.WriteLine(pos.Latitude + ", " + pos.Longitude);
                }
            }
        }
    }
}
