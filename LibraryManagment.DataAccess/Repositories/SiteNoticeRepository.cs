using Dapper;
using LibraryManagement.DataAccess.Context;
using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.Entities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LibraryManagement.DataAccess.Repositories
{
    public class SiteNoticeRepository : ISiteNoticeRepository
    {
        private readonly IDbContext _dbContext;

        public SiteNoticeRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<SiteNotice>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            var rows = await connection.QueryAsync<SiteNotice>(new CommandDefinition(
                "dbo.usp_SiteNotice_GetAll",
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return rows.AsList();
        }

        public async Task<IReadOnlyList<SiteNotice>> GetCurrentAsync(DateOnly today, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            var rows = await connection.QueryAsync<SiteNotice>(new CommandDefinition(
                "dbo.usp_SiteNotice_GetCurrent", new { Today = today },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return rows.AsList();
        }

        public async Task<SiteNotice> SaveAsync(int? id, string message, DateOnly? startDate, DateOnly? endDate, bool isActive, int sortOrder, int adminUserId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            try
            {
                return await connection.QuerySingleAsync<SiteNotice>(new CommandDefinition(
                    "dbo.usp_SiteNotice_Save",
                    new
                    {
                        Id = id,
                        Message = message,
                        StartDate = startDate,
                        EndDate = endDate,
                        IsActive = isActive,
                        SortOrder = sortOrder,
                        AdminUserId = adminUserId
                    },
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.ToAppException() is { } appException)
            {
                throw appException;
            }
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            try
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    "dbo.usp_SiteNotice_Delete", new { Id = id },
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.ToAppException() is { } appException)
            {
                throw appException;
            }
        }
    }
}
