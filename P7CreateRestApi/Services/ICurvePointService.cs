using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Services
{
    public interface ICurvePointService
    {
        Task<List<CurvePointDto>> GetAllAsync();
        Task<CurvePointDto?> GetByIdAsync(int id);
        Task<CurvePointDto> CreateAsync(CurvePointDto dto);
        Task<bool> UpdateAsync(int id, CurvePointDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
