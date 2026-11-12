using LibraryManagement.Models.DTOs.Sections;
using LibraryManagement.Models.Enums;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Admin
{
    // Admin: design the library's sections (Boys / Girls) and the seat layout of each one
    [Route("api/admin/sections")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class SectionsController : ControllerBase
    {
        private readonly ISectionService _sectionService;

        public SectionsController(ISectionService sectionService)
        {
            _sectionService = sectionService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<SectionDto>>> GetAll(CancellationToken cancellationToken)
        {
            return Ok(await _sectionService.GetAllAsync(cancellationToken));
        }

        // Section details plus every seat and the student on it, for the layout designer
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SectionLayoutDto>> GetById(int id, CancellationToken cancellationToken)
        {
            return Ok(await _sectionService.GetLayoutAsync(id, cancellationToken));
        }

        [HttpPost]
        public async Task<ActionResult<SectionDto>> Create(SectionRequest request, CancellationToken cancellationToken)
        {
            var section = await _sectionService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = section.Id }, section);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<SectionDto>> Update(int id, SectionRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _sectionService.UpdateAsync(id, request, cancellationToken));
        }

        // Only for sections that were never booked; otherwise set IsActive = false
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _sectionService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }

        // Saves the whole layout from the drag-and-drop designer
        [HttpPut("{id:int}/seats")]
        public async Task<ActionResult<SectionLayoutDto>> SaveLayout(int id, SaveSeatLayoutRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _sectionService.SaveLayoutAsync(id, request, cancellationToken));
        }

        // Adds a grid of seats to the section (existing seats are kept)
        [HttpPost("{id:int}/seats/generate")]
        public async Task<ActionResult<SectionLayoutDto>> GenerateSeats(int id, GenerateSeatsRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _sectionService.GenerateSeatsAsync(id, request, cancellationToken));
        }
    }
}
