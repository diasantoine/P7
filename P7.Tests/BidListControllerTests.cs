using Dot.Net.WebApi.Controllers;
using Dot.Net.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using P7CreateRestApi.Models;
using Xunit;

namespace P7.Tests
{
    public class BidListControllerTests
    {
        private readonly Mock<IBidListService> _service = new Mock<IBidListService>();
        private readonly BidListController _controller;
        private readonly List<BidListDto> _list = new List<BidListDto>
        {
            new BidListDto { BidListId = 1, Account = "Account A", BidType = "Type A", BidQuantity = 10 }
        };

        public BidListControllerTests()
        {
            _service.Setup(service => service.GetAllAsync()).ReturnsAsync(_list);
            _controller = new BidListController(_service.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsList()
        {
            var ok = Assert.IsType<OkObjectResult>(await _controller.GetAll());

            Assert.Same(_list, ok.Value);
        }

        [Fact]
        public async Task GetById_WithKnownId_ReturnsBid()
        {
            _service.Setup(service => service.GetByIdAsync(1)).ReturnsAsync(_list[0]);

            var ok = Assert.IsType<OkObjectResult>(await _controller.GetById(1));

            Assert.Same(_list[0], ok.Value);
        }

        [Fact]
        public async Task GetById_WithUnknownId_Returns404()
        {
            _service.Setup(service => service.GetByIdAsync(99)).ReturnsAsync((BidListDto?)null);

            Assert.IsType<NotFoundObjectResult>(await _controller.GetById(99));
        }

        [Fact]
        public async Task Create_SavesThenReturnsList()
        {
            var dto = new BidListDto { Account = "Account B", BidType = "Type B", BidQuantity = 5 };

            var ok = Assert.IsType<OkObjectResult>(await _controller.Create(dto));

            Assert.Same(_list, ok.Value);
            _service.Verify(service => service.CreateAsync(dto, It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task Update_WithKnownId_ReturnsList()
        {
            var dto = new BidListDto { Account = "Account B", BidType = "Type B", BidQuantity = 5 };
            _service.Setup(service => service.UpdateAsync(1, dto, It.IsAny<string?>())).ReturnsAsync(true);

            var ok = Assert.IsType<OkObjectResult>(await _controller.Update(1, dto));

            Assert.Same(_list, ok.Value);
        }

        [Fact]
        public async Task Update_WithUnknownId_Returns404()
        {
            var dto = new BidListDto { Account = "Account B", BidType = "Type B" };
            _service.Setup(service => service.UpdateAsync(99, dto, It.IsAny<string?>())).ReturnsAsync(false);

            Assert.IsType<NotFoundObjectResult>(await _controller.Update(99, dto));
        }

        [Fact]
        public async Task Delete_WithKnownId_ReturnsList()
        {
            _service.Setup(service => service.DeleteAsync(1)).ReturnsAsync(true);

            var ok = Assert.IsType<OkObjectResult>(await _controller.Delete(1));

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
