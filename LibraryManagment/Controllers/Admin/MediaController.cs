using LibraryManagement.Auth.Extensions;
using LibraryManagement.Models.DTOs.HomePage;
using LibraryManagement.Models.Enums;
using LibraryManagement.Models.Exceptions;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Admin
{
    // Admin: image library used by the home page builder and section backgrounds
    [Route("api/admin/media")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class MediaController : ControllerBase
    {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<MediaAssetDto>>> GetAll(CancellationToken cancellationToken)
        {
            return Ok(await _mediaService.GetAllAsync(cancellationToken));
        }

        // multipart/form-data with one "file" field. The size limit (Storage:MaxUploadMb) is checked by the service.
        [HttpPost]
        [RequestSizeLimit(50 * 1024 * 1024)]
        public async Task<ActionResult<MediaAssetDto>> Upload(IFormFile? file, CancellationToken cancellationToken)
        {
            if (file is null)
            {
                throw new BadRequestException("Choose an image to upload.");
            }

            await using var stream = file.OpenReadStream();
            return Ok(await _mediaService.UploadAsync(User.GetUserId(), stream, file.FileName, file.Length, cancellationToken));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _mediaService.DeleteAsync(User.GetUserId(), id, cancellationToken);
            return NoContent();
        }
    }
}
