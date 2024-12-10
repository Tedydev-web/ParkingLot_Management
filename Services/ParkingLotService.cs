using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkingLotManagement.Data;
using ParkingLotManagement.Models;
using ParkingLotManagement.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Security.Claims;

namespace ParkingLotManagement.Services
{
    public class ParkingLotService : IParkingLotService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGoongMapService _goongMapService;
        private readonly ILogger<ParkingLotService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ParkingLotService(
            ApplicationDbContext context,
            IGoongMapService goongMapService,
            ILogger<ParkingLotService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _goongMapService = goongMapService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        public async Task<IEnumerable<ParkingLot>> GetAllAsync()
        {
            return await _context.ParkingLots.Where(p => p.IsActive).ToListAsync();
        }

        public async Task<ParkingLot?> GetByIdAsync(int id)
        {
            return await _context.ParkingLots.FindAsync(id);
        }

        public async Task<ParkingLot?> CreateAsync(CreateParkingLotDto dto)
        {
            try
            {
                // Validate địa chỉ với Goong Geocoding
                var geocodeResult = await _goongMapService.GeocodeAddressAsync(dto.Address);
                if (geocodeResult?.Results.FirstOrDefault()?.PlaceId == null)
                {
                    _logger.LogWarning("Invalid address: {Address}", dto.Address);
                    return null;
                }

                var parkingLot = new ParkingLot
                {
                    Name = dto.Name,
                    Address = dto.Address,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    Capacity = dto.Capacity,
                    Description = dto.Description,
                    PlaceId = geocodeResult.Results[0].PlaceId ?? string.Empty,
                    AvailableSpots = dto.Capacity,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = GetCurrentUserId()
                };

                _context.ParkingLots.Add(parkingLot);
                await _context.SaveChangesAsync();
                return parkingLot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating parking lot");
                throw;
            }
        }

        public async Task<ParkingLot?> UpdateAsync(int id, UpdateParkingLotDto dto)
        {
            var parkingLot = await _context.ParkingLots.FindAsync(id);
            if (parkingLot == null) return null;

            parkingLot.Name = dto.Name;
            parkingLot.Address = dto.Address;
            parkingLot.Latitude = dto.Latitude;
            parkingLot.Longitude = dto.Longitude;
            parkingLot.Capacity = dto.Capacity;
            parkingLot.AvailableSpots = dto.AvailableSpots;
            parkingLot.Description = dto.Description;
            parkingLot.IsActive = dto.IsActive;
            parkingLot.UpdatedBy = GetCurrentUserId();
            parkingLot.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return parkingLot;
        }

        public async Task DeleteAsync(int id)
        {
            var parkingLot = await _context.ParkingLots.FindAsync(id);
            if (parkingLot != null)
            {
                parkingLot.IsActive = false;
                parkingLot.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ParkingLot>> GetNearbyAsync(double lat, double lng, double radiusKm)
        {
            // Tính toán bounding box
            var latChange = radiusKm / 111.32;
            var lngChange = Math.Abs(radiusKm / (111.32 * Math.Cos(lat * Math.PI / 180)));

            var minLat = lat - latChange;
            var maxLat = lat + latChange;
            var minLng = lng - lngChange;
            var maxLng = lng + lngChange;

            // Lấy danh sách trong bounding box
            var parkingLots = await _context.ParkingLots
                .Where(p => p.IsActive &&
                            p.Latitude >= minLat && p.Latitude <= maxLat &&
                            p.Longitude >= minLng && p.Longitude <= maxLng)
                .ToListAsync();

            // Lọc chính xác bằng khoảng cách thực
            return parkingLots.Where(p => 
                CalculateDistance(lat, lng, p.Latitude, p.Longitude) <= radiusKm);
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Bán kính trái đất (km)
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRad(double degree)
        {
            return degree * Math.PI / 180;
        }
    }
}
