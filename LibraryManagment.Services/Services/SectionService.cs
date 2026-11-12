using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.DTOs.Sections;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Exceptions;
using LibraryManagement.Services.IServices;
using Microsoft.Extensions.Logging;

namespace LibraryManagement.Services.Services
{
    public class SectionService : ISectionService
    {
        private const int MaxSeatsPerSection = 1000;
        private const string SectionNotFoundMessage = "Section not found.";

        private readonly ISectionRepository _sectionRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly ILogger<SectionService> _logger;

        public SectionService(ISectionRepository sectionRepository, ISeatRepository seatRepository, ILogger<SectionService> logger)
        {
            _sectionRepository = sectionRepository;
            _seatRepository = seatRepository;
            _logger = logger;
        }

        public async Task<IReadOnlyList<SectionDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var sections = await _sectionRepository.GetAllAsync(cancellationToken);
            return sections.Select(SectionDto.FromEntity).ToList();
        }

        public async Task<SectionLayoutDto> GetLayoutAsync(int id, CancellationToken cancellationToken = default)
        {
            var section = await GetSectionOrThrowAsync(id, cancellationToken);
            var seats = await _seatRepository.GetBySectionAsync(id, cancellationToken);

            return new SectionLayoutDto
            {
                Section = SectionDto.FromEntity(section),
                Seats = seats.Select(AdminSeatDto.FromEntity).ToList()
            };
        }

        public async Task<SectionDto> CreateAsync(SectionRequest request, CancellationToken cancellationToken = default)
        {
            var section = ToEntity(request);
            var id = await _sectionRepository.CreateAsync(section, cancellationToken);

            _logger.LogInformation("Section {SectionId} '{Name}' created", id, section.Name);
            return SectionDto.FromEntity(await GetSectionOrThrowAsync(id, cancellationToken));
        }

        public async Task<SectionDto> UpdateAsync(int id, SectionRequest request, CancellationToken cancellationToken = default)
        {
            var section = ToEntity(request);
            section.Id = id;
            await _sectionRepository.UpdateAsync(section, cancellationToken);

            _logger.LogInformation("Section {SectionId} updated", id);
            return SectionDto.FromEntity(await GetSectionOrThrowAsync(id, cancellationToken));
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await _sectionRepository.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("Section {SectionId} deleted", id);
        }

        public async Task<SectionLayoutDto> SaveLayoutAsync(int id, SaveSeatLayoutRequest request, CancellationToken cancellationToken = default)
        {
            var section = await GetSectionOrThrowAsync(id, cancellationToken);

            var seats = request.Seats.Select(item => new Seat
            {
                Id = item.Id ?? 0,
                SectionId = id,
                SeatLabel = item.SeatLabel.Trim(),
                PosX = Math.Round(item.PosX, 2),
                PosY = Math.Round(item.PosY, 2),
                Rotation = NormalizeRotation(item.Rotation),
                Status = item.Status
            }).ToList();

            ValidateLayout(section, seats);

            await _seatRepository.SaveLayoutAsync(id, seats, cancellationToken);
            _logger.LogInformation("Seat layout of section {SectionId} saved with {SeatCount} seats", id, seats.Count);

            return await GetLayoutAsync(id, cancellationToken);
        }

        public async Task<SectionLayoutDto> GenerateSeatsAsync(int id, GenerateSeatsRequest request, CancellationToken cancellationToken = default)
        {
            var section = await GetSectionOrThrowAsync(id, cancellationToken);

            var lastX = request.OriginX + request.SpacingX * (request.Columns - 1);
            var lastY = request.OriginY + request.SpacingY * (request.Rows - 1);
            if (lastX > section.LayoutWidth || lastY > section.LayoutHeight)
            {
                throw new BadRequestException(
                    $"{request.Rows} x {request.Columns} seats do not fit in a {section.LayoutWidth} x {section.LayoutHeight} layout. " +
                    "Use fewer rows/columns, smaller spacing, or make the layout bigger.");
            }

            // Keep every existing seat as it is and append the new grid
            var existing = await _seatRepository.GetBySectionAsync(id, cancellationToken);
            var seats = existing.Select(s => new Seat
            {
                Id = s.Id,
                SectionId = id,
                SeatLabel = s.SeatLabel,
                PosX = s.PosX,
                PosY = s.PosY,
                Rotation = s.Rotation,
                Status = s.Status
            }).ToList();

            var usedLabels = new HashSet<string>(seats.Select(s => s.SeatLabel), StringComparer.OrdinalIgnoreCase);
            var prefix = request.LabelPrefix?.Trim() ?? string.Empty;
            var number = request.StartNumber;

            for (var row = 0; row < request.Rows; row++)
            {
                for (var column = 0; column < request.Columns; column++)
                {
                    string label;
                    do
                    {
                        label = $"{prefix}{number++}";
                    } while (!usedLabels.Add(label));

                    seats.Add(new Seat
                    {
                        SectionId = id,
                        SeatLabel = label,
                        PosX = request.OriginX + request.SpacingX * column,
                        PosY = request.OriginY + request.SpacingY * row,
                        Status = Models.Enums.SeatStatus.Active
                    });
                }
            }

            ValidateLayout(section, seats);

            await _seatRepository.SaveLayoutAsync(id, seats, cancellationToken);
            _logger.LogInformation("Generated {SeatCount} seats in section {SectionId}", request.Rows * request.Columns, id);

            return await GetLayoutAsync(id, cancellationToken);
        }

        private async Task<SectionWithStats> GetSectionOrThrowAsync(int id, CancellationToken cancellationToken)
        {
            return await _sectionRepository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException(SectionNotFoundMessage);
        }

        private static void ValidateLayout(Section section, List<Seat> seats)
        {
            if (seats.Count > MaxSeatsPerSection)
            {
                throw new BadRequestException($"A section can have at most {MaxSeatsPerSection} seats.");
            }

            var outside = seats.FirstOrDefault(s => s.PosX > section.LayoutWidth || s.PosY > section.LayoutHeight);
            if (outside is not null)
            {
                throw new BadRequestException(
                    $"Seat \"{outside.SeatLabel}\" is outside the {section.LayoutWidth} x {section.LayoutHeight} layout.");
            }

            var duplicate = seats
                .GroupBy(s => s.SeatLabel, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);
            if (duplicate is not null)
            {
                throw new BadRequestException($"Seat label \"{duplicate.Key}\" is used more than once.");
            }
        }

        private static short NormalizeRotation(short rotation) => (short)(((rotation % 360) + 360) % 360);

        private static Section ToEntity(SectionRequest request) => new()
        {
            Name = request.Name.Trim(),
            Gender = request.Gender!.Value,
            LayoutWidth = request.LayoutWidth,
            LayoutHeight = request.LayoutHeight,
            BackgroundImageUrl = string.IsNullOrWhiteSpace(request.BackgroundImageUrl) ? null : request.BackgroundImageUrl.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };
    }
}
