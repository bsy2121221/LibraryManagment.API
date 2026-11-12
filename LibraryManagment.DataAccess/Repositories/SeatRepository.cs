using Dapper;
using LibraryManagement.DataAccess.Context;
using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.DTOs.SeatMap;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Exceptions;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace LibraryManagement.DataAccess.Repositories
{
    public class SeatRepository : ISeatRepository
    {
        private readonly IDbContext _dbContext;

        public SeatRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<SeatWithOccupant>> GetBySectionAsync(int sectionId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            var rows = await connection.QueryAsync<SeatWithOccupant>(new CommandDefinition(
                "dbo.usp_Seat_GetBySection", new { SectionId = sectionId },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            return rows.AsList();
        }

        public async Task SaveLayoutAsync(int sectionId, IEnumerable<Seat> seats, CancellationToken cancellationToken = default)
        {
            // Property names and numeric enums must match the OPENJSON paths in usp_Seat_SaveLayout
            var seatsJson = JsonSerializer.Serialize(seats.Select(s => new
            {
                Id = s.Id == 0 ? (int?)null : s.Id,
                s.SeatLabel,
                s.PosX,
                s.PosY,
                s.Rotation,
                Status = (byte)s.Status
            }));

            using var connection = _dbContext.CreateConnection();
            try
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    "dbo.usp_Seat_SaveLayout", new { SectionId = sectionId, SeatsJson = seatsJson },
                    commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));
            }
            catch (SqlException ex) when (ex.IsUniqueViolation())
            {
                throw new ConflictException("A seat label is already used in this section. Reload the layout and try again.");
            }
            catch (SqlException ex) when (ex.ToAppException() is { } appException)
            {
                throw appException;
            }
        }

        public async Task<SeatMapDto> GetSeatMapAsync(int? userId, CancellationToken cancellationToken = default)
        {
            using var connection = _dbContext.CreateConnection();
            using var results = await connection.QueryMultipleAsync(new CommandDefinition(
                "dbo.usp_SeatMap_Get", new { UserId = userId },
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken));

            var sections = (await results.ReadAsync<SeatMapSectionDto>()).AsList();
            var seatsBySection = (await results.ReadAsync<SeatMapSeatDto>()).ToLookup(s => s.SectionId);

            foreach (var section in sections)
            {
                section.Seats = seatsBySection[section.Id].ToList();
            }

            return new SeatMapDto { Sections = sections };
        }
    }
}
