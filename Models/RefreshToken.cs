using System;
using ParkingLotManagement.Models;

namespace ParkingLotManagement.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public bool IsRevoked => RevokedAt != null;
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
} 