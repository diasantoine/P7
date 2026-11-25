using Dot.Net.WebApi.Controllers;
using Dot.Net.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using P7CreateRestApi.Models;
using Xunit;

namespace P7.Tests
{
    public class RuleNameControllerTests
    {
        private readonly Mock<IRuleNameService> _service = new Mock<IRuleNameService>();
        private readonly RuleNameController _controller;
        private readonly List<RuleNameDto> _list = new List<RuleNameDto>
        {
            new RuleNameDto { Id = 3, Name = "Rule A", Description = "Description A" }
        };

        public RuleNameControllerTests()
        {
            _service.Setup(service => service.GetAllAsync()).ReturnsAsync(_list);
            _controller = new RuleNameController(_service.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsList()
        {
            var ok = Assert.IsType<OkObjectResult>(await _controller.GetAll());

            Assert.Same(_list, ok.Value);
        }

        [Fact]
        public async Task GetById_WithKnownId_ReturnsRule()
        {
            _service.Setup(service => service.GetByIdAsync(3)).ReturnsAsync(_list[0]);

            var ok = Assert.IsType<OkObjectResult>(await _controller.GetById(3));

            Assert.Same(_list[0], ok.Value);
        }

        [Fact]
        public async Task GetById_WithUnknownId_Returns404()
        {
            _service.Setup(service => service.GetByIdAsync(99)).ReturnsAsync((RuleNameDto?)null);

            Assert.IsType<NotFoundObjectResult>(await _controller.GetById(99));
        }

        [Fact]
        public async Task Create_SavesThenReturnsList()
        {
            var dto = new RuleNameDto { Name = "Rule B" };

            var ok = Assert.IsType<OkObjectResult>(await _controller.Create(dto));

            Assert.Same(_list, ok.Value);
            _service.Verify(service => service.CreateAsync(dto), Times.Once);
        }

        [Fact]
        public async Task Update_WithKnownId_ReturnsList()
        {
            var dto = new RuleNameDto { Name = "Rule B" };
            _service.Setup(service => service.UpdateAsync(3, dto)).ReturnsAsync(true);

            var ok = Assert.IsType<OkObjectResult>(await _controller.Update(3, dto));

            Assert.Same(_list, ok.Value);
        }

        [Fact]
        public async Task Update_WithUnknownId_Returns404()
        {
            var dto = new RuleNameDto { Name = "Rule B" };
            _service.Setup(service => service.UpdateAsync(99, dto)).ReturnsAsync(false);

            Assert.IsType<NotFoundObjectResult>(await _controller.Update(99, dto));
        }

        [Fact]
        public async Task Delete_WithKnownId_ReturnsList()
        {
            _service.Setup(service => service.DeleteAsync(3)).ReturnsAsync(true);

            var ok = Assert.IsType<OkObjectResult>(await _controller.Delete(3));

            Assert.Same(_list, ok.Value);
        }

        [Fact]
        public async Task Delete_WithUnknownId_Returns404()
        {
            _service.Setup(service => service.DeleteAsync(99)).ReturnsAsync(false);

            Assert.IsType<NotFoundObjectResult>(await _controller.Delete(99));
        }
    }
}
