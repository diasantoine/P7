using Dot.Net.WebApi.Security;
using Dot.Net.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Controllers
{
    // Managing the accounts is reserved to the administrators: any other user gets 403.
    [ApiController]
    [Route("users")]
    [Authorize(Roles = Roles.Admin)]
    public class UserController : ControllerBase
    {
        public const string OwnAccountMessage = "Vous ne pouvez pas supprimer votre propre compte.";

        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _userService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            UserDto? user = await _userService.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound(NotFoundMessage(id));
            }

            return Ok(user);
        }

        // [ApiController] checks the DTO first: an invalid body never reaches this method (400 with the messages).
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserInputDto user)
        {
            IdentityResult result = await _userService.CreateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(Messages(result));
            }

            return Ok(await _userService.GetAllAsync());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserInputDto user)
        {
            IdentityResult? result = await _userService.UpdateAsync(id, user);

            if (result == null)
            {
                return NotFound(NotFoundMessage(id));
            }

            if (!result.Succeeded)
            {
                return BadRequest(Messages(result));
            }

            return Ok(await _userService.GetAllAsync());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            UserDto? user = await _userService.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound(NotFoundMessage(id));
            }

            // An administrator who deletes their own account could leave the application without any administrator.
            if (string.Equals(user.Username, User?.Identity?.Name, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(OwnAccountMessage);
            }

            IdentityResult? result = await _userService.DeleteAsync(id);

            if (result == null)
            {
                return NotFound(NotFoundMessage(id));
            }

            if (!result.Succeeded)
            {
                return BadRequest(Messages(result));
            }

            return Ok(await _userService.GetAllAsync());
        }

        private static string NotFoundMessage(int id)
        {
            return $"Aucun utilisateur avec l'identifiant {id}.";
        }

        private static IEnumerable<string> Messages(IdentityResult result)
        {
            return result.Errors.Select(error => error.Description);
        }
    }
}
