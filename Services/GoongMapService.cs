using System.Text.Json;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ParkingLotManagement.Models.Goong;
using ParkingLotManagement.Exceptions;

namespace ParkingLotManagement.Services
{
  public class GoongMapService : IGoongMapService
  {
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly ILogger<GoongMapService> _logger;

    public GoongMapService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GoongMapService> logger)
    {
      _httpClient = httpClient;
      _logger = logger;
      _apiKey = configuration["GoongMap:ApiKey"]
          ?? throw new ArgumentNullException("GoongMap:ApiKey");
      _baseUrl = configuration["GoongMap:BaseUrl"]
          ?? "https://rsapi.goong.io/";
      _httpClient.BaseAddress = new Uri(_baseUrl);
    }

    private async Task<bool> ValidateApiKeyAsync()
    {
      try
      {
        var response = await _httpClient.GetAsync(
            $"Place/AutoComplete?input=test&api_key={_apiKey}");
        return response.StatusCode != HttpStatusCode.Forbidden;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to validate API key");
        return false;
      }
    }

    public async Task<GeocodingResult?> GeocodeAddressAsync(string address)
    {
      try
      {
        var response = await _httpClient.GetAsync(
            $"Geocode?address={Uri.EscapeDataString(address)}&api_key={_apiKey}");

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
          throw new Exception("Rate limit exceeded. Please try again later.");
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GeocodingResult>();
      }
      catch (HttpRequestException ex)
      {
        throw new Exception("Failed to geocode address", ex);
      }
    }

    public async Task<DistanceMatrix?> GetDistanceMatrixAsync(double fromLat, double fromLng, double toLat, double toLng)
    {
      var response = await _httpClient.GetAsync(
          $"DistanceMatrix?origins={fromLat},{fromLng}&destinations={toLat},{toLng}&vehicle=car&api_key={_apiKey}");

      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadFromJsonAsync<DistanceMatrix>();
      }
      return null;
    }

    public async Task<GeocodingResult?> ReverseGeocodeAsync(double lat, double lng)
    {
      var response = await _httpClient.GetAsync($"Geocode?latlng={lat},{lng}&api_key={_apiKey}");
      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadFromJsonAsync<GeocodingResult>();
      }
      return null;
    }

    public async Task<PlacesResult?> SearchPlacesAsync(string keyword, string type = "parking")
    {
      var response = await _httpClient.GetAsync(
          $"Place/AutoComplete?input={Uri.EscapeDataString(keyword)}&type={type}&api_key={_apiKey}");

      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadFromJsonAsync<PlacesResult>();
      }
      return null;
    }

    public async Task<PlacesAutocompleteResult?> AutocompletePlacesAsync(string input)
    {
      var response = await _httpClient.GetAsync(
          $"Place/AutoComplete?input={Uri.EscapeDataString(input)}&api_key={_apiKey}");

      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadFromJsonAsync<PlacesAutocompleteResult>();
      }
      return null;
    }

    public async Task<PlaceDetails?> GetPlaceDetailsAsync(string placeId)
    {
      var response = await _httpClient.GetAsync(
          $"Place/Detail?place_id={placeId}&api_key={_apiKey}");

      if (response.IsSuccessStatusCode)
      {
        return await response.Content.ReadFromJsonAsync<PlaceDetails>();
      }
      return null;
    }

    public async Task<DirectionsResult?> GetDirectionsAsync(double fromLat, double fromLng, double toLat, double toLng)
    {
      try
      {
        var response = await _httpClient.GetAsync(
            $"Direction?origin={fromLat},{fromLng}&destination={toLat},{toLng}" +
            $"&vehicle=car&alternatives=true&api_key={_apiKey}");

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
          throw new Exception("Rate limit exceeded. Please try again later.");
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DirectionsResult>();
      }
      catch (HttpRequestException ex)
      {
        throw new Exception("Failed to get directions", ex);
      }
    }
  }
}
