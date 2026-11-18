using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Services
{
    public interface IBidListService
    {
        Task<List<BidListDto>> GetAllAsync();
        Task<BidListDto?> GetByIdAsync(int id);
        Task<BidListDto> CreateAsync(BidListDto dto, string? userName);
        Task<bool> UpdateAsync(int id, BidListDto dto, string? userName);
        Task<bool> DeleteAsync(int id);
    }
}
