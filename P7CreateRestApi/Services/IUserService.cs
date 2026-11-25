using Microsoft.AspNetCore.Identity;
using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(int id);

        // The result carries the Identity errors (weak password, user name already taken).
        Task<IdentityResult> CreateAsync(UserInputDto dto);

        // Null when no user has this id.
        Task<IdentityResult?> UpdateAsync(int id, UserInputDto dto);
        Task<IdentityResult?> DeleteAsync(int id);
    }
}
