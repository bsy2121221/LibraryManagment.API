using LibraryManagement.Models.DTOs.Notifications;
using LibraryManagement.Models.Enums;
using LibraryManagement.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers.Admin
{
    // Admin: SMS / WhatsApp / email / call messages to students.
    // Messages are queued in dbo.Notifications by the action that causes them and sent by a background job every minute.
    [Route("api/admin/notifications")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationAdminService _service;

        public NotificationsController(INotificationAdminService service)
        {
            _service = service;
        }

        // ---------- Message log ----------

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetMessages(
            [FromQuery] NotificationStatus? status, [FromQuery] NotificationChannel? channel, [FromQuery] string? eventType,
            [FromQuery] int? studentId, [FromQuery] string? search, CancellationToken cancellationToken)
        {
            return Ok(await _service.GetMessagesAsync(status, channel, eventType, studentId, search, cancellationToken));
        }

        // Counts per status and the provider each channel uses
        [HttpGet("summary")]
        public async Task<ActionResult<NotificationSummaryDto>> GetSummary(CancellationToken cancellationToken)
        {
            return Ok(await _service.GetSummaryAsync(cancellationToken));
        }

        // Send a failed / skipped / cancelled message again on the next run
        [HttpPost("{id:long}/retry")]
        public async Task<IActionResult> Retry(long id, CancellationToken cancellationToken)
        {
            await _service.RetryAsync(id, cancellationToken);
            return NoContent();
        }

        // Stop a message that is still waiting
        [HttpPost("{id:long}/cancel")]
        public async Task<IActionResult> Cancel(long id, CancellationToken cancellationToken)
        {
            await _service.CancelAsync(id, cancellationToken);
            return NoContent();
        }

        // ---------- Messages per event (seat booked, seat moved, details changed...) ----------

        [HttpGet("events")]
        public async Task<ActionResult<IReadOnlyList<NotificationEventDto>>> GetEvents(CancellationToken cancellationToken)
        {
            return Ok(await _service.GetEventsAsync(cancellationToken));
        }

        [HttpPut("events/{eventType}")]
        public async Task<ActionResult<NotificationEventDto>> SaveEvent(string eventType, SaveNotificationEventRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _service.SaveEventAsync(eventType, request, cancellationToken));
        }

        // ---------- Fee reminder rules ----------

        [HttpGet("rules")]
        public async Task<ActionResult<IReadOnlyList<NotificationRuleDto>>> GetRules(CancellationToken cancellationToken)
        {
            return Ok(await _service.GetRulesAsync(cancellationToken));
        }

        [HttpPost("rules")]
        public async Task<ActionResult<NotificationRuleDto>> CreateRule(SaveNotificationRuleRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _service.SaveRuleAsync(null, request, cancellationToken));
        }

        [HttpPut("rules/{id:int}")]
        public async Task<ActionResult<NotificationRuleDto>> UpdateRule(int id, SaveNotificationRuleRequest request, CancellationToken cancellationToken)
        {
            return Ok(await _service.SaveRuleAsync(id, request, cancellationToken));
        }

        [HttpDelete("rules/{id:int}")]
        public async Task<IActionResult> DeleteRule(int id, CancellationToken cancellationToken)
        {
            await _service.DeleteRuleAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
