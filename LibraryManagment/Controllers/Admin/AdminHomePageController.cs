using LibraryManagement.Auth.Extensions;
using LibraryManagement.Models.DTOs.HomePage;
using LibraryManagement.Models.Enums;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Admin
{
    // Admin: home page builder. Every save is a new version; publishing makes a version live.
    [Route("api/admin/home-page")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class AdminHomePageController : ControllerBase
    {
        private readonly IHomePageService _homePageService;

        public AdminHomePageController(IHomePageService homePageService)
        {
            _homePageService = homePageService;
        }

        // Latest saved version (to continue editing) and which version is live
        [HttpGet]
        public async Task<ActionResult<AdminHomePageDto>> Get(CancellationToken cancellationToken)
        {
            return Ok(await _homePageService.GetForAdminAsync(cancellationToken));
        }

        [HttpPost]
        public async Task<ActionResult<HomePageVersionDto>> Save(SaveHomePageRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _homePageService.SaveAsync(User.GetUserId(), request.Layout, request.Publish, cancellationToken));
        }

        [HttpGet("versions")]
        public async Task<ActionResult<IReadOnlyList<HomePageVersionDto>>> GetVersions(CancellationToken cancellationToken)
        {
            return Ok(await _homePageService.GetHistoryAsync(cancellationToken));
        }

        [HttpGet("versions/{id:int}")]
        public async Task<ActionResult<HomePageVersionDto>> GetVersion(int id, CancellationToken cancellationToken)
        {
            return Ok(await _homePageService.GetVersionAsync(id, cancellationToken));
        }

        // Make a saved version live (also used to roll back)
        [HttpPost("versions/{id:int}/publish")]
        public async Task<ActionResult<HomePageVersionDto>> Publish(int id, CancellationToken cancellationToken)
        {
            return Ok(await _homePageService.PublishAsync(User.GetUserId(), id, cancellationToken));
        }
    }
}
