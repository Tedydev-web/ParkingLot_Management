using System.ComponentModel.DataAnnotations;

namespace ParkingLotManagement.DTOs
{
    public class CreateParkingLotDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Address { get; set; } = string.Empty;
        
        [Required]
        public double Latitude { get; set; }
        
        [Required]
        public double Longitude { get; set; }
        
        [Required]
        public int Capacity { get; set; }
        
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateParkingLotDto : CreateParkingLotDto
    {
        public int AvailableSpots { get; set; }
        public bool IsActive { get; set; }
    }
}
