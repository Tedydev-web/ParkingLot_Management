using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ParkingLotManagement.Helpers
{
    public static class AuthenticationHelper
    {
        public static string GetToken(this HttpContext context)
        {
            return context.Request.Headers["Authorization"]
                .FirstOrDefault()?.Replace("Bearer ", "");
        }

        public static void SetToken(this HttpContext context, string token)
        {
            context.Request.Headers["Authorization"] = $"Bearer {token}";
        }

        public static void RemoveToken(this HttpContext context)
        {
            context.Request.Headers.Remove("Authorization");
        }
    }
} 