using System.Security.Claims;
using Dot.Net.WebApi.Controllers;
using Dot.Net.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using P7CreateRestApi.Models;
using Xunit;

namespace P7.Tests
{
    public class UserControllerTests
    {
        private static readonly IdentityResult Failure = IdentityResult.Failed(
            new IdentityError { Code = "PasswordTooShort", Description = "Too short." });

        private readonly Mock<IUserService> _service = new Mock<IUserService>();
        private readonly UserController _controller;
        private readonly UserInputDto _input = new UserInputDto
        {
            Username = "bob",
            Password = "Trader#12345",
            Fullname = "Bob Trader",
            Role = "User"
        };
        private readonly List<UserDto> _list = new List<UserDto>
        {
            new UserDto { Id = 3, Username = "alice", Fullname = "Alice Admin", Role = "Admin" }
        };

        public UserControllerTests()
        {
            _service.Setup(service => service.GetAllAsync()).ReturnsAsync(_list);
            _controller = new UserController(_service.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsList()
        {
            var ok = Assert.IsType<OkObjectResult>(await _controller.GetAll());

            Assert.Same(_list, ok.Value);
        }

        [Fact]
        public async Task GetById_WithKnownId_ReturnsUser()
        {
            _service.Setup(service => service.GetByIdAsync(3)).ReturnsAsync(_list[0]);

            var ok = Assert.IsType<OkObjectResult>(await _controller.GetById(3));

            Assert.Same(_list[0], ok.Value);
        }

        [Fact]
        public async Task GetById_WithUnknownId_Returns404()
        {
            _service.Setup(service => service.GetByIdAsync(99)).ReturnsAsync((UserDto?)null);

            Assert.IsType<NotFoundObjectResult>(await _controller.GetById(99));
        }

        [Fact]
        public async Task Create_WhenIdentityAccepts_ReturnsList()
        {
            _service.Setup(service => service.CreateAsync(_input)).ReturnsAsync(IdentityResult.Success);

            var ok = Assert.IsType<OkObjectResult>(await _controller.Create(_input));

            Assert.Same(_list, ok.Value);
        }

        [Fact]
        public async Task Create_WhenIdentityRefuses_Returns400WithMessages()
        {
            _service.Setup(service => service.CreateAsync(_input)).ReturnsAsync(Failure);

            var badRequest = Assert.IsType<BadRequestObjectResult>(await _controller.Create(_input));

            Assert.Equal(new[] { "Too short." }, Assert.IsAssignableFrom<IEnumerable<string>>(badRequest.Value));
        }

        [Fact]
        public async Task Update_WithKnownId_ReturnsList()
        {
            _service.Setup(service => service.UpdateAsync(3, _input)).ReturnsAsync(IdentityResult.Success);

            var ok = Assert.IsType<OkObjectResult>(await _controller.Update(3, _input));

            Assert.Same(_list, ok.Value);
        }

        [Fact]
        public async Task Update_WithUnknownId_Returns404()
        {
            _service.Setup(service => service.UpdateAsync(99, _input)).ReturnsAsync((IdentityResult?)null);

            Assert.IsType<NotFoundObjectResult>(await _controller.Update(99, _input));
        }

        [Fact]
        public async Task Update_WhenIdentityRefuses_Returns400()
        {
            _service.Setup(service => service.UpdateAsync(3, _input)).ReturnsAsync(Failure);

            Assert.IsType<BadRequestObjectResult>(await _controller.Update(3, _input));
        }

        [Fact]
        public async Task Delete_WithKnownId_ReturnsList()
        {
            _service.Setup(service => service.GetByIdAsync(3)).ReturnsAsync(_list[0]);
            _service.Setup(service => service.DeleteAsync(3)).ReturnsAsync(IdentityResult.Success);

            var ok = Assert.IsType<OkObjectResult>(await _controller.Delete(3));

            Assert.Same(_list, ok.Value);
        }

        [Fact]
        public async Task Delete_WithUnknownId_Returns404()
        {
            _service.Setup(service => service.GetByIdAsync(99)).ReturnsAsync((UserDto?)null);

            Assert.IsType<NotFoundObjectResult>(await _controller.Delete(99));
            _service.Verify(service => service.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Delete_OwnAccount_Returns400AndDeletesNothing()
        {
            _service.Setup(service => service.GetByIdAsync(3)).ReturnsAsync(_list[0]);
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "ALICE") }, "Test");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var badRequest = Assert.IsType<BadRequestObjectResult>(await _controller.Delete(3));

            Assert.Equal(UserController.OwnAccountMessage, badRequest.Value);
            _service.Verify(service => service.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Delete_WhenIdentityRefuses_Returns400()
        {
            _service.Setup(service => service.GetByIdAsync(3)).ReturnsAsync(_list[0]);
            _service.Setup(service => service.DeleteAsync(3)).ReturnsAsync(Failure);

            Assert.IsType<BadRequestObjectResult>(await _controller.Delete(3));
        }
    }
}
