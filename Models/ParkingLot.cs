using System;
using System.ComponentModel.DataAnnotations;

namespace ParkingLotManagement.Models
{
  public class ParkingLot
  {
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required]
    public double Latitude { get; set; }

    [Required]
    public double Longitude { get; set; }

    [Required]
    public int Capacity { get; set; }

    public int AvailableSpots { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public string PlaceId { get; set; } = string.Empty; // Từ Goong Places API
    public string IconUrl { get; set; } = string.Empty; // Icon cho marker
    public string MarkerColor { get; set; } = "#FF0000";

    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
  }
}
