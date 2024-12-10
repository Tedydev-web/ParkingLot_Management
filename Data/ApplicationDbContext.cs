using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ParkingLotManagement.Models;

namespace ParkingLotManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ParkingLot> ParkingLots { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ParkingLot>()
                .HasIndex(p => new { p.Latitude, p.Longitude });

            modelBuilder.Entity<RefreshToken>()
                .HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<ParkingLot>()
                .Property<string>("CreatedBy")
                .HasMaxLength(450);

            modelBuilder.Entity<ParkingLot>()
                .Property<string>("UpdatedBy")
                .HasMaxLength(450);
        }
    }
}
