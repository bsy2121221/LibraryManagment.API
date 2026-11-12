using Dapper;
using LibraryManagement.DataAccess.Context;
using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.Entities;
using System.Data;

namespace LibraryManagement.DataAccess.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IDbContext _dbContext;

        public RefreshTokenRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<RefreshToken>(new CommandDefinition(
                "dbo.usp_RefreshToken_GetByHash", new { TokenHash = tokenHash },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task CreateAsync(RefreshToken token, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            await connection.ExecuteAsync(new CommandDefinition(
                "dbo.usp_RefreshToken_Create",
                new { token.UserId, token.TokenHash, token.ExpiresAt },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task RevokeAsync(int id, string? replacedByTokenHash, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            await connection.ExecuteAsync(new CommandDefinition(
                "dbo.usp_RefreshToken_Revoke",
                new { Id = id, ReplacedByTokenHash = replacedByTokenHash },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task RevokeAllForUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            await connection.ExecuteAsync(new CommandDefinition(
                "dbo.usp_RefreshToken_RevokeAllForUser", new { UserId = userId },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }
    }
}
