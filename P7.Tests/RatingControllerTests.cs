using Dot.Net.WebApi.Controllers;
using Dot.Net.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using P7CreateRestApi.Models;
using Xunit;

namespace P7.Tests
{
    public class RatingControllerTests
    {
        private readonly Mock<IRatingService> _service = new Mock<IRatingService>();
        private readonly RatingController _controller;
        private readonly List<RatingDto> _list = new List<RatingDto>
        {
            new RatingDto { Id = 3, MoodysRating = "Aaa", SandPRating = "AAA", FitchRating = "AA+", OrderNumber = 1 }
        };

        public RatingControllerTests()
        {
            _service.Setup(service => service.GetAllAsync()).ReturnsAsync(_list);
            _controller = new RatingController(_service.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsList()
        {
            var ok = Assert.IsType<OkObjectResult>(await _controller.GetAll());

            Assert.Same(_list, ok.Value);
        }

        [Fact]
        public async Task GetById_WithKnownId_ReturnsRating()
        {
            _service.Setup(service => service.GetByIdAsync(3)).ReturnsAsync(_list[0]);

            var ok = Assert.IsType<OkObjectResult>(await _controller.GetById(3));

            Assert.Same(_list[0], ok.Value);
        }

        [Fact]
        public async Task GetById_WithUnknownId_Returns404()
        {
            _service.Setup(service => service.GetByIdAsync(99)).ReturnsAsync((RatingDto?)null);

            Assert.IsType<NotFoundObjectResult>(await _controller.GetById(99));
        }

        [Fact]
        public async Task Create_SavesThenReturnsList()
        {
            var dto = new RatingDto { MoodysRating = "A1", SandPRating = "A+", FitchRating = "A", OrderNumber = 2 };

            var ok = Assert.IsType<OkObjectResult>(await _controller.Create(dto));

            Assert.Same(_list, ok.Value);
            _service.Verify(service => service.CreateAsync(dto), Times.Once);
        }

        [Fact]
        public async Task Update_WithKnownId_ReturnsList()
        {
            var dto = new RatingDto { MoodysRating = "A1", SandPRating = "A+", FitchRating = "A", OrderNumber = 2 };
            _service.Setup(service => service.UpdateAsync(3, dto)).ReturnsAsync(true);

            var ok = Assert.IsType<OkObjectResult>(await _controller.Update(3, dto));

            Assert.Same(_list, ok.Value);
        }

        [Fact]
        public async Task Update_WithUnknownId_Returns404()
        {
            var dto = new RatingDto { MoodysRating = "A1", SandPRating = "A+", FitchRating = "A", OrderNumber = 2 };
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
