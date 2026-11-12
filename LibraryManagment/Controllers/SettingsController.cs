using LibraryManagement.Models.DTOs.Settings;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers
{
    [Route("api/settings")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        // Library name, monthly fee and hold time, for the seat map and checkout
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<ActionResult<PublicSettingsDto>> GetPublic(CancellationToken cancellationToken)
        {
            return Ok(await _settingsService.GetPublicAsync(cancellationToken));
        }
    }
}
