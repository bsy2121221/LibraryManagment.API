using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Services.IServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LibraryManagement.Services.Background
{
    // Every minute: creates next month's fees, marks unpaid ones overdue, releases seats if auto-release is on,
    // then queues the fee reminders the admin's rules ask for. The messages are sent by NotificationDispatchService.
    // Each step only changes what is due, so running it often is cheap and safe.
    public class BillingJobService : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BillingJobService> _logger;

        public BillingJobService(IServiceScopeFactory scopeFactory, ILogger<BillingJobService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Interval);
            do
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var settings = await scope.ServiceProvider.GetRequiredService<ISettingsService>().GetBillingSettingsAsync(stoppingToken);
                    var fees = scope.ServiceProvider.GetRequiredService<IFeeRepository>();
                    var notifications = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

                    var result = await fees.RunBillingAsync(settings.MonthlyFee, settings.InvoiceDaysBefore, settings.GraceDays,
                        settings.AutoReleaseAfterOverdueDays, stoppingToken);
                    if (result.InvoicesCreated + result.InvoicesMarkedOverdue + result.SeatsReleased > 0)
                    {
                        _logger.LogInformation("Billing: {Created} fee(s) created, {Overdue} marked overdue, {Released} seat(s) released",
                            result.InvoicesCreated, result.InvoicesMarkedOverdue, result.SeatsReleased);
                    }

                    var reminders = await notifications.EnqueueFeeRemindersAsync(settings.GraceDays, stoppingToken);
                    if (reminders > 0)
                    {
                        _logger.LogInformation("Billing: queued {Count} fee reminder(s)", reminders);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Billing run failed");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}
