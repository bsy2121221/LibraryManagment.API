using LibraryManagement.Auth.Extensions;
using LibraryManagement.Models.DTOs.Fees;
using LibraryManagement.Models.Enums;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Admin
{
    // Admin: monthly fees of all students
    [Route("api/admin/fees")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class AdminFeesController : ControllerBase
    {
        private readonly IFeeService _feeService;

        public AdminFeesController(IFeeService feeService)
        {
            _feeService = feeService;
        }

        // status omitted = unpaid (Due + Overdue). search matches name, mobile or seat.
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<FeeInvoiceDto>>> GetAll(
            [FromQuery] InvoiceStatus? status, [FromQuery] string? search, CancellationToken cancellationToken)
        {
            return Ok(await _feeService.GetAllAsync(status, search, cancellationToken));
        }

        // Add a fee for a student with a seat (paid at the desk now, or due)
        [HttpPost]
        public async Task<ActionResult<FeeInvoiceDto>> Create(CreateFeeRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _feeService.CreateAsync(User.GetUserId(), request, cancellationToken));
        }

        // Record a fee paid at the desk (cash / UPI to the library). The student gets the "fee paid" messages.
        [HttpPost("{id:int}/mark-paid")]
        public async Task<ActionResult<FeeInvoiceDto>> MarkPaid(int id, MarkFeePaidRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _feeService.MarkPaidAsync(User.GetUserId(), id, request.Note, cancellationToken));
        }
    }
}
