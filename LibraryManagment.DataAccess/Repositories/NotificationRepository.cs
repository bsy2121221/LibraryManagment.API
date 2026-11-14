using Dapper;
using LibraryManagement.DataAccess.Context;
using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Enums;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace LibraryManagement.DataAccess.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IDbContext _dbContext;

        public NotificationRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<QueuedNotification>> ClaimBatchAsync(int batchSize, int lockSeconds, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            var rows = await connection.QueryAsync<QueuedNotification>(new CommandDefinition(
                "dbo.usp_Notification_ClaimBatch", new { BatchSize = batchSize, LockSeconds = lockSeconds },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return rows.AsList();
        }

        public async Task MarkSentAsync(long id, string provider, string? providerMessageId, string? subject, string body, CancellationToken cancellationToken = default)
        {
            var parameters = new { Id = id, Provider = provider, ProviderMessageId = providerMessageId, Subject = subject, Body = body };

            using var connection = _dbContext.CreateConnection();
            await connection.ExecuteAsync(new CommandDefinition(
                "dbo.usp_Notification_MarkSent", parameters,
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task MarkFailedAsync(long id, string? provider, string error, int retryDelaySeconds, bool permanent, bool skip, CancellationToken cancellationToken = default)
        {
            var parameters = new
            {
                Id = id,
                Provider = provider,
                Error = error,
                RetryDelaySeconds = retryDelaySeconds,
                Permanent = permanent,
                Skip = skip
            };

            using var connection = _dbContext.CreateConnection();
            await connection.ExecuteAsync(new CommandDefinition(
                "dbo.usp_Notification_MarkFailed", parameters,
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task<int> EnqueueFeeRemindersAsync(int graceDays, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                "dbo.usp_Notification_EnqueueFeeReminders", new { GraceDays = graceDays },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task<IReadOnlyList<NotificationLogEntry>> GetForAdminAsync(NotificationStatus? status, NotificationChannel? channel,
            string? eventType, int? userId, string? search, CancellationToken cancellationToken = default)
        {
            var parameters = new
            {
                Status = (byte?)status,
                Channel = (byte?)channel,
                EventType = eventType,
                UserId = userId,
                Search = search
            };

            using var connection = _dbContext.CreateConnection();
            var rows = await connection.QueryAsync<NotificationLogEntry>(new CommandDefinition(
                "dbo.usp_Notification_GetForAdmin", parameters,
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return rows.AsList();
        }

        public async Task<IReadOnlyList<NotificationStatusCount>> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            var rows = await connection.QueryAsync<NotificationStatusCount>(new CommandDefinition(
                "dbo.usp_Notification_GetSummary",
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return rows.AsList();
        }

        public Task RetryAsync(long id, CancellationToken cancellationToken = default)
            => ExecuteAsync("dbo.usp_Notification_Retry", new { Id = id }, cancellationToken);

        public Task CancelAsync(long id, CancellationToken cancellationToken = default)
            => ExecuteAsync("dbo.usp_Notification_Cancel", new { Id = id }, cancellationToken);

        public async Task<IReadOnlyList<NotificationEventChannelRow>> GetEventChannelsAsync(CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            var rows = await connection.QueryAsync<NotificationEventChannelRow>(new CommandDefinition(
                "dbo.usp_NotificationEvent_GetAll",
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return rows.AsList();
        }

        public Task SaveEventChannelAsync(string eventType, NotificationChannel channel, bool isActive, string? subject, string body, CancellationToken cancellationToken = default)
            => ExecuteAsync("dbo.usp_NotificationEvent_SaveChannel",
                new { EventType = eventType, Channel = (byte)channel, IsActive = isActive, Subject = subject, Body = body },
                cancellationToken);

        public async Task<IReadOnlyList<NotificationRule>> GetRulesAsync(CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            using var results = await connection.QueryMultipleAsync(new CommandDefinition(
                "dbo.usp_NotificationRule_GetAll",
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));

            var rules = (await results.ReadAsync<NotificationRule>()).AsList();
            var channels = (await results.ReadAsync<NotificationRuleChannelRow>()).ToLookup(c => c.RuleId);
            foreach (var rule in rules)
            {
                rule.Channels = channels[rule.Id].ToList();
            }
            return rules;
        }

        public async Task<int> SaveRuleAsync(NotificationRule rule, CancellationToken cancellationToken = default)
        {
            // Property names must match the OPENJSON paths in usp_NotificationRule_Save
            var channelsJson = JsonSerializer.Serialize(rule.Channels.Select(c => new { Channel = (byte)c.Channel, c.Subject, c.Body }));
            var parameters = new
            {
                Id = rule.Id == 0 ? (int?)null : rule.Id,
                rule.Name,
                Trigger = (byte)rule.Trigger,
                rule.OffsetDays,
                rule.RepeatEveryMinutes,
                rule.MaxAttempts,
                rule.QuietHoursStart,
                rule.QuietHoursEnd,
                rule.IsActive,
                ChannelsJson = channelsJson
            };

            using var connection = _dbContext.CreateConnection();
            try
            {
                return await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                    "dbo.usp_NotificationRule_Save", parameters,
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.ToAppException() is { } appException)
            {
                throw appException;
            }
        }

        public Task DeleteRuleAsync(int id, CancellationToken cancellationToken = default)
            => ExecuteAsync("dbo.usp_NotificationRule_Delete", new { Id = id }, cancellationToken);

        private async Task ExecuteAsync(string procedure, object parameters, CancellationToken cancellationToken)
        {
            using var connection = _dbContext.CreateConnection();
            try
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    procedure, parameters, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.ToAppException() is { } appException)
            {
                throw appException;
            }
        }
    }
}
