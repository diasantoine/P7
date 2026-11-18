using Dot.Net.WebApi.Controllers;
using Dot.Net.WebApi.Domain;
using Dot.Net.WebApi.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using P7CreateRestApi.Models;
using Xunit;

namespace P7.Tests
{
    public class LoginControllerTests
    {
        private readonly Mock<UserManager<User>> _userManager;
        private readonly Mock<ITokenService> _tokenService;
        private readonly LoginController _controller;

        public LoginControllerTests()
        {
            _userManager = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null!, null!, null!, null!, null!, null!, null!, null!);
            _tokenService = new Mock<ITokenService>();
            _controller = new LoginController(
                _userManager.Object,
                _tokenService.Object,
                NullLogger<LoginController>.Instance);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            var user = new User { Id = 1, UserName = "alice" };
            var roles = new List<string> { Roles.User };
            var expected = new LoginResponse("token", DateTime.UtcNow.AddHours(1));
            _userManager.Setup(manager => manager.FindByNameAsync("alice")).ReturnsAsync(user);
            _userManager.Setup(manager => manager.CheckPasswordAsync(user, "Alice#12345")).ReturnsAsync(true);
            _userManager.Setup(manager => manager.GetRolesAsync(user)).ReturnsAsync(roles);
            _tokenService.Setup(service => service.CreateToken(user, roles)).Returns(expected);

            IActionResult result = await _controller.Login(new LoginModel { Username = "alice", Password = "Alice#12345" });

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            var user = new User { Id = 1, UserName = "alice" };
            _userManager.Setup(manager => manager.FindByNameAsync("alice")).ReturnsAsync(user);
            _userManager.Setup(manager => manager.CheckPasswordAsync(user, "wrong")).ReturnsAsync(false);

            IActionResult result = await _controller.Login(new LoginModel { Username = "alice", Password = "wrong" });

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(LoginController.InvalidCredentialsMessage, unauthorized.Value);
            _tokenService.Verify(service => service.CreateToken(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        [Fact]
        public async Task Login_WithUnknownUser_ReturnsUnauthorized()
        {
            _userManager.Setup(manager => manager.FindByNameAsync("ghost")).ReturnsAsync((User)null!);

            IActionResult result = await _controller.Login(new LoginModel { Username = "ghost", Password = "Ghost#12345" });

            Assert.IsType<UnauthorizedObjectResult>(result);
            _userManager.Verify(manager => manager.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }
    }
}
