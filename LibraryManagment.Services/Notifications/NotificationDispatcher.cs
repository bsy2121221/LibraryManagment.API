using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Enums;
using LibraryManagement.Models.Options;
using LibraryManagement.Services.IServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LibraryManagement.Services.Notifications
{
    public interface INotificationDispatcher
    {
        // Sends everything that is Ready now. Returns how many messages were processed.
        Task<int> DispatchAsync(CancellationToken cancellationToken = default);
    }

    // Claims Ready rows from dbo.Notifications in batches, fills in the template, sends with the configured
    // provider for the channel and records the result. Failures are retried with a growing delay
    // (1, 4, 9... minutes) until MaxAttempts, then the message is marked Failed.
    public class NotificationDispatcher : INotificationDispatcher
    {
        private const int LockSeconds = 300;
        private const int MaxBatchesPerRun = 20;
        private const int Parallelism = 4;

        private readonly INotificationRepository _repository;
        private readonly ISettingsService _settingsService;
        private readonly Dictionary<NotificationChannel, INotificationSender> _senders;
        private readonly NotificationOptions _options;
        private readonly ILogger<NotificationDispatcher> _logger;

        public NotificationDispatcher(
            INotificationRepository repository,
            ISettingsService settingsService,
            IEnumerable<INotificationSender> senders,
            IOptions<NotificationOptions> options,
            ILogger<NotificationDispatcher> logger)
        {
            _repository = repository;
            _settingsService = settingsService;
            _senders = senders.GroupBy(s => s.Channel).ToDictionary(g => g.Key, g => g.Last());
            _options = options.Value;
            _logger = logger;
        }

        public async Task<int> DispatchAsync(CancellationToken cancellationToken = default)
        {
            var processed = 0;
            string? libraryName = null;

            for (var batch = 0; batch < MaxBatchesPerRun; batch++)
            {
                var items = await _repository.ClaimBatchAsync(Math.Clamp(_options.BatchSize, 1, 500), LockSeconds, cancellationToken);
                if (items.Count == 0)
                {
                    break;
                }

                libraryName ??= await _settingsService.GetLibraryNameAsync(cancellationToken);

                await Parallel.ForEachAsync(items,
                    new ParallelOptions { MaxDegreeOfParallelism = Parallelism, CancellationToken = cancellationToken },
                    async (item, ct) => await SendOneAsync(item, libraryName, ct));

                processed += items.Count;
                if (items.Count < _options.BatchSize)
                {
                    break;
                }
            }

            return processed;
        }

        private async Task SendOneAsync(QueuedNotification item, string libraryName, CancellationToken cancellationToken)
        {
            if (!_senders.TryGetValue(item.Channel, out var sender))
            {
                await _repository.MarkFailedAsync(item.Id, null, $"No sender is configured for {item.Channel}.", 0,
                    permanent: false, skip: true, CancellationToken.None);
                return;
            }

            string? subject;
            string body;
            try
            {
                var values = TemplateRenderer.ParseData(item.DataJson, libraryName);
                subject = item.Subject is null ? null : TemplateRenderer.Render(item.Subject, values);
                body = TemplateRenderer.Render(item.Body, values, htmlEncodeValues: item.Channel == NotificationChannel.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not build notification {NotificationId}", item.Id);
                await _repository.MarkFailedAsync(item.Id, sender.ProviderName, $"Could not build the message: {ex.Message}", 0,
                    permanent: true, skip: false, CancellationToken.None);
                return;
            }

            try
            {
                var providerMessageId = await sender.SendAsync(
                    new OutgoingMessage(item.Id, item.Channel, item.Recipient, subject, body), cancellationToken);

                // CancellationToken.None: once sent, always record it, even if the app is shutting down
                await _repository.MarkSentAsync(item.Id, sender.ProviderName, providerMessageId, subject, body, CancellationToken.None);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Shutting down: the row stays Processing and is picked up again after its lock ends
                throw;
            }
            catch (Exception ex)
            {
                var permanent = ex is NotificationSendException { Permanent: true };
                var retryDelaySeconds = item.Attempts * item.Attempts * 60;

                _logger.LogWarning(ex, "Sending notification {NotificationId} ({Channel} via {Provider}) failed, attempt {Attempt}/{MaxAttempts}",
                    item.Id, item.Channel, sender.ProviderName, item.Attempts, item.MaxAttempts);

                await _repository.MarkFailedAsync(item.Id, sender.ProviderName, ex.Message, retryDelaySeconds,
                    permanent, skip: false, CancellationToken.None);
            }
        }
    }
}
