using LibraryManagement.Auth.Extensions;
using LibraryManagement.Models.DTOs.Bookings;
using LibraryManagement.Models.DTOs.Students;
using LibraryManagement.Models.Enums;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Admin
{
    // Admin: student accounts. Seat changes go through /api/admin/bookings (move / release).
    [Route("api/admin/students")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly IBookingService _bookingService;

        public StudentsController(IStudentService studentService, IBookingService bookingService)
        {
            _studentService = studentService;
            _bookingService = bookingService;
        }

        // search matches name, mobile, email or seat; isActive / hasSeat omitted = all
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<StudentListItemDto>>> GetAll(
            [FromQuery] string? search, [FromQuery] bool? isActive, [FromQuery] bool? hasSeat, CancellationToken cancellationToken)
        {
            return Ok(await _studentService.GetAllAsync(search, isActive, hasSeat, cancellationToken));
        }

        // Register a walk-in student (the admin sets the first password)
        [HttpPost]
        public async Task<ActionResult<StudentDetailDto>> Create(CreateStudentRequest request, CancellationToken cancellationToken)
        {
            var student = await _studentService.CreateAsync(User.GetUserId(), request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = student.Profile.Id }, student);
        }

        // Give a seat directly at the desk; the first month is recorded as paid now or as due
        [HttpPost("{id:int}/assign-seat")]
        public async Task<ActionResult<BookingDto>> AssignSeat(int id, AssignSeatRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _bookingService.AssignSeatAsync(User.GetUserId(), id, request, cancellationToken));
        }

        // Profile, current seat, seat history and payments
        [HttpGet("{id:int}")]
        public async Task<ActionResult<StudentDetailDto>> GetById(int id, CancellationToken cancellationToken)
        {
            return Ok(await _studentService.GetDetailAsync(id, cancellationToken));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<StudentDetailDto>> Update(int id, UpdateStudentRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _studentService.UpdateAsync(User.GetUserId(), id, request, cancellationToken));
        }

        // Sets a new password and logs the student out everywhere
        [HttpPost("{id:int}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            await _studentService.ResetPasswordAsync(User.GetUserId(), id, request.NewPassword, cancellationToken);
            return NoContent();
        }
    }
}
