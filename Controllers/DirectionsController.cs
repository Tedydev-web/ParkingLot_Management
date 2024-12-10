using Microsoft.AspNetCore.Mvc;
using ParkingLotManagement.Services;

namespace ParkingLotManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DirectionsController : ControllerBase
    {
        private readonly IGoongMapService _goongMapService;

        public DirectionsController(IGoongMapService goongMapService)
        {
            _goongMapService = goongMapService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDirections(
            [FromQuery] double fromLat,
            [FromQuery] double fromLng,
            [FromQuery] double toLat,
            [FromQuery] double toLng)
        {
            var result = await _goongMapService.GetDirectionsAsync(fromLat, fromLng, toLat, toLng);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
