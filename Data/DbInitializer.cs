using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ParkingLotManagement.Data;
using ParkingLotManagement.Models;
using System;
using System.Threading.Tasks;

public static class DbInitializer
{
  public static async Task Initialize(IServiceProvider serviceProvider)
  {
    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    context.Database.EnsureCreated();

    // Kiểm tra và tạo role Admin nếu chưa có
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
      await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Tạo tài khoản Admin mặc định nếu chưa có
    if (await userManager.FindByEmailAsync("admin@example.com") == null)
    {
      var admin = new ApplicationUser
      {
        UserName = "admin@example.com",
        Email = "admin@example.com",
        FirstName = "Admin",
        LastName = "User",
        EmailConfirmed = true,
        IsActive = true
      };

      var result = await userManager.CreateAsync(admin, "Admin@123");
      if (result.Succeeded)
      {
        await userManager.AddToRoleAsync(admin, "Admin");
      }
    }
  }
}