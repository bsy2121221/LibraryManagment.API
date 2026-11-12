using LibraryManagement.Models.Entities;

namespace LibraryManagement.DataAccess.IRepositories
{
    public interface ISettingRepository
    {
        Task<IReadOnlyList<Setting>> GetAllAsync(CancellationToken cancellationToken = default);
        Task UpdateManyAsync(IReadOnlyDictionary<string, string> values, int updatedByUserId, CancellationToken cancellationToken = default);
    }
}
