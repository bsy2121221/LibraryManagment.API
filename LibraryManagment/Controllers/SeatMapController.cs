using LibraryManagement.Auth.Extensions;
using LibraryManagement.Models.DTOs.SeatMap;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers
{
    [Route("api/seat-map")]
    [ApiController]
    public class SeatMapController : ControllerBase
    {
        private readonly ISeatMapService _seatMapService;

        public SeatMapController(ISeatMapService seatMapService)
        {
            _seatMapService = seatMapService;
        }

        // Public seat map of every active section. If a valid token is sent, the caller's own seat has IsMine = true.
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<SeatMapDto>> Get(CancellationToken cancellationToken)
        {
            return Ok(await _seatMapService.GetAsync(User.GetUserIdOrNull(), cancellationToken));
        }
    }
}
