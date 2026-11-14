using LibraryManagement.Auth.Services;
using LibraryManagement.DataAccess.IRepositories;
using LibraryManagement.Models.DTOs.Bookings;
using LibraryManagement.Models.DTOs.Fees;
using LibraryManagement.Models.DTOs.Students;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.Enums;
using LibraryManagement.Models.Exceptions;
using LibraryManagement.Services.IServices;
using Microsoft.Extensions.Logging;

namespace LibraryManagement.Services.Services
{
    public class StudentService : IStudentService
    {
        private readonly IUserRepository _userRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IFeeRepository _feeRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<StudentService> _logger;

        public StudentService(
            IUserRepository userRepository,
            IBookingRepository bookingRepository,
            IPaymentRepository paymentRepository,
            IFeeRepository feeRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordHasher passwordHasher,
            TimeProvider timeProvider,
            ILogger<StudentService> logger)
        {
            _userRepository = userRepository;
            _bookingRepository = bookingRepository;
            _paymentRepository = paymentRepository;
            _feeRepository = feeRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task<IReadOnlyList<StudentListItemDto>> GetAllAsync(string? search, bool? isActive, bool? hasSeat, CancellationToken cancellationToken = default)
        {
            var students = await _userRepository.GetStudentsForAdminAsync(search?.Trim(), isActive, hasSeat, cancellationToken);
            return students.Select(StudentListItemDto.FromEntity).ToList();
        }

        public async Task<StudentDetailDto> CreateAsync(int adminUserId, CreateStudentRequest request, CancellationToken cancellationToken = default)
        {
            var mobile = request.Mobile.Trim();
            if (await _userRepository.MobileExistsAsync(mobile, cancellationToken))
            {
                throw new ConflictException("This mobile number is already registered.");
            }

            var id = await _userRepository.CreateByAdminAsync(new User
            {
                FullName = request.FullName.Trim(),
                Mobile = mobile,
                Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                PasswordHash = _passwordHasher.Hash(request.Password),
                Gender = request.Gender,
                Address = request.Address.Trim(),
                Role = UserRole.Student,
                IsActive = true
            }, cancellationToken);

            _logger.LogInformation("Admin {AdminUserId} registered walk-in student {StudentId}", adminUserId, id);
            return await GetDetailAsync(id, cancellationToken);
        }

        public async Task<StudentDetailDto> GetDetailAsync(int studentId, CancellationToken cancellationToken = default)
        {
            var student = await GetStudentOrThrowAsync(studentId, cancellationToken);

            var booking = await _bookingRepository.GetLiveByUserAsync(studentId, cancellationToken);
            var history = await _bookingRepository.GetSeatHistoryByUserAsync(studentId, cancellationToken);
            var payments = await _paymentRepository.GetByUserAsync(studentId, cancellationToken);
            var fees = await _feeRepository.GetByUserAsync(studentId, cancellationToken);

            return new StudentDetailDto
            {
                Profile = StudentProfileDto.FromEntity(student),
                CurrentBooking = booking is null ? null : BookingDto.FromEntity(booking, _timeProvider.GetUtcNow().UtcDateTime),
                SeatHistory = history.Select(SeatChangeDto.FromEntity).ToList(),
                Payments = payments.Select(StudentPaymentDto.FromEntity).ToList(),
                Fees = fees.Select(FeeInvoiceDto.FromEntity).ToList()
            };
        }

        public async Task<StudentDetailDto> UpdateAsync(int adminUserId, int studentId, UpdateStudentRequest request, CancellationToken cancellationToken = default)
        {
            var current = await GetStudentOrThrowAsync(studentId, cancellationToken);

            var updated = new User
            {
                Id = studentId,
                FullName = request.FullName.Trim(),
                Mobile = request.Mobile.Trim(),
                Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                Gender = request.Gender,
                Address = request.Address.Trim(),
                IsActive = request.IsActive
            };
            await _userRepository.UpdateProfileAsync(updated, cancellationToken);

            // A deactivated student, or one whose login mobile changed, must log in again
            if ((current.IsActive && !updated.IsActive) || current.Mobile != updated.Mobile)
            {
                await _refreshTokenRepository.RevokeAllForUserAsync(studentId, cancellationToken);
            }

            _logger.LogInformation("Admin {AdminUserId} updated student {StudentId} (active: {IsActive})", adminUserId, studentId, updated.IsActive);
            return await GetDetailAsync(studentId, cancellationToken);
        }

        public async Task ResetPasswordAsync(int adminUserId, int studentId, string newPassword, CancellationToken cancellationToken = default)
        {
            await GetStudentOrThrowAsync(studentId, cancellationToken);

            await _userRepository.UpdatePasswordAsync(studentId, _passwordHasher.Hash(newPassword), cancellationToken);
            await _refreshTokenRepository.RevokeAllForUserAsync(studentId, cancellationToken);

            _logger.LogInformation("Admin {AdminUserId} reset the password of student {StudentId}", adminUserId, studentId);
        }

        private async Task<User> GetStudentOrThrowAsync(int studentId, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(studentId, cancellationToken);
            if (user is null || user.Role != UserRole.Student)
            {
                throw new NotFoundException("Student not found.");
            }
            return user;
        }
    }
}
