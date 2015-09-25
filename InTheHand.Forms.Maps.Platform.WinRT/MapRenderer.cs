﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.Reflection;
using System.Collections.Specialized;
using Windows.UI.Xaml.Controls.Maps;

[assembly: Xamarin.Forms.Platform.WinRT.ExportRenderer(typeof(Xamarin.Forms.Maps.Map), typeof(InTheHand.Forms.Maps.Platform.WinRT.MapRenderer))]

namespace InTheHand.Forms.Maps.Platform.WinRT
{
    public sealed class MapRenderer : Xamarin.Forms.Platform.WinRT.VisualElementRenderer<Xamarin.Forms.Maps.Map, Windows.UI.Xaml.Controls.Maps.MapControl>
    {
        private bool firstZoomLevelChangeFired;

        protected override void OnElementChanged(Xamarin.Forms.Platform.WinRT.ElementChangedEventArgs<Xamarin.Forms.Maps.Map> e)
        {
            base.OnElementChanged(e);

            if(e.NewElement != null)
            {
                ((INotifyCollectionChanged)e.NewElement.Pins).CollectionChanged += MapRenderer_CollectionChanged;
                SetNativeControl(new Windows.UI.Xaml.Controls.Maps.MapControl());
                Control.ZoomLevelChanged += Control_ZoomLevelChanged;
                Control.CenterChanged += Control_CenterChanged;
                Xamarin.Forms.MessagingCenter.Subscribe<Xamarin.Forms.Maps.Map, Xamarin.Forms.Maps.MapSpan>(this, "MapMoveToRegion", (Action<Xamarin.Forms.Maps.Map, Xamarin.Forms.Maps.MapSpan>)((s, a) => this.MoveToRegion(a)), Element);
      
                if(!string.IsNullOrEmpty(FormsMaps._serviceToken ))
                {
                    Control.MapServiceToken = FormsMaps._serviceToken;
                }
                Control.Style = MapTypeToMapStyle(e.NewElement.MapType);
                AddPins();        
            }
            else
            {
                ((INotifyCollectionChanged)e.NewElement.Pins).CollectionChanged -= MapRenderer_CollectionChanged;
            }
        }

        private void AddPins()
        {
            foreach(Pin p in Element.Pins)
            {
                Control.MapElements.Add(new MapIcon() { Location = new Windows.Devices.Geolocation.Geopoint(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = p.Position.Latitude, Longitude = p.Position.Longitude }), Title = p.Label });                  
            }
        }
        void MapRenderer_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    Control.MapElements.Clear();
                    break;
                case NotifyCollectionChangedAction.Add:
                    Pin p = e.NewItems[0] as Pin;
                    Control.MapElements.Insert(e.NewStartingIndex, new MapIcon(){ Location = new Windows.Devices.Geolocation.Geopoint(new Windows.Devices.Geolocation.BasicGeoposition(){ Latitude=p.Position.Latitude, Longitude=p.Position.Longitude}), Title = p.Label});
                    break;
                case NotifyCollectionChangedAction.Move:
                    MapIcon i = Control.MapElements[e.OldStartingIndex] as MapIcon;
                    Control.MapElements.RemoveAt(e.OldStartingIndex);
                    Control.MapElements.Insert(e.NewStartingIndex, i);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Control.MapElements.RemoveAt(e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Control.MapElements.RemoveAt(e.OldStartingIndex);
                    Pin rp = e.NewItems[0] as Pin;
                    Control.MapElements.Insert(e.OldStartingIndex, new MapIcon(){ Location = new Windows.Devices.Geolocation.Geopoint(new Windows.Devices.Geolocation.BasicGeoposition(){ Latitude=rp.Position.Latitude, Longitude=rp.Position.Longitude}), Title = rp.Label});
                    break;
            }
        }

        void Control_CenterChanged(Windows.UI.Xaml.Controls.Maps.MapControl sender, object args)
        {
            UpdateVisibleRegion();
        }

        void Control_ZoomLevelChanged(Windows.UI.Xaml.Controls.Maps.MapControl sender, object args)
        {
            UpdateVisibleRegion();
        }

        private PropertyInfo _lmtrProperty = null;
        private PropertyInfo _vrProperty = null;

        private void UpdateVisibleRegion()
        {
            if (!this.firstZoomLevelChangeFired)
            {
                MapSpan region = null;
                if (_lmtrProperty == null)
                {
                    foreach (PropertyInfo pi in typeof(Map).GetRuntimeProperties())
                    {
                        if (pi.Name == "LastMoveToRegion")
                        {
                            _lmtrProperty = pi;               
                            break;
                        }
                    }
                }

                region = _lmtrProperty.GetValue(Element) as MapSpan;

                this.MoveToRegion(region);
                this.firstZoomLevelChangeFired = true;
            }
            else
            {
                Position center = new Position(Control.Center.Position.Latitude, Control.Center.Position.Longitude);
                //GeoCoordinate geoCoordinate1 = Control.ConvertViewportPointToGeoCoordinate(new Point(0.0, 0.0));
                //GeoCoordinate geoCoordinate2 = Control.ConvertViewportPointToGeoCoordinate(new Point(Control.ActualWidth, Control.ActualHeight));
                //if (geoCoordinate1 == (GeoCoordinate)null || geoCoordinate2 == (GeoCoordinate)null)
                //    return;
                //LocationRectangle boundingRectangle = LocationRectangle.CreateBoundingRectangle(geoCoordinate1, geoCoordinate2);
                if (_vrProperty == null)
                {
                    foreach (PropertyInfo pi in typeof(Map).GetRuntimeProperties())
                    {
                        if (pi.Name == "VisibleRegion")
                        {
                            _vrProperty = pi;
                            break;
                        }
                    }
                }
                
                // TODO: work out zoom conversion
                double degrees = ZoomLevelToDegrees(Convert.ToInt32(Control.ZoomLevel));
                _vrProperty.SetValue(Element, new MapSpan(center, degrees, degrees));

            }
        }

        public override Xamarin.Forms.SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            return new Xamarin.Forms.SizeRequest(new Size(100, 100));
        }

        private async void MoveToRegion(MapSpan span)
        {
            await Control.TrySetViewAsync(new Windows.Devices.Geolocation.Geopoint(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = span.Center.Latitude, Longitude = span.Center.Longitude }), DegreesToZoomLevel(Math.Max(span.LatitudeDegrees, span.LatitudeDegrees)));
        }


        protected async override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.PropertyName);

            switch(e.PropertyName)
            {
                case "MapType":
                    Control.Style = MapTypeToMapStyle(Element.MapType);
                    break;
            }
            base.OnElementPropertyChanged(sender, e);
        }

        private static Windows.UI.Xaml.Controls.Maps.MapStyle MapTypeToMapStyle(Xamarin.Forms.Maps.MapType type)
        {
            switch(type)
            {
                case Xamarin.Forms.Maps.MapType.Street:
                    return Windows.UI.Xaml.Controls.Maps.MapStyle.Road;

                case Xamarin.Forms.Maps.MapType.Satellite:
                    return Windows.UI.Xaml.Controls.Maps.MapStyle.Aerial;

                case Xamarin.Forms.Maps.MapType.Hybrid:
                    return Windows.UI.Xaml.Controls.Maps.MapStyle.AerialWithRoads;

                default:
                    return Windows.UI.Xaml.Controls.Maps.MapStyle.None;
            }
        }

        private double ZoomLevelToDegrees(int zoomLevel)
        {
            if (zoomLevel < 1 || zoomLevel > 20)
            {
                throw new ArgumentOutOfRangeException("zoomLevel");
            }

            switch (zoomLevel)
            {
                case 1:
                    return 360.0;
                case 2:
                    return 180.0;
                case 3:
                    return 90.0;
                case 4:
                    return 45.0;
                case 5:
                    return 22.5;
                case 6:
                    return 11.25;
                case 7:
                    return 5.625;
                case 8:
                    return 2.8125;
                case 9:
                    return 1.40625;
                case 10:
                    return 0.703125;
                case 11:
                    return 0.3515625;
                case 12:
                    return 0.17578125;
                case 13:
                    return 0.087890625;
                case 14:
                    return 0.0439453125;
                case 15:
                    return 0.02197265625;
                case 16:
                    return 0.010986328125;
                case 17:
                    return 0.0054931640625;
                case 18:
                    return 0.00274658203125;
                case 19:
                    return 0.001373291015625;
                case 20:
                    return 0.0006866455078125;
            }

            return 0.0;
        }

        private int DegreesToZoomLevel(double degrees)
        {
            if (degrees > 180)
            {
                return 1;
            }
            else if (degrees > 90)
            {
                return 2;
            }
            else if (degrees > 45)
            {
                return 3;
            }
            else if (degrees > 22.5)
            {
                return 4;
            }
            else if (degrees > 11.25)
            {
                return 5;
            }
            else if (degrees > 5.625)
            {
                return 6;
            }
            else if (degrees > 2.8125)
            {
                return 7;
            }
            else if (degrees > 1.40625)
            {
                return 8;
            }
            else if (degrees > 0.703125)
            {
                return 9;
            }
            else if (degrees > 0.3515625)
            {
                return 10;
            }
            else if (degrees > 0.17578125)
            {
                return 11;
            }
            else if (degrees > 0.087890625)
            {
                return 12;
            }
            else if (degrees > 0.0439453125)
            {
                return 13;
            }
            else if (degrees > 0.2197265625)
            {
                return 14;
            }
            else if (degrees > 0.010986328125)
            {
                return 15;
            }
            else if (degrees > 0.0054931640625)
            {
                return 16;
            }
            else if (degrees > 0.00274658203125)
            {
                return 17;
            }
            else if (degrees > 0.001373291015625)
            {
                return 18;
            }
            else if (degrees > 0.0006866455078125)
            {
                return 19;
            }
            return 20;
        }

        /*private int RadiusToZoomLevel(double meters)
        {
            double renderedLength = Math.Min(this.Control.ActualWidth, this.Control.ActualHeight);
            double mperp = (meters*2) / renderedLength;
            if(mperp<=.30)
            {
                return 19;
            }
            else if (mperp <= .60)
            {
                return 18;
            }
            else if (mperp <= 1.19)
            {
                return 17;
            }
            else if (mperp <= 2.39)
            {
                return 16;
            }
            else if (mperp <= 4.78)
            {
                return 15;
            }
            else if (mperp <=9.55)
            {
                return 14;
            }
            else if (mperp <= 19.11)
            {
                return 13;
            }
            else if (mperp <= 38.22)
            {
                return 12;
            }
            else if (mperp <= 76.44)
            {
                return 11;
            }
            else if (mperp <= 152.87)
            {
                return 10;
            }
            else if (mperp <= 305.75)
            {
                return 9;
            }
            else if (mperp <= 611.50)
            {
                return 8;
            }
            else if (mperp <= 1222.99)
            {
                return 7;
            }
            else if (mperp <= 2445.98)
            {
                return 6;
            }
            else if (mperp <= 4891.97)
            {
                return 5;
            }
            else if (mperp <= 9783.94)
            {
                return 4;
            }
            else if (mperp <= 19567.88)
            {
                return 3;
            }
            else if (mperp <= 39135.76)
            {
                return 2;
            }
            else if (mperp <= 78271.52)
            {
                return 1;
            }
            
            return 0;
        }*/
    }
}