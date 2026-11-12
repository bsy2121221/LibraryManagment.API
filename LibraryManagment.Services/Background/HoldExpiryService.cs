using LibraryManagement.DataAccess.IRepositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LibraryManagement.Services.Background
{
    // Every 30 seconds, marks seat holds whose payment time ran out as Expired.
    // (Reads already treat a timed-out hold as free; this keeps the stored status correct.)
    public class HoldExpiryService : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<HoldExpiryService> _logger;

        public HoldExpiryService(IServiceScopeFactory scopeFactory, ILogger<HoldExpiryService> logger)
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
                    var repository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                    var expired = await repository.ExpireHoldsAsync(stoppingToken);
                    if (expired > 0)
                    {
                        _logger.LogInformation("Expired {Count} unpaid seat hold(s)", expired);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Keep running: a database hiccup must not stop the job for good
                    _logger.LogError(ex, "Seat hold expiry run failed");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}
