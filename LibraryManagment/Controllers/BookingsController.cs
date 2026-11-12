using LibraryManagement.Auth.Extensions;
using LibraryManagement.Models.DTOs.Bookings;
using LibraryManagement.Models.DTOs.Payments;
using LibraryManagement.Models.Enums;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers
{
    // Student booking flow: hold a seat -> create payment order -> pay -> POST /api/payments/verify
    [Route("api/bookings")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.Student))]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IPaymentService _paymentService;

        public BookingsController(IBookingService bookingService, IPaymentService paymentService)
        {
            _bookingService = bookingService;
            _paymentService = paymentService;
        }

        // Holds the seat for SeatHoldMinutes (7 by default) so the student can pay
        [HttpPost("hold")]
        public async Task<ActionResult<BookingDto>> Hold(HoldSeatRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _bookingService.HoldSeatAsync(User.GetUserId(), request.SeatId, cancellationToken));
        }

        // The student's current seat or hold; 204 when they have none
        [HttpGet("me")]
        public async Task<ActionResult<BookingDto>> Me(CancellationToken cancellationToken)
        {
            var booking = await _bookingService.GetMyBookingAsync(User.GetUserId(), cancellationToken);
            return booking is null ? NoContent() : Ok(booking);
        }

        // Gives up a hold before paying
        [HttpDelete("{id:int}/hold")]
        public async Task<IActionResult> CancelHold(int id, CancellationToken cancellationToken)
        {
            await _bookingService.CancelHoldAsync(User.GetUserId(), id, cancellationToken);
            return NoContent();
        }

        // Creates a payment order for the first month's fee; the UI opens the checkout with it
        [HttpPost("{id:int}/payment-order")]
        public async Task<ActionResult<PaymentOrderDto>> CreatePaymentOrder(int id, CancellationToken cancellationToken)
        {
            return Ok(await _paymentService.CreateBookingOrderAsync(User.GetUserId(), id, cancellationToken));
        }
    }
}
