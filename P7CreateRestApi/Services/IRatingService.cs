using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Services
{
    public interface IRatingService
    {
        Task<List<RatingDto>> GetAllAsync();
        Task<RatingDto?> GetByIdAsync(int id);
        Task<RatingDto> CreateAsync(RatingDto dto);
        Task<bool> UpdateAsync(int id, RatingDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
