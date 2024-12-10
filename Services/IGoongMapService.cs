using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ParkingLotManagement.Models.Goong;

namespace ParkingLotManagement.Services
{
  public interface IGoongMapService
  {
    Task<GeocodingResult?> GeocodeAddressAsync(string address);
    Task<GeocodingResult?> ReverseGeocodeAsync(double lat, double lng);
    Task<DistanceMatrix?> GetDistanceMatrixAsync(double fromLat, double fromLng, double toLat, double toLng);
    Task<PlacesResult?> SearchPlacesAsync(string keyword, string type = "parking");
    Task<PlacesAutocompleteResult?> AutocompletePlacesAsync(string input);
    Task<PlaceDetails?> GetPlaceDetailsAsync(string placeId);
    Task<DirectionsResult?> GetDirectionsAsync(double fromLat, double fromLng, double toLat, double toLng);
  }
}
