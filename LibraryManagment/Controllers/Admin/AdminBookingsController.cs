using LibraryManagement.Auth.Extensions;
using LibraryManagement.Models.DTOs.Bookings;
using LibraryManagement.Models.Enums;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Admin
{
    // Admin: who is sitting where; release a student from a seat or move them to another seat
    [Route("api/admin/bookings")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class AdminBookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public AdminBookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // status omitted = current bookings (Active + Held). search matches name, mobile or seat label.
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<BookingDto>>> GetAll(
            [FromQuery] BookingStatus? status, [FromQuery] string? search, CancellationToken cancellationToken)
        {
            return Ok(await _bookingService.GetAllAsync(status, search, cancellationToken));
        }

        [HttpPost("{id:int}/release")]
        public async Task<IActionResult> Release(int id, ReleaseBookingRequest request, CancellationToken cancellationToken)
        {
            await _bookingService.ReleaseAsync(User.GetUserId(), id, request.Reason, cancellationToken);
            return NoContent();
        }

        [HttpPost("{id:int}/move")]
        public async Task<ActionResult<BookingDto>> Move(int id, MoveSeatRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _bookingService.MoveSeatAsync(User.GetUserId(), id, request, cancellationToken));
        }
    }
}
