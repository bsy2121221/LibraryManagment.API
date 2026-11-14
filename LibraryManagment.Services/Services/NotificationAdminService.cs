using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.DTOs.Notifications;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Enums;
using LibraryManagement.Models.Exceptions;
using LibraryManagement.Models.Notifications;
using LibraryManagement.Models.Options;
using LibraryManagement.Services.IServices;
using LibraryManagement.Services.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LibraryManagement.Services.Services
{
    public class NotificationAdminService : INotificationAdminService
    {
        private readonly INotificationRepository _repository;
        private readonly NotificationOptions _options;
        private readonly ILogger<NotificationAdminService> _logger;

        public NotificationAdminService(INotificationRepository repository, IOptions<NotificationOptions> options, ILogger<NotificationAdminService> logger)
        {
            _repository = repository;
            _options = options.Value;
            _logger = logger;
        }

        // ---------- Message log ----------

        public async Task<IReadOnlyList<NotificationDto>> GetMessagesAsync(NotificationStatus? status, NotificationChannel? channel, string? eventType,
            int? studentId, string? search, CancellationToken cancellationToken = default)
        {
            var rows = await _repository.GetForAdminAsync(status, channel, eventType, studentId, search?.Trim(), cancellationToken);
            return rows.Select(r => NotificationDto.FromEntity(r, EventName(r))).ToList();
        }

        public async Task<NotificationSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            var counts = (await _repository.GetSummaryAsync(cancellationToken)).ToDictionary(c => c.Status, c => c.Count);
            int Count(NotificationStatus s) => counts.TryGetValue(s, out var n) ? n : 0;

            return new NotificationSummaryDto
            {
                Ready = Count(NotificationStatus.Ready),
                Processing = Count(NotificationStatus.Processing),
                Sent = Count(NotificationStatus.Sent),
                Failed = Count(NotificationStatus.Failed),
                Skipped = Count(NotificationStatus.Skipped),
                Cancelled = Count(NotificationStatus.Cancelled),
                Providers = NotificationSenderRegistration.ConfiguredProviders(_options)
            };
        }

        public Task RetryAsync(long id, CancellationToken cancellationToken = default) => _repository.RetryAsync(id, cancellationToken);

        public Task CancelAsync(long id, CancellationToken cancellationToken = default) => _repository.CancelAsync(id, cancellationToken);

        // ---------- Event messages ----------

        public async Task<IReadOnlyList<NotificationEventDto>> GetEventsAsync(CancellationToken cancellationToken = default)
        {
            var rows = await _repository.GetEventChannelsAsync(cancellationToken);
            return NotificationEvents.All.Select(e => ToEventDto(e, rows)).ToList();
        }

        public async Task<NotificationEventDto> SaveEventAsync(string eventType, SaveNotificationEventRequest request, CancellationToken cancellationToken = default)
        {
            var info = NotificationEvents.Find(eventType) ?? throw new NotFoundException($"Unknown event \"{eventType}\".");

            var duplicate = request.Channels.GroupBy(c => c.Channel).FirstOrDefault(g => g.Count() > 1);
            if (duplicate is not null)
            {
                throw new BadRequestException($"{duplicate.Key} is listed more than once.");
            }

            var existing = (await _repository.GetEventChannelsAsync(cancellationToken))
                .Where(r => r.EventType == info.EventType)
                .ToDictionary(r => r.Channel);

            foreach (var channel in request.Channels)
            {
                var body = channel.Body?.Trim() ?? string.Empty;
                var subject = string.IsNullOrWhiteSpace(channel.Subject) ? null : channel.Subject.Trim();

                if (!channel.IsActive && body.Length == 0)
                {
                    // Switched off and never written: nothing to store. Switched off with text: keep the text for later.
                    if (existing.TryGetValue(channel.Channel, out var current))
                    {
                        await _repository.SaveEventChannelAsync(info.EventType, channel.Channel, false, current.Subject, current.Body, cancellationToken);
                    }
                    continue;
                }

                ValidateMessage(channel.Channel, subject, body, channel.IsActive);
                await _repository.SaveEventChannelAsync(info.EventType, channel.Channel, channel.IsActive, subject, body, cancellationToken);
            }

            _logger.LogInformation("Notification messages for {EventType} updated", info.EventType);
            var rows = await _repository.GetEventChannelsAsync(cancellationToken);
            return ToEventDto(info, rows);
        }

        // ---------- Reminder rules ----------

        public async Task<IReadOnlyList<NotificationRuleDto>> GetRulesAsync(CancellationToken cancellationToken = default)
        {
            var rules = await _repository.GetRulesAsync(cancellationToken);
            return rules.Select(NotificationRuleDto.FromEntity).ToList();
        }

        public async Task<NotificationRuleDto> SaveRuleAsync(int? id, SaveNotificationRuleRequest request, CancellationToken cancellationToken = default)
        {
            var duplicate = request.Channels.GroupBy(c => c.Channel).FirstOrDefault(g => g.Count() > 1);
            if (duplicate is not null)
            {
                throw new BadRequestException($"{duplicate.Key} is listed more than once.");
            }
            foreach (var channel in request.Channels)
            {
                ValidateMessage(channel.Channel, channel.Subject?.Trim(), channel.Body.Trim(), isActive: true);
            }

            var quietStart = ParseTime(request.QuietHoursStart);
            var quietEnd = ParseTime(request.QuietHoursEnd);
            if ((quietStart is null) != (quietEnd is null))
            {
                throw new BadRequestException("Set both quiet hours times, or neither.");
            }
            if (quietStart is not null && quietStart == quietEnd)
            {
                throw new BadRequestException("Quiet hours start and end cannot be the same time.");
            }

            var rule = new NotificationRule
            {
                Id = id ?? 0,
                Name = request.Name.Trim(),
                Trigger = request.Trigger,
                OffsetDays = request.OffsetDays,
                RepeatEveryMinutes = request.RepeatEveryMinutes,
                MaxAttempts = request.RepeatEveryMinutes is null ? 1 : request.MaxAttempts,
                QuietHoursStart = quietStart,
                QuietHoursEnd = quietEnd,
                IsActive = request.IsActive,
                Channels = request.Channels.Select(c => new NotificationRuleChannelRow
                {
                    Channel = c.Channel,
                    Subject = string.IsNullOrWhiteSpace(c.Subject) ? null : c.Subject.Trim(),
                    Body = c.Body.Trim()
                }).ToList()
            };

            var savedId = await _repository.SaveRuleAsync(rule, cancellationToken);
            _logger.LogInformation("Reminder rule {RuleId} '{Name}' saved", savedId, rule.Name);

            var rules = await _repository.GetRulesAsync(cancellationToken);
            return NotificationRuleDto.FromEntity(rules.First(r => r.Id == savedId));
        }

        public async Task DeleteRuleAsync(int id, CancellationToken cancellationToken = default)
        {
            await _repository.DeleteRuleAsync(id, cancellationToken);
            _logger.LogInformation("Reminder rule {RuleId} deleted", id);
        }

        // ---------- helpers ----------

        private static NotificationEventDto ToEventDto(NotificationEventInfo info, IEnumerable<NotificationEventChannelRow> rows)
        {
            var byChannel = rows.Where(r => r.EventType == info.EventType).ToDictionary(r => r.Channel);
            return new NotificationEventDto
            {
                EventType = info.EventType,
                Name = info.Name,
                Description = info.Description,
                Placeholders = [.. NotificationEvents.CommonPlaceholders, .. info.Placeholders],
                Channels = NotificationEvents.Channels.Select(channel => byChannel.TryGetValue(channel, out var row)
                    ? new EventChannelDto { Channel = channel, IsActive = row.IsActive, Subject = row.Subject, Body = row.Body }
                    : new EventChannelDto { Channel = channel, IsActive = false }).ToList()
            };
        }

        private static void ValidateMessage(NotificationChannel channel, string? subject, string body, bool isActive)
        {
            if (isActive && body.Length < 5)
            {
                throw new BadRequestException($"Write a message for {channel}.");
            }
            if (isActive && channel == NotificationChannel.Email && string.IsNullOrWhiteSpace(subject))
            {
                throw new BadRequestException("Email messages need a subject.");
            }
        }

        private static TimeSpan? ParseTime(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : TimeSpan.ParseExact(value, @"hh\:mm", null);

        private static string EventName(NotificationLogEntry row) =>
            row.EventType == NotificationEvents.FeeReminder
                ? $"Fee reminder{(row.RuleName is null ? string.Empty : $": {row.RuleName}")}"
                : NotificationEvents.Find(row.EventType)?.Name ?? row.EventType;
    }
}
