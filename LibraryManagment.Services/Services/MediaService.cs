using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.DTOs.HomePage;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Exceptions;
using LibraryManagement.Models.Options;
using LibraryManagement.Services.IServices;
using LibraryManagement.Services.Media;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LibraryManagement.Services.Services
{
    public class MediaService : IMediaService
    {
        private readonly IMediaRepository _repository;
        private readonly IMediaStorage _storage;
        private readonly StorageOptions _options;
        private readonly ILogger<MediaService> _logger;

        public MediaService(IMediaRepository repository, IMediaStorage storage, IOptions<StorageOptions> options, ILogger<MediaService> logger)
        {
            _repository = repository;
            _storage = storage;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<IReadOnlyList<MediaAssetDto>> GetAllAsync(CancellationToken cancellationToken = default)
            => (await _repository.GetAllAsync(cancellationToken)).Select(ToDto).ToList();

        public async Task<MediaAssetDto> UploadAsync(int adminUserId, Stream content, string fileName, long length, CancellationToken cancellationToken = default)
        {
            var maxBytes = _options.MaxUploadMb * 1024L * 1024L;
            if (length <= 0)
            {
                throw new BadRequestException("The file is empty.");
            }
            if (length > maxBytes)
            {
                throw new BadRequestException($"The image is too large. The limit is {_options.MaxUploadMb} MB.");
            }

            // Detect the format from the file's first bytes; the browser's file name and type are not trusted.
            // SVG is not accepted because it can contain scripts.
            using var buffer = new MemoryStream();
            await content.CopyToAsync(buffer, cancellationToken);
            var format = ImageFormat.Detect(buffer.GetBuffer().AsSpan(0, (int)buffer.Length))
                ?? throw new BadRequestException("Only JPG, PNG, WEBP and GIF images can be uploaded.");

            var storedFileName = $"{Guid.NewGuid():N}{format.Extension}";
            buffer.Position = 0;
            var url = await _storage.SaveAsync(buffer, storedFileName, format.ContentType, cancellationToken);

            try
            {
                var asset = await _repository.CreateAsync(new MediaAsset
                {
                    FileName = CleanFileName(fileName, format.Extension),
                    StoredFileName = storedFileName,
                    Url = url,
                    ContentType = format.ContentType,
                    SizeBytes = buffer.Length,
                    UploadedByUserId = adminUserId
                }, cancellationToken);

                _logger.LogInformation("Admin {AdminUserId} uploaded image {StoredFileName} ({Bytes} bytes)", adminUserId, storedFileName, buffer.Length);
                return ToDto(asset);
            }
            catch
            {
                // Do not leave an orphan file when the database insert fails
                await _storage.DeleteAsync(storedFileName, CancellationToken.None);
                throw;
            }
        }

        public async Task DeleteAsync(int adminUserId, int id, CancellationToken cancellationToken = default)
        {
            var storedFileName = await _repository.DeleteAsync(id, cancellationToken);
            try
            {
                await _storage.DeleteAsync(storedFileName, CancellationToken.None);
            }
            catch (Exception ex)
            {
                // The record is gone; a leftover file is harmless
                _logger.LogWarning(ex, "Could not delete media file {StoredFileName}", storedFileName);
            }
            _logger.LogInformation("Admin {AdminUserId} deleted image {Id}", adminUserId, id);
        }

        private static string CleanFileName(string fileName, string extension)
        {
            var name = Path.GetFileName(fileName ?? string.Empty).Trim();
            if (name.Length == 0) name = "image" + extension;
            return name.Length > 200 ? name[..200] : name;
        }

        private static MediaAssetDto ToDto(MediaAsset a) => new()
        {
            Id = a.Id,
            FileName = a.FileName,
            Url = a.Url,
            ContentType = a.ContentType,
            SizeBytes = a.SizeBytes,
            UploadedByName = a.UploadedByName,
            CreatedAt = a.CreatedAt
        };

        private sealed record ImageFormat(string Extension, string ContentType)
        {
            public static ImageFormat? Detect(ReadOnlySpan<byte> b)
            {
                if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF)
                    return new(".jpg", "image/jpeg");
                if (b.Length >= 8 && b[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
                    return new(".png", "image/png");
                if (b.Length >= 6 && (b[..6].SequenceEqual("GIF87a"u8) || b[..6].SequenceEqual("GIF89a"u8)))
                    return new(".gif", "image/gif");
                if (b.Length >= 12 && b[..4].SequenceEqual("RIFF"u8) && b[8..12].SequenceEqual("WEBP"u8))
                    return new(".webp", "image/webp");
                return null;
            }
        }
    }
}
