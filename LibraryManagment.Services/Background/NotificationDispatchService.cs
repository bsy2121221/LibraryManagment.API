using LibraryManagement.Models.Options;
using LibraryManagement.Services.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LibraryManagement.Services.Background
{
    // Every minute (Notifications:DispatchIntervalSeconds) sends the messages waiting in dbo.Notifications.
    // Requests only add rows to that table, so booking, payment and admin actions never wait for SMS/email.
    public class NotificationDispatchService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly NotificationOptions _options;
        private readonly ILogger<NotificationDispatchService> _logger;

        public NotificationDispatchService(IServiceScopeFactory scopeFactory, IOptions<NotificationOptions> options, ILogger<NotificationDispatchService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = TimeSpan.FromSeconds(Math.Clamp(_options.DispatchIntervalSeconds, 10, 3600));
            _logger.LogInformation("Notification dispatcher started, running every {Interval}", interval);

            using var timer = new PeriodicTimer(interval);
            do
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dispatcher = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();
                    var processed = await dispatcher.DispatchAsync(stoppingToken);
                    if (processed > 0)
                    {
                        _logger.LogInformation("Notification dispatcher processed {Count} message(s)", processed);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Notification dispatch run failed");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}
