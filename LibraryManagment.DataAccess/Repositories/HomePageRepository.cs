using Dapper;
using LibraryManagement.DataAccess.Context;
using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.Entities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LibraryManagement.DataAccess.Repositories
{
    public class HomePageRepository : IHomePageRepository
    {
        private readonly IDbContext _dbContext;

        public HomePageRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HomePageLayout?> GetPublishedAsync(CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<HomePageLayout>(new CommandDefinition(
                "dbo.usp_HomePageLayout_GetPublished",
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task<HomePageLayout?> GetByIdAsync(int? id, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<HomePageLayout>(new CommandDefinition(
                "dbo.usp_HomePageLayout_GetById", new { Id = id },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task<IReadOnlyList<HomePageLayout>> GetHistoryAsync(int top, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            var rows = await connection.QueryAsync<HomePageLayout>(new CommandDefinition(
                "dbo.usp_HomePageLayout_GetHistory", new { Top = top },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return rows.AsList();
        }

        public async Task<HomePageLayout> SaveAsync(string layoutJson, bool publish, int createdByUserId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            try
            {
                return await connection.QuerySingleAsync<HomePageLayout>(new CommandDefinition(
                    "dbo.usp_HomePageLayout_Save",
                    new { LayoutJson = layoutJson, Publish = publish, CreatedByUserId = createdByUserId },
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.ToAppException() is { } appException)
            {
                throw appException;
            }
        }

        public async Task PublishAsync(int id, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            try
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    "dbo.usp_HomePageLayout_Publish", new { Id = id },
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.ToAppException() is { } appException)
            {
                throw appException;
            }
        }
    }

    public class MediaRepository : IMediaRepository
    {
        private readonly IDbContext _dbContext;

        public MediaRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<MediaAsset>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            var rows = await connection.QueryAsync<MediaAsset>(new CommandDefinition(
                "dbo.usp_MediaAsset_GetAll",
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return rows.AsList();
        }

        public async Task<MediaAsset> CreateAsync(MediaAsset asset, CancellationToken cancellationToken = default)
        {
            var parameters = new
            {
                asset.FileName,
                asset.StoredFileName,
                asset.Url,
                asset.ContentType,
                asset.SizeBytes,
                asset.UploadedByUserId
            };

            using var connection = _dbContext.CreateConnection();
            return await connection.QuerySingleAsync<MediaAsset>(new CommandDefinition(
                "dbo.usp_MediaAsset_Create", parameters,
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task<string> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            try
            {
                return await connection.ExecuteScalarAsync<string>(new CommandDefinition(
                    "dbo.usp_MediaAsset_Delete", new { Id = id },
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken)) ?? string.Empty;
            }
            catch (SqlException ex) when (ex.ToAppException() is { } appException)
            {
                throw appException;
            }
        }
    }
}
