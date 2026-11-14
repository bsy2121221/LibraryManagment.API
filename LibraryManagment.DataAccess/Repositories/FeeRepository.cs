using Dapper;
using LibraryManagement.DataAccess.Context;
using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Enums;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LibraryManagement.DataAccess.Repositories
{
    public class FeeRepository : IFeeRepository
    {
        private readonly IDbContext _dbContext;

        public FeeRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<BillingRunResult> RunBillingAsync(decimal monthlyFee, int invoiceDaysBefore, int graceDays, int autoReleaseAfterOverdueDays,
            CancellationToken cancellationToken = default)
        {
            var parameters = new
            {
                MonthlyFee = monthlyFee,
                InvoiceDaysBefore = invoiceDaysBefore,
                GraceDays = graceDays,
                AutoReleaseAfterOverdueDays = autoReleaseAfterOverdueDays
            };

            using var connection = _dbContext.CreateConnection();
            return await connection.QuerySingleAsync<BillingRunResult>(new CommandDefinition(
                "dbo.usp_Billing_Run", parameters,
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task<FeeInvoiceDetails?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<FeeInvoiceDetails>(new CommandDefinition(
                "dbo.usp_FeeInvoice_GetById", new { Id = id },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task<IReadOnlyList<FeeInvoiceDetails>> GetByUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            var rows = await connection.QueryAsync<FeeInvoiceDetails>(new CommandDefinition(
                "dbo.usp_FeeInvoice_GetByUser", new { UserId = userId },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return rows.AsList();
        }

        public async Task<IReadOnlyList<FeeInvoiceDetails>> GetForAdminAsync(InvoiceStatus? status, string? search, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            var rows = await connection.QueryAsync<FeeInvoiceDetails>(new CommandDefinition(
                "dbo.usp_FeeInvoice_GetForAdmin", new { Status = (byte?)status, Search = search },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return rows.AsList();
        }

        public async Task<int> CreateByAdminAsync(int bookingId, DateOnly periodStart, DateOnly periodEnd, decimal amount, DateOnly dueDate,
            bool paidAtDesk, string? note, int adminUserId, string currency, CancellationToken cancellationToken = default)
        {
            var parameters = new
            {
                BookingId = bookingId,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                Amount = amount,
                DueDate = dueDate,
                PaidAtDesk = paidAtDesk,
                Note = note,
                AdminUserId = adminUserId,
                Currency = currency
            };

            using var connection = _dbContext.CreateConnection();
            try
            {
                return await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                    "dbo.usp_FeeInvoice_CreateByAdmin", parameters,
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.IsUniqueViolation())
            {
                throw new LibraryManagement.Models.Exceptions.ConflictException(
                    "This student already has a fee starting on that date. Change the period start, or edit the existing fee.");
            }
            catch (SqlException ex) when (ex.ToAppException() is { } appException)
            {
                throw appException;
            }
        }

        public async Task MarkPaidAsync(int invoiceId, int adminUserId, string? note, string currency, CancellationToken cancellationToken = default)
        {
            var parameters = new { InvoiceId = invoiceId, AdminUserId = adminUserId, Note = note, Currency = currency };

            using var connection = _dbContext.CreateConnection();
            try
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    "dbo.usp_FeeInvoice_MarkPaid", parameters,
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.ToAppException() is { } appException)
            {
                throw appException;
            }
        }
    }
}
