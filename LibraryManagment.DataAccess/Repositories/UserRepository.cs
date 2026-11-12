using Dapper;
using LibraryManagement.DataAccess.Context;
using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Exceptions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LibraryManagement.DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbContext _dbContext;

        public UserRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<User>(new CommandDefinition(
                "dbo.usp_User_GetById", new { Id = id },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task<User?> GetByMobileAsync(string mobile, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<User>(new CommandDefinition(
                "dbo.usp_User_GetByMobile", new { Mobile = mobile },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task<bool> MobileExistsAsync(string mobile, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
                "dbo.usp_User_MobileExists", new { Mobile = mobile },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task<bool> AnyAdminExistsAsync(CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
                "dbo.usp_User_AnyAdminExists",
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task<int> CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            // Stored procedures reject unknown parameters, so pass exactly what usp_User_Create declares
            var parameters = new
            {
                user.FullName,
                user.Mobile,
                user.Email,
                user.PasswordHash,
                user.Gender,
                user.Address,
                user.Role,
                user.IsMobileVerified,
                user.IsActive
            };

            using var connection = _dbContext.CreateConnection();
            try
            {
                return await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                    "dbo.usp_User_Create", parameters,
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627)
            {
                // Lost a race with another registration using the same mobile
                throw new ConflictException("This mobile number is already registered.");
            }
        }

        public async Task<IReadOnlyList<StudentSummary>> GetStudentsForAdminAsync(string? search, bool? isActive, bool? hasSeat, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            var rows = await connection.QueryAsync<StudentSummary>(new CommandDefinition(
                "dbo.usp_User_GetStudentsForAdmin", new { Search = search, IsActive = isActive, HasSeat = hasSeat },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return rows.AsList();
        }

        public async Task UpdateProfileAsync(User user, CancellationToken cancellationToken = default)
        {
            var parameters = new
            {
                user.Id,
                user.FullName,
                user.Mobile,
                user.Email,
                user.Gender,
                user.Address,
                user.IsActive
            };

            using var connection = _dbContext.CreateConnection();
            try
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    "dbo.usp_User_UpdateProfile", parameters,
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.IsUniqueViolation())
            {
                throw new ConflictException("This mobile number is already used by another account.");
            }
            catch (SqlException ex) when (ex.ToAppException() is { } appException)
            {
                throw appException;
            }
        }

        public async Task UpdatePasswordAsync(int userId, string passwordHash, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            try
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    "dbo.usp_User_UpdatePassword", new { Id = userId, PasswordHash = passwordHash },
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.ToAppException() is { } appException)
            {
                throw appException;
            }
        }
    }
}
