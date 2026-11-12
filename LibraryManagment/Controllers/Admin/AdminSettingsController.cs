using LibraryManagement.Auth.Extensions;
using LibraryManagement.Models.DTOs.Settings;
using LibraryManagement.Models.Enums;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Admin
{
    // Admin: business settings (library name, monthly fee, hold time, billing rules)
    [Route("api/admin/settings")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class AdminSettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;

        public AdminSettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AdminSettingDto>>> GetAll(CancellationToken cancellationToken)
        {
            return Ok(await _settingsService.GetAllForAdminAsync(cancellationToken));
        }

        // Send only the settings that changed. Returns every setting after the update.
        [HttpPut]
        public async Task<ActionResult<IReadOnlyList<AdminSettingDto>>> Update(UpdateSettingsRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _settingsService.UpdateAsync(User.GetUserId(), request.Values, cancellationToken));
        }
    }
}
