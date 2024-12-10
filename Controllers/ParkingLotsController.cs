using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ParkingLotManagement.Services;
using ParkingLotManagement.Models;
using ParkingLotManagement.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace ParkingLotManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ParkingLotsController : ControllerBase
    {
        private readonly IParkingLotService _parkingLotService;
        private readonly ILogger<ParkingLotsController> _logger;

        public ParkingLotsController(IParkingLotService parkingLotService, ILogger<ParkingLotsController> logger)
        {
            _parkingLotService = parkingLotService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ParkingLot>>> GetAll()
        {
            var parkingLots = await _parkingLotService.GetAllAsync();
            return Ok(parkingLots);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ParkingLot>> GetById(int id)
        {
            var parkingLot = await _parkingLotService.GetByIdAsync(id);
            if (parkingLot == null) return NotFound();
            return Ok(parkingLot);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ParkingLot>> Create(CreateParkingLotDto dto)
        {
            var parkingLot = await _parkingLotService.CreateAsync(dto);
            if (parkingLot == null)
            {
                return BadRequest("Không thể xác thực địa chỉ với Goong Map");
            }
            return CreatedAtAction(nameof(GetById), new { id = parkingLot.Id }, parkingLot);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ParkingLot>> Update(int id, UpdateParkingLotDto dto)
        {
            var parkingLot = await _parkingLotService.UpdateAsync(id, dto);
            if (parkingLot == null) return NotFound();
            return Ok(parkingLot);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _parkingLotService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("nearby")]
        public async Task<ActionResult<IEnumerable<ParkingLot>>> GetNearby(
            [FromQuery] double lat,
            [FromQuery] double lng,
            [FromQuery] double radiusKm = 1.0)
        {
            try 
            {
                if (radiusKm <= 0 || radiusKm > 10)
                    return BadRequest("Bán kính tìm kiếm phải từ 0-10km");

                var parkingLots = await _parkingLotService.GetNearbyAsync(lat, lng, radiusKm);
                return Ok(parkingLots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm bãi đỗ xe gần đây");
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu");
            }
        }
    }
}
