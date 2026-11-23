using LibraryManagement.Auth.Extensions;
using LibraryManagement.Models.DTOs.Notices;
using LibraryManagement.Models.Enums;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Admin
{
    // Admin: important notices shown to everyone in the bar above the navbar
    [Route("api/admin/notices")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class AdminNoticesController : ControllerBase
    {
        private readonly ISiteNoticeService _noticeService;

        public AdminNoticesController(ISiteNoticeService noticeService)
        {
            _noticeService = noticeService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<SiteNoticeDto>>> GetAll(CancellationToken cancellationToken)
        {
            return Ok(await _noticeService.GetAllAsync(cancellationToken));
        }

        [HttpPost]
        public async Task<ActionResult<SiteNoticeDto>> Create(SiteNoticeRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _noticeService.SaveAsync(User.GetUserId(), null, request, cancellationToken));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<SiteNoticeDto>> Update(int id, SiteNoticeRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _noticeService.SaveAsync(User.GetUserId(), id, request, cancellationToken));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _noticeService.DeleteAsync(User.GetUserId(), id, cancellationToken);
            return NoContent();
        }
    }
}
