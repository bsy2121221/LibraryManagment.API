using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.DTOs.Notices;
using LibraryManagement.Models.Exceptions;
using LibraryManagement.Services.IServices;
using Microsoft.Extensions.Logging;

namespace LibraryManagement.Services.Services
{
    // Admin notices for the rotating bar above the navbar (e.g. "Closed on 5 Dec for maintenance")
    public class SiteNoticeService : ISiteNoticeService
    {
        private readonly ISiteNoticeRepository _repository;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<SiteNoticeService> _logger;

        public SiteNoticeService(ISiteNoticeRepository repository, TimeProvider timeProvider, ILogger<SiteNoticeService> logger)
        {
            _repository = repository;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task<IReadOnlyList<PublicNoticeDto>> GetCurrentAsync(CancellationToken cancellationToken = default)
        {
            var notices = await _repository.GetCurrentAsync(LibraryClock.Today(_timeProvider), cancellationToken);
            return notices.Select(n => new PublicNoticeDto { Id = n.Id, Message = n.Message }).ToList();
        }

        public async Task<IReadOnlyList<SiteNoticeDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var today = LibraryClock.Today(_timeProvider);
            var notices = await _repository.GetAllAsync(cancellationToken);
            return notices.Select(n => SiteNoticeDto.FromEntity(n, today)).ToList();
        }

        public async Task<SiteNoticeDto> SaveAsync(int adminUserId, int? id, SiteNoticeRequest request, CancellationToken cancellationToken = default)
        {
            var message = request.Message.Trim();
            if (message.Length < 3)
            {
                throw new BadRequestException("Notice text must be at least 3 characters.");
            }
            if (request.StartDate is { } start && request.EndDate is { } end && end < start)
            {
                throw new BadRequestException("The \"show until\" date cannot be before the \"show from\" date.");
            }

            var notice = await _repository.SaveAsync(id, message, request.StartDate, request.EndDate, request.IsActive, request.SortOrder, adminUserId, cancellationToken);
            _logger.LogInformation("Admin {AdminUserId} {Action} notice {NoticeId}", adminUserId, id is null ? "created" : "updated", notice.Id);
            return SiteNoticeDto.FromEntity(notice, LibraryClock.Today(_timeProvider));
        }

        public async Task DeleteAsync(int adminUserId, int id, CancellationToken cancellationToken = default)
        {
            await _repository.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("Admin {AdminUserId} deleted notice {NoticeId}", adminUserId, id);
        }
    }
}
