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

            MyMap.Pins.Add(new Xamarin.Forms.Maps.Pin() { Label = "Cheese", Position = new Xamarin.Forms.Maps.Position(52.4817055, -1.9065627) });
            MyMap.MoveToRegion(Xamarin.Forms.Maps.MapSpan.FromCenterAndRadius(new Xamarin.Forms.Maps.Position(52.4817055, -1.9065627), Xamarin.Forms.Maps.Distance.FromKilometers(3)));

            Task.Run(async () =>
            {
                await Task.Delay(10000);
                Device.BeginInvokeOnMainThread(() =>
                {
                    MyMap.Pins.Add(new Xamarin.Forms.Maps.Pin() { Label = "Toast", Position = new Xamarin.Forms.Maps.Position(52.479014, -1.9039143) });
                });

            });
        }
    }
}
