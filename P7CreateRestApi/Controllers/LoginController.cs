using Dot.Net.WebApi.Domain;
using Dot.Net.WebApi.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Controllers
{
    [ApiController]
    [Route("login")]
    public class LoginController : ControllerBase
    {
        public const string InvalidCredentialsMessage = "Nom d'utilisateur ou mot de passe incorrect.";

        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(
            UserManager<User> userManager,
            ITokenService tokenService,
            ILogger<LoginController> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            User? user = await _userManager.FindByNameAsync(model.Username);

            // Same answer for an unknown user and a wrong password: do not reveal which accounts exist.
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                _logger.LogWarning("Failed login attempt for {Username}", model.Username);
                return Unauthorized(InvalidCredentialsMessage);
            }

            IList<string> roles = await _userManager.GetRolesAsync(user);

            return Ok(_tokenService.CreateToken(user, roles));
        }
    }
}
