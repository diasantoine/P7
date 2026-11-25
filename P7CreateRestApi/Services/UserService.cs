using Dot.Net.WebApi.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Services
{
    // Users go through Identity's UserManager instead of the generic repository:
    // it hashes the password, applies the password rules and refuses duplicate user names.
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;

        public UserService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            List<User> users = await _userManager.Users.ToListAsync();
            var result = new List<UserDto>();

            foreach (User user in users)
            {
                result.Add(await ToDtoAsync(user));
            }

            return result;
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            User? user = await _userManager.FindByIdAsync(id.ToString());

            return user == null ? null : await ToDtoAsync(user);
        }

        public async Task<IdentityResult> CreateAsync(UserInputDto dto)
        {
            var user = new User { UserName = dto.Username, Fullname = dto.Fullname };

            IdentityResult result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                return result;
            }

            return await _userManager.AddToRoleAsync(user, dto.Role);
        }

        public async Task<IdentityResult?> UpdateAsync(int id, UserInputDto dto)
        {
            User? user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                return null;
            }

            // Check the new password before changing anything, so a weak one leaves the account untouched.
            foreach (IPasswordValidator<User> validator in _userManager.PasswordValidators)
            {
                IdentityResult passwordResult = await validator.ValidateAsync(_userManager, user, dto.Password);

                if (!passwordResult.Succeeded)
                {
                    return passwordResult;
                }
            }

            user.UserName = dto.Username;
            user.Fullname = dto.Fullname;
            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, dto.Password);

            IdentityResult result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return result;
            }

            IList<string> currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Count == 1 && currentRoles[0] == dto.Role)
            {
                return result;
            }

            result = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!result.Succeeded)
            {
                return result;
            }

            return await _userManager.AddToRoleAsync(user, dto.Role);
        }

        public async Task<IdentityResult?> DeleteAsync(int id)
        {
            User? user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                return null;
            }

            return await _userManager.DeleteAsync(user);
        }

        // The password hash is never copied: it stays in the database.
        private async Task<UserDto> ToDtoAsync(User user)
        {
            IList<string> roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Username = user.UserName,
                Fullname = user.Fullname,
                Role = roles.FirstOrDefault()
            };
        }
    }
}
