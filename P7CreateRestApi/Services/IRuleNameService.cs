using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Services
{
    public interface IRuleNameService
    {
        Task<List<RuleNameDto>> GetAllAsync();
        Task<RuleNameDto?> GetByIdAsync(int id);
        Task<RuleNameDto> CreateAsync(RuleNameDto dto);
        Task<bool> UpdateAsync(int id, RuleNameDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
