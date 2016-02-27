using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.Reflection;
using System.Collections.Specialized;

#if WINDOWS_APP
using Bing.Maps;
#else
using Windows.UI.Xaml.Controls.Maps;
using Windows.Devices.Geolocation;
#endif

#if WINDOWS_UWP
using Xamarin.Forms.Platform.UWP;
[assembly: ExportRenderer(typeof(Xamarin.Forms.Maps.Map), typeof(InTheHand.Forms.Maps.Platform.UWP.MapRenderer))]

namespace InTheHand.Forms.Maps.Platform.UWP
#else
using Xamarin.Forms.Platform.WinRT;
[assembly: ExportRenderer(typeof(Xamarin.Forms.Maps.Map), typeof(InTheHand.Forms.Maps.Platform.WinRT.MapRenderer))]

namespace InTheHand.Forms.Maps.Platform.WinRT
#endif


{
#if WINDOWS_APP
    public class MapRenderer : Xamarin.Forms.Platform.WinRT.VisualElementRenderer<Xamarin.Forms.Maps.Map, Bing.Maps.Map>
#else
    public class MapRenderer : VisualElementRenderer<Xamarin.Forms.Maps.Map, Windows.UI.Xaml.Controls.Maps.MapControl>
#endif
    {
        private bool firstZoomLevelChangeFired;

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Maps.Map> e)
        {
            base.OnElementChanged(e);

            if(e.NewElement != null)
            {
                ((INotifyCollectionChanged)e.NewElement.Pins).CollectionChanged += MapRenderer_CollectionChanged;
#if WINDOWS_APP
                
                SetNativeControl(new Bing.Maps.Map());
                Control.ViewChanged += Control_ViewChanged;
                Control.ManipulationMode = Element.HasScrollEnabled ? Control.ManipulationMode & Windows.UI.Xaml.Input.ManipulationModes.Scale : Control.ManipulationMode ^ Windows.UI.Xaml.Input.ManipulationModes.Scale;

                Control.MapType = MapTypeToBingMapType(e.NewElement.MapType);
                controlReadyForMoveUpdates = true;
#else
                SetNativeControl(new Windows.UI.Xaml.Controls.Maps.MapControl());
                Control.ZoomLevelChanged += Control_ZoomLevelChanged;
                Control.CenterChanged += Control_CenterChanged;
#if WINDOWS_UWP
                Control.MapElementClick += Control_MapElementClick;
#endif
                Control.LoadingStatusChanged += Control_LoadingStatusChanged;
                
                if(!string.IsNullOrEmpty(FormsMaps._serviceToken ))
                {
                    Control.MapServiceToken = FormsMaps._serviceToken;
                }
                Control.Style = MapTypeToMapStyle(e.NewElement.MapType);
#endif
                if (!string.IsNullOrEmpty(FormsMaps._serviceToken))
                {
#if WINDOWS_APP
                    Control.Credentials = FormsMaps._serviceToken;
#else
                    Control.MapServiceToken = FormsMaps._serviceToken;
#endif
                }
                Xamarin.Forms.MessagingCenter.Subscribe<Xamarin.Forms.Maps.Map, Xamarin.Forms.Maps.MapSpan>(this, "MapMoveToRegion", (Action<Xamarin.Forms.Maps.Map, Xamarin.Forms.Maps.MapSpan>)((s, a) => this.MoveToRegion(a)), Element);
      
                AddPins();        
            }
            else if(e.OldElement != null)
            {
                ((INotifyCollectionChanged)e.OldElement.Pins).CollectionChanged -= MapRenderer_CollectionChanged;
            }
        }

#if WINDOWS_UWP
        private void Control_MapElementClick(MapControl sender, MapElementClickEventArgs args)
        {
            MapIcon i = args.MapElements[0] as MapIcon;
            if (i != null)
            {
                foreach (Pin p in Element.Pins)
                {
                    if (p.Position.Latitude == i.Location.Position.Latitude && p.Position.Longitude == i.Location.Position.Longitude)
                    {
                        _tapMethod.Invoke(p, new object[0]);
                        break;
                    }
                }
            }
        }
#endif

        bool controlReadyForMoveUpdates = false;

#if !WINDOWS_APP
        private void Control_LoadingStatusChanged(MapControl sender, object args)
        {
            controlReadyForMoveUpdates = Control.LoadingStatus == MapLoadingStatus.Loaded;
        }
#endif

        private void AddPins()
        {
            foreach(Pin p in Element.Pins)
            {
#if WINDOWS_APP
                Pushpin pp = new Pushpin();
                pp.Text = p.Label;
                pp.Tag = p;
                pp.SetValue(Bing.Maps.MapLayer.PositionProperty, new Location(p.Position.Latitude, p.Position.Longitude));
                pp.Tapped += Pp_Tapped;
                Control.Children.Add(pp);
#else
                MapIcon mi = new MapIcon() { Location = new Geopoint(new BasicGeoposition() { Latitude = p.Position.Latitude, Longitude = p.Position.Longitude }), Title = p.Label, NormalizedAnchorPoint = new Windows.Foundation.Point(0.5,1.0) };
                Control.MapElements.Add(mi);                  
#endif
            }
        }

        MethodInfo _tapMethod = typeof(Pin).GetTypeInfo().GetDeclaredMethod("SendTap");

#if WINDOWS_APP

        
        private void Pp_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Pushpin pp = sender as Pushpin;
            Pin p = pp.Tag as Pin;
            _tapMethod.Invoke(p,new object[0]);
        }
#endif

        void MapRenderer_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
#if WINDOWS_APP
                    Control.Children.Clear();
#else
                    Control.MapElements.Clear();
#endif
                    break;
                case NotifyCollectionChangedAction.Add:
                    Pin p = e.NewItems[0] as Pin;
#if WINDOWS_APP
                    Pushpin pp = new Pushpin();
                    pp.Text = p.Label;
                    pp.Tag = p;
                    pp.Tapped += Pp_Tapped;
                    pp.SetValue(Bing.Maps.MapLayer.PositionProperty, new Location(p.Position.Latitude, p.Position.Longitude));

                    Control.Children.Add(pp);
#else
                    Control.MapElements.Insert(e.NewStartingIndex, new MapIcon(){ Location = new Geopoint(new BasicGeoposition(){ Latitude=p.Position.Latitude, Longitude=p.Position.Longitude}), Title = p.Label, NormalizedAnchorPoint = new Windows.Foundation.Point(0.5, 1.0) });
#endif
                    break;
                case NotifyCollectionChangedAction.Move:
#if WINDOWS_APP
#else
                    MapIcon i = Control.MapElements[e.OldStartingIndex] as MapIcon;
                    Control.MapElements.RemoveAt(e.OldStartingIndex);
                    Control.MapElements.Insert(e.NewStartingIndex, i);
#endif
                    break;
                case NotifyCollectionChangedAction.Remove:
#if WINDOWS_APP
                    Control.Children.RemoveAt(e.OldStartingIndex);
#else
                    Control.MapElements.RemoveAt(e.OldStartingIndex);
#endif
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Pin rp = e.NewItems[0] as Pin;
#if WINDOWS_APP
                    Control.Children.RemoveAt(e.OldStartingIndex);

                    Pushpin rpp = new Pushpin();
                    rpp.Text = rp.Label;
                    rpp.Tag = rp;
                    rpp.Tapped += Pp_Tapped;
                    rpp.SetValue(Bing.Maps.MapLayer.PositionProperty, new Location(rp.Position.Latitude, rp.Position.Longitude));

                    Control.Children.Insert(e.OldStartingIndex, rpp);
#else
                    Control.MapElements.RemoveAt(e.OldStartingIndex);
                    
                    Control.MapElements.Insert(e.OldStartingIndex, new MapIcon(){ Location = new Geopoint(new BasicGeoposition(){ Latitude=rp.Position.Latitude, Longitude=rp.Position.Longitude}), Title = rp.Label, NormalizedAnchorPoint = new Windows.Foundation.Point(0.5, 1.0) });
#endif
                    break;
            }
        }

#if WINDOWS_APP
        private void Control_ViewChanged(object sender, ViewChangedEventArgs e)
        {
            UpdateVisibleRegion();
        }
#elif WINDOWS_PHONE_APP || WINDOWS_UWP
        void Control_CenterChanged(Windows.UI.Xaml.Controls.Maps.MapControl sender, object args)
        {
            UpdateVisibleRegion();
        }

        void Control_ZoomLevelChanged(Windows.UI.Xaml.Controls.Maps.MapControl sender, object args)
        {
            UpdateVisibleRegion();
        }
#endif
        private PropertyInfo _lmtrProperty = null;
        private PropertyInfo _vrProperty = null;

        private void UpdateVisibleRegion()
        {
            if (!controlReadyForMoveUpdates)
                return;


            if (!this.firstZoomLevelChangeFired)
            {
                MapSpan region = null;
                if (_lmtrProperty == null)
                {
                    foreach (PropertyInfo pi in typeof(Xamarin.Forms.Maps.Map).GetRuntimeProperties())
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
#if WINDOWS_APP
                Position center = new Position(Control.Center.Latitude, Control.Center.Longitude);
#else
                Position center = new Position(Control.Center.Position.Latitude, Control.Center.Position.Longitude);
#endif
                if (_vrProperty == null)
                {
                    foreach (PropertyInfo pi in typeof(Xamarin.Forms.Maps.Map).GetRuntimeProperties())
                    {
                        if (pi.Name == "VisibleRegion")
                        {
                            _vrProperty = pi;
                            break;
                        }
                    }
                }

                // TODO: work out zoom conversion
#if WINDOWS_APP
                _vrProperty.SetValue(Element, new MapSpan(center, Control.Bounds.Height, Control.Bounds.Width));
#else
                Geopoint point;
                Control.GetLocationFromOffset(new Windows.Foundation.Point(0.0, 0.0), out point);
                double lat = (point.Position.Latitude - center.Latitude) * 2;
                double lon = (point.Position.Longitude - center.Longitude) * 2;
                _vrProperty.SetValue(Element, new MapSpan(center, lat, lon));
#endif

            }
        }

        public override Xamarin.Forms.SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            return new Xamarin.Forms.SizeRequest(new Size(100, 100));
        }

        private async void MoveToRegion(MapSpan span)
        {
#if WINDOWS_APP
            Control.SetView(new LocationRect(new Location(span.Center.Latitude, span.Center.Longitude), span.LongitudeDegrees, span.LatitudeDegrees));
#else
            var nw = new BasicGeoposition()
            {
                Latitude = span.Center.Latitude + span.LatitudeDegrees / 2,
                Longitude = span.Center.Longitude - span.LongitudeDegrees / 2,
            };
            var se = new BasicGeoposition()
            {
                Latitude = span.Center.Latitude - span.LatitudeDegrees / 2,
                Longitude = span.Center.Longitude + span.LongitudeDegrees / 2
            };

            try
            {
                //_suspendUpdateVisibleRegion = true;
                await this.Control.TrySetViewBoundsAsync(new GeoboundingBox(nw, se), null, MapAnimationKind.Bow);
            }
            finally
            {
                //_suspendUpdateVisibleRegion = false;
            }

            //await Control.TrySetViewAsync(new Windows.Devices.Geolocation.Geopoint(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = span.Center.Latitude, Longitude = span.Center.Longitude }), DegreesToZoomLevel(Math.Max(span.LatitudeDegrees, span.LatitudeDegrees)));
#endif
        }


        protected async override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.PropertyName);

            switch(e.PropertyName)
            {
                case "MapType":
#if WINDOWS_APP
                    Control.MapType = MapTypeToBingMapType(Element.MapType);
#else
                    Control.Style = MapTypeToMapStyle(Element.MapType);
#endif
                    break;

                case "HasScrollEnabled":
#if WINDOWS_APP
                    Control.ManipulationMode = Element.HasScrollEnabled ? Control.ManipulationMode & Windows.UI.Xaml.Input.ManipulationModes.Scale : Control.ManipulationMode ^ Windows.UI.Xaml.Input.ManipulationModes.Scale;
#else
#endif
                    break;
            }
            base.OnElementPropertyChanged(sender, e);
        }

#if WINDOWS_APP
        private static Bing.Maps.MapType MapTypeToBingMapType(Xamarin.Forms.Maps.MapType type)
        {
            switch (type)
            {
                case Xamarin.Forms.Maps.MapType.Street:
                    return Bing.Maps.MapType.Road;

                case Xamarin.Forms.Maps.MapType.Satellite:
                    return Bing.Maps.MapType.Aerial;

                case Xamarin.Forms.Maps.MapType.Hybrid:
                    return Bing.Maps.MapType.Birdseye;
                   
                default:
                    return Bing.Maps.MapType.Empty;
            }
        }
#else
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
#endif
  
    }
}
