using Microsoft.AspNetCore.Mvc;
using ParkingLotManagement.Services;
using ParkingLotManagement.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ParkingLotManagement.Configurations;
using Microsoft.Extensions.Options;
using ParkingLotManagement.Helpers;

namespace ParkingLotManagement.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly JwtSettings _jwtSettings;

    public AuthController(
        IAuthService authService,
        ILogger<AuthController> logger,
        IOptions<JwtSettings> jwtSettings)
    {
      _authService = authService;
      _logger = logger;
      _jwtSettings = jwtSettings.Value;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
      var (success, result) = await _authService.LoginAsync(model.Email, model.Password);
      if (!success || result == null)
        return BadRequest(new { message = "Invalid credentials" });

      HttpContext.SetToken(result.AccessToken);

      return Ok(new
      {
        token = result.AccessToken,
        refreshToken = result.RefreshToken,
        expiresIn = _jwtSettings.AccessTokenExpiryInMinutes * 60
      });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
      HttpContext.RemoveToken();
      return Ok();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
      (bool success, string message) = await _authService.RegisterAsync(
          model.Email,
          model.Password,
          model.FirstName,
          model.LastName,
          model.Role);

      if (!success)
        return BadRequest(new { message });

      return Ok(new { message });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userId))
        return Unauthorized();

      var user = await _authService.GetUserByIdAsync(userId);
      if (user == null)
        return NotFound();

      return Ok(user);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto dto)
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userId))
        return Unauthorized();

      var success = await _authService.UpdateUserAsync(userId, dto);
      if (!success)
        return BadRequest("Failed to update profile");

      return Ok();
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
      if (dto.NewPassword != dto.ConfirmPassword)
        return BadRequest("Passwords do not match");

      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userId))
        return Unauthorized();

      var success = await _authService.ChangePasswordAsync(userId, dto);
      if (!success)
        return BadRequest("Failed to change password");

      return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("users/{userId}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(string userId)
    {
      var success = await _authService.ToggleUserStatusAsync(userId);
      if (!success)
        return BadRequest("Failed to toggle user status");

      return Ok();
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
      var (success, tokens) = await _authService.RefreshTokenAsync(dto.RefreshToken);
      if (!success)
        return BadRequest("Invalid refresh token");

      return Ok(tokens);
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenDto dto)
    {
      var success = await _authService.RevokeRefreshTokenAsync(dto.RefreshToken);
      if (!success)
        return BadRequest("Invalid refresh token");

      return Ok();
    }
  }
}