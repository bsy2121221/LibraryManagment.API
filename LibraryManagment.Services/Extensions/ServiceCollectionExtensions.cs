using LibraryManagement.Models.Options;
using LibraryManagement.Services.Background;
using LibraryManagement.Services.IServices;
using LibraryManagement.Services.Media;
using LibraryManagement.Services.Notifications;
using LibraryManagement.Services.Payments;
using LibraryManagement.Services.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.Services.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Options
            services.Configure<AdminSeedOptions>(configuration.GetSection(AdminSeedOptions.SectionName));
            services.Configure<PaymentOptions>(configuration.GetSection(PaymentOptions.SectionName));
            services.Configure<NotificationOptions>(configuration.GetSection(NotificationOptions.SectionName));
            services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));

            services.AddSingleton(TimeProvider.System);

            // Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAdminSeeder, AdminSeeder>();
            services.AddScoped<ISectionService, SectionService>();
            services.AddScoped<ISeatMapService, SeatMapService>();
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IFeeService, FeeService>();
            services.AddScoped<INotificationAdminService, NotificationAdminService>();
            services.AddScoped<IHomePageService, HomePageService>();
            services.AddScoped<IMediaService, MediaService>();

            services.AddPaymentGateway(configuration);
            services.AddNotificationSenders(configuration);
            services.AddMediaStorage(configuration);

            // Background jobs
            services.AddHostedService<HoldExpiryService>();
            services.AddHostedService<BillingJobService>();
            services.AddHostedService<NotificationDispatchService>();

            return services;
        }

        // Picks where uploaded images are stored from "Storage:Provider"
        private static void AddMediaStorage(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetSection(StorageOptions.SectionName).Get<StorageOptions>() ?? new StorageOptions();

            switch (options.Provider.Trim().ToLowerInvariant())
            {
                case "local":
                    services.AddSingleton<IMediaStorage, LocalMediaStorage>();
                    break;

                case "azureblob":
                    throw new InvalidOperationException(
                        "Storage:Provider 'AzureBlob' is not implemented yet. Use 'Local' (images are kept on the API server).");

                default:
                    throw new InvalidOperationException($"Unknown Storage:Provider '{options.Provider}'. Use 'Local'.");
            }
        }

        // Picks the payment provider from "Payment:Provider". Switching to Razorpay needs only config changes.
        private static void AddPaymentGateway(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetSection(PaymentOptions.SectionName).Get<PaymentOptions>() ?? new PaymentOptions();

            switch (options.Provider.Trim().ToLowerInvariant())
            {
                case "razorpay":
                    if (string.IsNullOrWhiteSpace(options.Razorpay.KeyId) || string.IsNullOrWhiteSpace(options.Razorpay.KeySecret))
                    {
                        throw new InvalidOperationException(
                            "Payment:Provider is Razorpay but Payment:Razorpay:KeyId / KeySecret are not configured.");
                    }
                    services.AddHttpClient<IPaymentGateway, RazorpayPaymentGateway>(client =>
                    {
                        client.BaseAddress = new Uri(RazorpayPaymentGateway.BaseUrl);
                        client.Timeout = TimeSpan.FromSeconds(30);
                    });
                    break;

                case "mock":
                    services.AddSingleton<IPaymentGateway, MockPaymentGateway>();
                    break;

                default:
                    throw new InvalidOperationException($"Unknown Payment:Provider '{options.Provider}'. Use 'Mock' or 'Razorpay'.");
            }
        }
    }
}
