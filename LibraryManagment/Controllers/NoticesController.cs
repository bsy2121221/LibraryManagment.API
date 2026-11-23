using LibraryManagement.Models.DTOs.Notices;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers
{
    [Route("api/notices")]
    [ApiController]
    public class NoticesController : ControllerBase
    {
        private readonly ISiteNoticeService _noticeService;

        public NoticesController(ISiteNoticeService noticeService)
        {
            _noticeService = noticeService;
        }

        // Admin notices showing today (rotating bar above the navbar, visible to everyone)
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IReadOnlyList<PublicNoticeDto>>> GetCurrent(CancellationToken cancellationToken)
        {
            return Ok(await _noticeService.GetCurrentAsync(cancellationToken));
        }
    }
}
