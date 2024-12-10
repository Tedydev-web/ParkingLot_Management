using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace ParkingLotManagement.Services
{
  public class HttpInterceptorService : DelegatingHandler
  {
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpInterceptorService(IHttpContextAccessor httpContextAccessor)
    {
      _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
      var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
      if (!string.IsNullOrEmpty(token))
      {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
      }

      return await base.SendAsync(request, cancellationToken);
    }
  }
}