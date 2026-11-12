using Dapper;
using LibraryManagement.DataAccess.Context;
using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Exceptions;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace LibraryManagement.DataAccess.Repositories
{
    public class SettingRepository : ISettingRepository
    {
        private readonly IDbContext _dbContext;

        public SettingRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<Setting>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            var rows = await connection.QueryAsync<Setting>(new CommandDefinition(
                "dbo.usp_Setting_GetAll",
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return rows.AsList();
        }

        public async Task UpdateManyAsync(IReadOnlyDictionary<string, string> values, int updatedByUserId, CancellationToken cancellationToken = default)
        {
            // Property names must match the OPENJSON paths in usp_Setting_UpdateMany
            var settingsJson = JsonSerializer.Serialize(values.Select(v => new { v.Key, v.Value }));

            using var connection = _dbContext.CreateConnection();
            try
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    "dbo.usp_Setting_UpdateMany", new { SettingsJson = settingsJson, UpdatedByUserId = updatedByUserId },
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.ToAppException() is { } appException)
            {
                throw appException;
            }
        }
    }
}
