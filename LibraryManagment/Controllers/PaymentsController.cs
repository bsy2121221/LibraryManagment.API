using LibraryManagement.Auth.Extensions;
using LibraryManagement.Models.DTOs.Payments;
using LibraryManagement.Models.Enums;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // Checkout succeeded: verify the signature and confirm the booking
        [HttpPost("verify")]
        [Authorize(Roles = nameof(UserRole.Student))]
        public async Task<ActionResult<PaymentResultDto>> Verify(VerifyPaymentRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _paymentService.VerifyAsync(User.GetUserId(), request, cancellationToken));
        }

        // Checkout failed or was closed. The seat stays held until the hold time runs out, so the student can retry.
        [HttpPost("failed")]
        [Authorize(Roles = nameof(UserRole.Student))]
        public async Task<IActionResult> Failed(PaymentFailedRequest request, CancellationToken cancellationToken)
        {
            await _paymentService.MarkFailedAsync(User.GetUserId(), request, cancellationToken);
            return NoContent();
        }

        // Razorpay dashboard -> Webhooks: point it at https://<api>/api/payments/razorpay/webhook
        // with events payment.captured, payment.failed, order.paid and the WebhookSecret from appsettings.
        [HttpPost("razorpay/webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> RazorpayWebhook(CancellationToken cancellationToken)
        {
            // The signature is computed over the exact raw body, so read it as text instead of model binding
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync(cancellationToken);
            var signature = Request.Headers["X-Razorpay-Signature"].ToString();

            await _paymentService.HandleWebhookAsync(payload, signature, cancellationToken);
            return Ok();
        }
    }
}
