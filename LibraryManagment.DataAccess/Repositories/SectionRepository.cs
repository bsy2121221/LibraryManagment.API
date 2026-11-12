using Dapper;
using LibraryManagement.DataAccess.Context;
using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Exceptions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LibraryManagement.DataAccess.Repositories
{
    public class SectionRepository : ISectionRepository
    {
        private const string DuplicateNameMessage = "A section with this name already exists.";

        private readonly IDbContext _dbContext;

        public SectionRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<SectionWithStats>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            var rows = await connection.QueryAsync<SectionWithStats>(new CommandDefinition(
                "dbo.usp_Section_GetAll",
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return rows.AsList();
        }

        public async Task<SectionWithStats?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<SectionWithStats>(new CommandDefinition(
                "dbo.usp_Section_GetById", new { Id = id },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
        }

        public async Task<int> CreateAsync(Section section, CancellationToken cancellationToken = default)
        {
            var parameters = new
            {
                section.Name,
                section.Gender,
                section.LayoutWidth,
                section.LayoutHeight,
                section.BackgroundImageUrl,
                section.DisplayOrder,
                section.IsActive
            };

            using var connection = _dbContext.CreateConnection();
            try
            {
                return await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                    "dbo.usp_Section_Create", parameters,
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.IsUniqueViolation())
            {
                throw new ConflictException(DuplicateNameMessage);
            }
        }

        public async Task UpdateAsync(Section section, CancellationToken cancellationToken = default)
        {
            var parameters = new
            {
                section.Id,
                section.Name,
                section.Gender,
                section.LayoutWidth,
                section.LayoutHeight,
                section.BackgroundImageUrl,
                section.DisplayOrder,
                section.IsActive
            };

            using var connection = _dbContext.CreateConnection();
            try
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    "dbo.usp_Section_Update", parameters,
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.IsUniqueViolation())
            {
                throw new ConflictException(DuplicateNameMessage);
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
                    "dbo.usp_Section_Delete", new { Id = id },
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.ToAppException() is { } appException)
            {
                throw appException;
            }
        }
    }
}
