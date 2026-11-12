using Dapper;
using LibraryManagement.DataAccess.Context;
using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.Entities;
using System.Data;

namespace LibraryManagement.DataAccess.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IDbContext _dbContext;

        public PaymentRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            var parameters = new
            {
                payment.UserId,
                payment.BookingId,
                payment.FeeInvoiceId,
                payment.Gateway,
                payment.GatewayOrderId,
                payment.Amount,
                payment.Currency
            };

            using var connection = _dbContext.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                "dbo.usp_Payment_Create", parameters,
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task<IReadOnlyList<Payment>> GetByUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            var rows = await connection.QueryAsync<Payment>(new CommandDefinition(
                "dbo.usp_Payment_GetByUser", new { UserId = userId },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return rows.AsList();
        }

        public async Task<Payment?> GetByOrderIdAsync(string gatewayOrderId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<Payment>(new CommandDefinition(
                "dbo.usp_Payment_GetByOrderId", new { GatewayOrderId = gatewayOrderId },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task MarkFailedAsync(string gatewayOrderId, string? gatewayPaymentId, string? reason, string? rawResponse, CancellationToken cancellationToken = default)
        {
            var parameters = new
            {
                GatewayOrderId = gatewayOrderId,
                GatewayPaymentId = gatewayPaymentId,
                FailureReason = reason,
                RawResponse = rawResponse
            };

            using var connection = _dbContext.CreateConnection();
            await connection.ExecuteAsync(new CommandDefinition(
                "dbo.usp_Payment_MarkFailed", parameters,
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }
    }
}
