using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace ParkingLotManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? FacebookId { get; set; }
        public string? GithubId { get; set; }
        public string? Avatar { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public bool IsVerified => EmailConfirmed || PhoneNumberConfirmed;
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
} 