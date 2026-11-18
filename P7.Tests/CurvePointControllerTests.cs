using Dot.Net.WebApi.Controllers;
using Dot.Net.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using P7CreateRestApi.Models;
using Xunit;

namespace P7.Tests
{
    public class CurvePointControllerTests
    {
        private readonly Mock<ICurvePointService> _service = new Mock<ICurvePointService>();
        private readonly CurvePointController _controller;
        private readonly List<CurvePointDto> _list = new List<CurvePointDto>
        {
            new CurvePointDto { Id = 45, CurveId = 12, Term = 14, CurvePointValue = 10 }
        };

        public CurvePointControllerTests()
        {
            _service.Setup(service => service.GetAllAsync()).ReturnsAsync(_list);
            _controller = new CurvePointController(_service.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsList()
        {
            var ok = Assert.IsType<OkObjectResult>(await _controller.GetAll());

            Assert.Same(_list, ok.Value);
        }

        [Fact]
        public async Task GetById_WithKnownId_ReturnsCurvePoint()
        {
            _service.Setup(service => service.GetByIdAsync(45)).ReturnsAsync(_list[0]);

            var ok = Assert.IsType<OkObjectResult>(await _controller.GetById(45));

            Assert.Same(_list[0], ok.Value);
        }

        [Fact]
        public async Task GetById_WithUnknownId_Returns404()
        {
            _service.Setup(service => service.GetByIdAsync(99)).ReturnsAsync((CurvePointDto?)null);

            Assert.IsType<NotFoundObjectResult>(await _controller.GetById(99));
        }

        [Fact]
        public async Task Create_SavesThenReturnsList()
        {
            var dto = new CurvePointDto { CurveId = 3, Term = 2, CurvePointValue = 1 };

            var ok = Assert.IsType<OkObjectResult>(await _controller.Create(dto));

            Assert.Same(_list, ok.Value);
            _service.Verify(service => service.CreateAsync(dto), Times.Once);
        }

        [Fact]
        public async Task Update_WithKnownId_ReturnsList()
        {
            var dto = new CurvePointDto { CurveId = 3, Term = 2, CurvePointValue = 1 };
            _service.Setup(service => service.UpdateAsync(45, dto)).ReturnsAsync(true);

            var ok = Assert.IsType<OkObjectResult>(await _controller.Update(45, dto));

            Assert.Same(_list, ok.Value);
        }

        [Fact]
        public async Task Update_WithUnknownId_Returns404()
        {
            var dto = new CurvePointDto { CurveId = 3 };
            _service.Setup(service => service.UpdateAsync(99, dto)).ReturnsAsync(false);

            Assert.IsType<NotFoundObjectResult>(await _controller.Update(99, dto));
        }

        [Fact]
        public async Task Delete_WithKnownId_ReturnsList()
        {
            _service.Setup(service => service.DeleteAsync(45)).ReturnsAsync(true);

            var ok = Assert.IsType<OkObjectResult>(await _controller.Delete(45));

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
