using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Xamarin.Forms.Maps;
#if WINDOWS_APP
using Bing.Maps;
using Bing.Maps.Search;
#else
using Windows.Services.Maps;
#endif

namespace InTheHand.Forms.Maps.Platform.WinRT
{
    internal static class GeocoderBackend
    {
#if WINDOWS_APP
        private static SearchManager _manager;

        private static void CreateSearchManager()
        {
            if(_manager == null)
            {
                Bing.Maps.Map m = new Bing.Maps.Map();

                if (!string.IsNullOrEmpty(InTheHand.FormsMaps._serviceToken))
                {
                    m.Credentials = InTheHand.FormsMaps._serviceToken;
                }
                _manager = m.SearchManager;
            }
        }
#endif
        internal static Position PositionForGeocoding { get; set; }

        public static void Register()
        {
#if WINDOWS_APP
            
#else
            if (!string.IsNullOrEmpty(InTheHand.FormsMaps._serviceToken))
            {
                MapService.ServiceToken = InTheHand.FormsMaps._serviceToken;
            }
#endif
            foreach(FieldInfo fi in typeof(Geocoder).GetRuntimeFields())
            {
                switch(fi.Name)
                {
                    case "GetPositionsForAddressAsyncFunc":
                        fi.SetValue(null, new Func<string, Task<IEnumerable<Position>>>(GeocoderBackend.GetPositionsForAddressAsync));
                        break;

                    case "GetAddressesForPositionFuncAsync":
                        fi.SetValue(null, new Func<Position, Task<IEnumerable<string>>>(GeocoderBackend.GetAddressesForPositionAsync));
                        break;
                }
            }
                    }

#if WINDOWS_PHONE_APP
        private static string AddressToString(MapAddress address)
        {
            string str1 = "";
            string str2 = "";
            string str3 = "";
            string str4 = "";
            string str5 = "";
            string str6 = "";
            List<string> list1 = new List<string>();
            if (!string.IsNullOrEmpty(address.BuildingRoom))
                list1.Add(address.BuildingRoom);
            if (!string.IsNullOrEmpty(address.BuildingFloor))
                list1.Add(address.BuildingFloor);
            if (!string.IsNullOrEmpty(address.BuildingName))
                list1.Add(address.BuildingName);
            if (!string.IsNullOrEmpty(address.BuildingWing))
                list1.Add(address.BuildingWing);
            if (list1.Count > 0)
                str1 = string.Join(" ", (IEnumerable<string>)list1) + ", ";

            List<string> list2 = new List<string>();
            if (!string.IsNullOrEmpty(address.StreetNumber))
                list2.Add(address.StreetNumber);
            if (!string.IsNullOrEmpty(address.Street))
                list2.Add(address.Street);
            if (list2.Count > 0)
                str2 = string.Join(" ", (IEnumerable<string>)list2) + ", ";

            if (!string.IsNullOrEmpty(address.Town))
                str3 = address.Town + ", ";
            if (!string.IsNullOrEmpty(address.Region))
                str4 = address.Region + ", ";
            if (!string.IsNullOrEmpty(address.PostCode))
                str5 = address.PostCode + ", ";
            if (!string.IsNullOrEmpty(address.Country))
                str6 = address.Country;
            return str1 + str2 + str3 + str4 + str5 + str6;
        }
#endif
        private static async Task<IEnumerable<string>> GetAddressesForPositionAsync(Position position)
        {
            List<string> results = new List<string>();
#if WINDOWS_APP
            CreateSearchManager();
            Location location = new Location(position.Latitude, position.Longitude);
            ReverseGeocodeRequestOptions options = new ReverseGeocodeRequestOptions(location);
            
            //options.IncludeEntityTypeFlags = ReverseGeocodeEntityType.Address | ReverseGeocodeEntityType.Neighborhood | ReverseGeocodeEntityType.Postcode | ReverseGeocodeEntityType.PopulatedPlace;

            LocationDataResponse response = await _manager.ReverseGeocodeAsync(options);
            foreach(GeocodeLocation l in response.LocationData)
            {
                results.Add(l.Address.FormattedAddress);
            }
#else
            MapLocationFinderResult result = await MapLocationFinder.FindLocationsAtAsync(new Windows.Devices.Geolocation.Geopoint(new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = position.Latitude, Longitude = position.Longitude }));
            if(result.Status  == MapLocationFinderStatus.Success)
            {
                foreach(MapLocation location in result.Locations)
                {
                    if(location.Address != null)
                    {
                        results.Add(AddressToString(location.Address));
                    }
                }
            }
#endif
            return results;
        }

        private static async Task<IEnumerable<Position>> GetPositionsForAddressAsync(string s)
        {
            List<Position> results = new List<Position>();
#if WINDOWS_APP
            CreateSearchManager();
            GeocodeRequestOptions options = new GeocodeRequestOptions(s);
            LocationDataResponse response = await _manager.GeocodeAsync(options);
            foreach(GeocodeLocation l in response.LocationData)
            {
                results.Add(new Position(l.Location.Latitude, l.Location.Longitude));
            }
#else
            MapLocationFinderResult result = await MapLocationFinder.FindLocationsAsync(s, new Windows.Devices.Geolocation.Geopoint(new Windows.Devices.Geolocation.BasicGeoposition()));
            if (result.Status == MapLocationFinderStatus.Success)
            {
                foreach (MapLocation location in result.Locations)
                {
                    if (location.Address != null)
                    {
                        results.Add(new Position(location.Point.Position.Latitude, location.Point.Position.Longitude));
                    }
                }
            }
#endif
            return results;
        }
    }
}
