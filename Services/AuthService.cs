using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ParkingLotManagement.Models;
using ParkingLotManagement.Configurations;
using ParkingLotManagement.DTOs;
using Microsoft.EntityFrameworkCore;
using ParkingLotManagement.Data;

namespace ParkingLotManagement.Services
{
  public interface IAuthService
  {
    Task<(bool success, TokenDto? result)> LoginAsync(string email, string password);
    Task<(bool success, string message)> RegisterAsync(string email, string password, string firstName, string lastName, string role = "User");
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<bool> UpdateUserAsync(string userId, UpdateUserDto dto);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    Task<bool> ToggleUserStatusAsync(string userId);
    Task<(bool success, TokenDto? tokens)> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);
  }

  public class AuthService : IAuthService
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtSettings _jwtSettings;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthService> _logger;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IOptions<JwtSettings> jwtSettings,
        ApplicationDbContext context,
        ILogger<AuthService> logger,
        RoleManager<IdentityRole> roleManager)
    {
      _userManager = userManager;
      _jwtSettings = jwtSettings.Value;
      _context = context;
      _logger = logger;
      _roleManager = roleManager;
    }

    public async Task<(bool success, TokenDto? result)> LoginAsync(string email, string password)
    {
      var user = await _userManager.FindByEmailAsync(email);
      if (user == null || !await _userManager.CheckPasswordAsync(user, password))
      {
        return (false, null);
      }

      var accessToken = await GenerateJwtTokenAsync(user);
      var refreshToken = await GenerateRefreshTokenAsync(user);

      return (true, new TokenDto
      {
        AccessToken = accessToken,
        RefreshToken = refreshToken.Token,
        AccessTokenExpiresIn = _jwtSettings.AccessTokenExpiryInMinutes * 60,
        RefreshTokenExpiresAt = refreshToken.ExpiresAt
      });
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user)
    {
      var refreshToken = new RefreshToken
      {
        Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
        ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays),
        CreatedAt = DateTime.UtcNow,
        UserId = user.Id
      };

      user.RefreshTokens.Add(refreshToken);
      await _userManager.UpdateAsync(user);

      return refreshToken;
    }

    public async Task<(bool success, TokenDto? tokens)> RefreshTokenAsync(string refreshToken)
    {
      try
      {
        var storedToken = await _context.RefreshTokens
          .Include(r => r.User)
          .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (storedToken == null || storedToken.IsRevoked || storedToken.IsExpired)
          return (false, null);

        var user = storedToken.User;
        var newAccessToken = await GenerateJwtTokenAsync(user);
        var newRefreshToken = await GenerateRefreshTokenAsync(user);

        storedToken.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, new TokenDto
        {
          AccessToken = newAccessToken,
          RefreshToken = newRefreshToken.Token,
          AccessTokenExpiresIn = _jwtSettings.AccessTokenExpiryInMinutes * 60,
          RefreshTokenExpiresAt = newRefreshToken.ExpiresAt
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Lỗi khi refresh token");
        return (false, null);
      }
    }

    public async Task<(bool success, string message)> RegisterAsync(
        string email, string password, string firstName, string lastName, string role = "User")
    {
      using var transaction = await _context.Database.BeginTransactionAsync();
      try
      {
        var user = new ApplicationUser
        {
          UserName = email,
          Email = email,
          FirstName = firstName,
          LastName = lastName,
          IsActive = true,
          CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
          return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        if (!await _roleManager.RoleExistsAsync(role))
        {
          await _roleManager.CreateAsync(new IdentityRole(role));
        }

        await _userManager.AddToRoleAsync(user, role);
        
        await transaction.CommitAsync();
        return (true, "Đăng ký thành công");
      }
      catch (Exception ex)
      {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Lỗi khi đăng ký user");
        return (false, "Đã xảy ra lỗi khi đăng ký");
      }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

      try
      {
        var result = await Task.FromResult(tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(key),
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidIssuer = _jwtSettings.Issuer,
          ValidAudience = _jwtSettings.Audience,
          ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken));

        return true;
      }
      catch
      {
        return false;
      }
    }

    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
      var roles = await _userManager.GetRolesAsync(user);

      var claims = new List<Claim>
      {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
        new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
        new Claim("fullName", user.FullName),
        new Claim("isActive", user.IsActive.ToString()),
        new Claim("isVerified", user.IsVerified.ToString())
      };

      claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryInMinutes),
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature),
        Issuer = _jwtSettings.Issuer,
        Audience = _jwtSettings.Audience
      };

      var token = tokenHandler.CreateToken(tokenDescriptor);
      return tokenHandler.WriteToken(token);
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
      var user = await _userManager.FindByIdAsync(userId);
      if (user == null) return null;

      var roles = await _userManager.GetRolesAsync(user);

      return new UserDto
      {
        Id = user.Id,
        Email = user.Email ?? string.Empty,
        PhoneNumber = user.PhoneNumber ?? string.Empty,
        FirstName = user.FirstName,
        LastName = user.LastName,
        FullName = user.FullName,
        Avatar = user.Avatar,
        Address = user.Address,
        IsVerified = user.IsVerified,
        IsActive = user.IsActive,
        Roles = roles.ToList()
      };
    }

    public async Task<bool> UpdateUserAsync(string userId, UpdateUserDto dto)
    {
      var user = await _userManager.FindByIdAsync(userId);
      if (user == null) return false;

      if (dto.FirstName != null) user.FirstName = dto.FirstName;
      if (dto.LastName != null) user.LastName = dto.LastName;
      if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
      if (dto.Address != null) user.Address = dto.Address;
      if (dto.Avatar != null) user.Avatar = dto.Avatar;

      user.UpdatedAt = DateTime.UtcNow;

      var result = await _userManager.UpdateAsync(user);
      return result.Succeeded;
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
      var user = await _userManager.FindByIdAsync(userId);
      if (user == null) return false;

      var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
      return result.Succeeded;
    }

    public async Task<bool> ToggleUserStatusAsync(string userId)
    {
      var user = await _userManager.FindByIdAsync(userId);
      if (user == null) return false;

      user.IsActive = !user.IsActive;
      user.UpdatedAt = DateTime.UtcNow;

      var result = await _userManager.UpdateAsync(user);
      return result.Succeeded;
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
      var user = await _userManager.Users
        .Include(u => u.RefreshTokens)
        .FirstOrDefaultAsync(u =>
          u.RefreshTokens.Any(rt =>
            rt.Token == refreshToken &&
            !rt.IsRevoked &&
            !rt.IsExpired));

      if (user == null)
        return false;

      var oldRefreshToken = user.RefreshTokens.First(rt => rt.Token == refreshToken);
      oldRefreshToken.RevokedAt = DateTime.UtcNow;

      await _userManager.UpdateAsync(user);

      return true;
    }
  }
}