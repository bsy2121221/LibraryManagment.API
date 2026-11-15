using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.DTOs.HomePage;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Exceptions;
using LibraryManagement.Services.IServices;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LibraryManagement.Services.Services
{
    public class HomePageService : IHomePageService
    {
        // Block types the website knows how to show (see LibraryManagment_UI/src/components/home/blockTypes.js)
        private static readonly HashSet<string> BlockTypes =
            ["hero", "features", "text", "imageText", "gallery", "pricing", "faq", "cta"];

        private const int MaxBlocks = 40;
        private const int MaxJsonLength = 300_000;
        private const int MaxTextLength = 5_000;
        private const int HistorySize = 50;

        private readonly IHomePageRepository _repository;
        private readonly ILogger<HomePageService> _logger;

        public HomePageService(IHomePageRepository repository, ILogger<HomePageService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<PublicHomePageDto?> GetPublishedAsync(CancellationToken cancellationToken = default)
        {
            var layout = await _repository.GetPublishedAsync(cancellationToken);
            if (layout?.LayoutJson is null)
            {
                return null;
            }

            return new PublicHomePageDto
            {
                Version = layout.Version,
                Layout = Parse(layout.LayoutJson),
                PublishedAt = layout.PublishedAt
            };
        }

        public async Task<AdminHomePageDto> GetForAdminAsync(CancellationToken cancellationToken = default)
        {
            var latest = await _repository.GetByIdAsync(null, cancellationToken);
            var live = await _repository.GetPublishedAsync(cancellationToken);
            return new AdminHomePageDto
            {
                Latest = latest is null ? null : ToDto(latest, live?.Id),
                LiveVersion = live?.Version
            };
        }

        public async Task<IReadOnlyList<HomePageVersionDto>> GetHistoryAsync(CancellationToken cancellationToken = default)
        {
            var rows = await _repository.GetHistoryAsync(HistorySize, cancellationToken);
            return rows.Select(r => ToDto(r, r.IsLive ? r.Id : null)).ToList();
        }

        public async Task<HomePageVersionDto> GetVersionAsync(int id, CancellationToken cancellationToken = default)
        {
            var layout = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("Home page version not found.");
            var live = await _repository.GetPublishedAsync(cancellationToken);
            return ToDto(layout, live?.Id);
        }

        public async Task<HomePageVersionDto> SaveAsync(int adminUserId, JsonElement layout, bool publish, CancellationToken cancellationToken = default)
        {
            var json = Validate(layout);
            var saved = await _repository.SaveAsync(json, publish, adminUserId, cancellationToken);
            _logger.LogInformation("Admin {AdminUserId} saved home page version {Version} (published: {Published})",
                adminUserId, saved.Version, publish);

            var live = publish ? saved : await _repository.GetPublishedAsync(cancellationToken);
            return ToDto(saved, live?.Id);
        }

        public async Task<HomePageVersionDto> PublishAsync(int adminUserId, int id, CancellationToken cancellationToken = default)
        {
            await _repository.PublishAsync(id, cancellationToken);
            _logger.LogInformation("Admin {AdminUserId} published home page version id {Id}", adminUserId, id);
            return await GetVersionAsync(id, cancellationToken);
        }

        // Checks the shape the website renders: { "blocks": [ { "id": "...", "type": "hero", ... } ] }.
        // Text is shown as plain text by the website, so only links need checking for script URLs.
        private static string Validate(JsonElement layout)
        {
            if (layout.ValueKind != JsonValueKind.Object
                || !layout.TryGetProperty("blocks", out var blocks)
                || blocks.ValueKind != JsonValueKind.Array)
            {
                throw new BadRequestException("The page design must have a list of blocks.");
            }

            if (blocks.GetArrayLength() > MaxBlocks)
            {
                throw new BadRequestException($"A page can have at most {MaxBlocks} blocks.");
            }

            var position = 0;
            foreach (var block in blocks.EnumerateArray())
            {
                position++;
                if (block.ValueKind != JsonValueKind.Object
                    || !block.TryGetProperty("type", out var type)
                    || type.ValueKind != JsonValueKind.String
                    || !BlockTypes.Contains(type.GetString()!))
                {
                    throw new BadRequestException($"Block {position} has an unknown type.");
                }
            }

            CheckValues(layout);

            var json = layout.GetRawText();
            if (json.Length > MaxJsonLength)
            {
                throw new BadRequestException("The page design is too large. Remove some blocks or shorten the text.");
            }
            return json;
        }

        private static void CheckValues(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject()) CheckValues(property.Value);
                    break;

                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray()) CheckValues(item);
                    break;

                case JsonValueKind.String:
                    var value = element.GetString()!;
                    if (value.Length > MaxTextLength)
                    {
                        throw new BadRequestException($"A text in the page is longer than {MaxTextLength} characters.");
                    }
                    if (IsScriptUrl(value))
                    {
                        throw new BadRequestException("Links must start with https://, / , tel: or mailto:.");
                    }
                    break;
            }
        }

        private static bool IsScriptUrl(string value)
        {
            var compact = new string(value.Where(c => !char.IsWhiteSpace(c) && !char.IsControl(c)).ToArray());
            return compact.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)
                || compact.StartsWith("vbscript:", StringComparison.OrdinalIgnoreCase)
                || compact.StartsWith("data:", StringComparison.OrdinalIgnoreCase);
        }

        private static JsonElement Parse(string json)
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.Clone();
        }

        private static HomePageVersionDto ToDto(HomePageLayout layout, int? liveId) => new()
        {
            Id = layout.Id,
            Version = layout.Version,
            Status = layout.Status.ToString(),
            IsLive = liveId == layout.Id,
            Layout = layout.LayoutJson is null ? null : Parse(layout.LayoutJson),
            CreatedByName = layout.CreatedByName,
            CreatedAt = layout.CreatedAt,
            PublishedAt = layout.PublishedAt
        };
    }
}
