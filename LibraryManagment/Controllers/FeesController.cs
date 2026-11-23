using LibraryManagement.Auth.Extensions;
using LibraryManagement.Models.DTOs.Fees;
using LibraryManagement.Models.DTOs.Payments;
using LibraryManagement.Models.Enums;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers
{
    // Student: monthly fees. Paying works like booking: create an order, pay, then POST /api/payments/verify.
    [Route("api/fees")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.Student))]
    public class FeesController : ControllerBase
    {
        private readonly IFeeService _feeService;
        private readonly IPaymentService _paymentService;

        public FeesController(IFeeService feeService, IPaymentService paymentService)
        {
            _feeService = feeService;
            _paymentService = paymentService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<IReadOnlyList<FeeInvoiceDto>>> Mine(CancellationToken cancellationToken)
        {
            return Ok(await _feeService.GetMyFeesAsync(User.GetUserId(), cancellationToken));
        }

        // Important popup after login: 204 when there is nothing to show
        [HttpGet("me/overdue-notice")]
        public async Task<ActionResult<OverdueNoticeDto>> MyOverdueNotice(CancellationToken cancellationToken)
        {
            var notice = await _feeService.GetMyOverdueNoticeAsync(User.GetUserId(), cancellationToken);
            return notice is null ? NoContent() : Ok(notice);
        }

        [HttpPost("{id:int}/payment-order")]
        public async Task<ActionResult<PaymentOrderDto>> CreatePaymentOrder(int id, CancellationToken cancellationToken)
        {
            return Ok(await _paymentService.CreateFeeOrderAsync(User.GetUserId(), id, cancellationToken));
        }
    }
}
