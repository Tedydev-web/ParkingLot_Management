namespace ParkingLotManagement.Configurations
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int AccessTokenExpiryInMinutes { get; set; } = 15;
        public int RefreshTokenExpiryInDays { get; set; } = 7;
    }
} 