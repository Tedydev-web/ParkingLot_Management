using Microsoft.AspNetCore.Mvc;
using ParkingLotManagement.Services;

namespace ParkingLotManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeocodeController : ControllerBase
    {
        private readonly IGoongMapService _goongMapService;

        public GeocodeController(IGoongMapService goongMapService)
        {
            _goongMapService = goongMapService;
        }

        [HttpGet("reverse")]
        public async Task<IActionResult> ReverseGeocode([FromQuery] double lat, [FromQuery] double lng)
        {
            var result = await _goongMapService.ReverseGeocodeAsync(lat, lng);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
} 