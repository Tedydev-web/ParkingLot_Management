using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace ParkingLotManagement.Middleware
{
  public class RateLimitingMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private const int REQUESTS_PER_SECOND = 5;

    public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache)
    {
      _next = next;
      _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
      var key = context.Request.Path.ToString();
      var requests = _cache.Get<int>(key);

      if (requests >= REQUESTS_PER_SECOND)
      {
        context.Response.StatusCode = 429;
        await context.Response.WriteAsJsonAsync(new { message = "Rate limit exceeded" });
        return;
      }

      _cache.Set(key, requests + 1, TimeSpan.FromSeconds(1));
      await _next(context);
    }
  }
}
