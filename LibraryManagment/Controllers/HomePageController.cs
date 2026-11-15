using LibraryManagement.Models.DTOs.HomePage;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers
{
    [Route("api/home-page")]
    [ApiController]
    public class HomePageController : ControllerBase
    {
        private readonly IHomePageService _homePageService;

        public HomePageController(IHomePageService homePageService)
        {
            _homePageService = homePageService;
        }

        // The published home page design. 204 = nothing published yet (the website shows its default page).
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PublicHomePageDto>> Get(CancellationToken cancellationToken)
        {
            var page = await _homePageService.GetPublishedAsync(cancellationToken);
            return page is null ? NoContent() : Ok(page);
        }
    }
}
