public class TokenDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int AccessTokenExpiresIn { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
}

public class RefreshTokenDto
{
    public string RefreshToken { get; set; } = string.Empty;
} 