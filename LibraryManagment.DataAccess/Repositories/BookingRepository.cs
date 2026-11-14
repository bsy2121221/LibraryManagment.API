using Dapper;
using LibraryManagement.DataAccess.Context;
using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Enums;
using LibraryManagement.Models.Exceptions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LibraryManagement.DataAccess.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        // The filtered unique indexes on dbo.Bookings stop two live bookings per seat or per student
        private const string SeatTakenMessage = "Sorry, this seat was just taken. Please choose another seat.";

        private readonly IDbContext _dbContext;

        public BookingRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<BookingDetails?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<BookingDetails>(new CommandDefinition(
                "dbo.usp_Booking_GetById", new { Id = id },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task<BookingDetails?> GetLiveByUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<BookingDetails>(new CommandDefinition(
                "dbo.usp_Booking_GetLiveByUser", new { UserId = userId },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task<BookingDetails> HoldAsync(int userId, int seatId, int holdMinutes, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            try
            {
                return await connection.QuerySingleAsync<BookingDetails>(new CommandDefinition(
                    "dbo.usp_Booking_Hold", new { UserId = userId, SeatId = seatId, HoldMinutes = holdMinutes },
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.IsUniqueViolation())
            {
                throw new ConflictException(SeatTakenMessage);
            }
            catch (SqlException ex) when (ex.ToAppException() is { } appException)
            {
                throw appException;
            }
        }

        public async Task CancelHoldAsync(int bookingId, int userId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            try
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    "dbo.usp_Booking_CancelHold", new { BookingId = bookingId, UserId = userId },
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.ToAppException() is { } appException)
            {
                throw appException;
            }
        }

        public async Task<int> ExpireHoldsAsync(CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                "dbo.usp_Booking_ExpireHolds",
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task<(PaymentOutcome Outcome, int BookingId)> ConfirmPaymentAsync(
            string gatewayOrderId, string gatewayPaymentId, string? rawResponse, CancellationToken cancellationToken = default)
        {
            var parameters = new { GatewayOrderId = gatewayOrderId, GatewayPaymentId = gatewayPaymentId, RawResponse = rawResponse };

            using var connection = _dbContext.CreateConnection();
            try
            {
                var row = await connection.QuerySingleAsync<(byte Outcome, int BookingId)>(new CommandDefinition(
                    "dbo.usp_Booking_ConfirmPayment", parameters,
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
                return ((PaymentOutcome)row.Outcome, row.BookingId);
            }
            catch (SqlException ex) when (ex.IsUniqueViolation())
            {
                throw new ConflictException(SeatTakenMessage);
            }
            catch (SqlException ex) when (ex.ToAppException() is { } appException)
            {
                throw appException;
            }
        }

        public async Task<IReadOnlyList<BookingDetails>> GetAllForAdminAsync(BookingStatus? status, string? search, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            var rows = await connection.QueryAsync<BookingDetails>(new CommandDefinition(
                "dbo.usp_Booking_GetAllForAdmin", new { Status = (byte?)status, Search = search },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return rows.AsList();
        }

        public async Task<int> AssignByAdminAsync(int userId, int seatId, DateOnly startDate, decimal amount, bool paidAtDesk, string? note,
            int adminUserId, string currency, CancellationToken cancellationToken = default)
        {
            var parameters = new
            {
                UserId = userId,
                SeatId = seatId,
                StartDate = startDate,
                Amount = amount,
                PaidAtDesk = paidAtDesk,
                Note = note,
                AdminUserId = adminUserId,
                Currency = currency
            };

            using var connection = _dbContext.CreateConnection();
            try
            {
                return await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                    "dbo.usp_Booking_AssignByAdmin", parameters,
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.IsUniqueViolation())
            {
                throw new ConflictException(SeatTakenMessage);
            }
            catch (SqlException ex) when (ex.ToAppException() is { } appException)
            {
                throw appException;
            }
        }

        public async Task ReleaseAsync(int bookingId, int adminUserId, string reason, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            try
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    "dbo.usp_Booking_Release", new { BookingId = bookingId, AdminUserId = adminUserId, Reason = reason },
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.ToAppException() is { } appException)
            {
                throw appException;
            }
        }

        public async Task<IReadOnlyList<SeatChangeEntry>> GetSeatHistoryByUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            var rows = await connection.QueryAsync<SeatChangeEntry>(new CommandDefinition(
                "dbo.usp_SeatChangeHistory_GetByUser", new { UserId = userId },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return rows.AsList();
        }

        public async Task MoveSeatAsync(int bookingId, int toSeatId, int adminUserId, string? reason, CancellationToken cancellationToken = default)
        {
            var parameters = new { BookingId = bookingId, ToSeatId = toSeatId, AdminUserId = adminUserId, Reason = reason };

            using var connection = _dbContext.CreateConnection();
            try
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    "dbo.usp_Booking_MoveSeat", parameters,
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.IsUniqueViolation())
            {
                throw new ConflictException("That seat was just taken. Choose another seat.");
            }
            catch (SqlException ex) when (ex.ToAppException() is { } appException)
            {
                throw appException;
            }
        }
    }
}
