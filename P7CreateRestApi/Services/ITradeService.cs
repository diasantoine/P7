using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Services
{
    public interface ITradeService
    {
        Task<List<TradeDto>> GetAllAsync();
        Task<TradeDto?> GetByIdAsync(int id);
        Task<TradeDto> CreateAsync(TradeDto dto, string? userName);
        Task<bool> UpdateAsync(int id, TradeDto dto, string? userName);
        Task<bool> DeleteAsync(int id);
    }
}
