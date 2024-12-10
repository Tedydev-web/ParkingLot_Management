using System.Collections.Generic;
using System.Threading.Tasks;
using ParkingLotManagement.Models;
using ParkingLotManagement.DTOs;

namespace ParkingLotManagement.Services
{
    public interface IParkingLotService
    {
        Task<IEnumerable<ParkingLot>> GetAllAsync();
        Task<ParkingLot?> GetByIdAsync(int id);
        Task<ParkingLot?> CreateAsync(CreateParkingLotDto dto);
        Task<ParkingLot?> UpdateAsync(int id, UpdateParkingLotDto dto);
        Task DeleteAsync(int id);
        Task<IEnumerable<ParkingLot>> GetNearbyAsync(double lat, double lng, double radiusKm);
    }
}
