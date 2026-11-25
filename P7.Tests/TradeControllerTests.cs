using Dot.Net.WebApi.Controllers;
using Dot.Net.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using P7CreateRestApi.Models;
using Xunit;

namespace P7.Tests
{
    public class TradeControllerTests
    {
        private readonly Mock<ITradeService> _service = new Mock<ITradeService>();
        private readonly TradeController _controller;
        private readonly List<TradeDto> _list = new List<TradeDto>
        {
            new TradeDto { TradeId = 1, Account = "Account A", AccountType = "Type A", BuyQuantity = 10 }
        };

        public TradeControllerTests()
        {
            _service.Setup(service => service.GetAllAsync()).ReturnsAsync(_list);
            _controller = new TradeController(_service.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsList()
        {
            var ok = Assert.IsType<OkObjectResult>(await _controller.GetAll());

            Assert.Same(_list, ok.Value);
        }

        [Fact]
        public async Task GetById_WithKnownId_ReturnsTrade()
        {
            _service.Setup(service => service.GetByIdAsync(1)).ReturnsAsync(_list[0]);

            var ok = Assert.IsType<OkObjectResult>(await _controller.GetById(1));

            Assert.Same(_list[0], ok.Value);
        }

        [Fact]
        public async Task GetById_WithUnknownId_Returns404()
        {
            _service.Setup(service => service.GetByIdAsync(99)).ReturnsAsync((TradeDto?)null);

            Assert.IsType<NotFoundObjectResult>(await _controller.GetById(99));
        }

        [Fact]
        public async Task Create_SavesThenReturnsList()
        {
            var dto = new TradeDto { Account = "Account B", AccountType = "Type B", BuyQuantity = 5 };

            var ok = Assert.IsType<OkObjectResult>(await _controller.Create(dto));

            Assert.Same(_list, ok.Value);
            _service.Verify(service => service.CreateAsync(dto, It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task Update_WithKnownId_ReturnsList()
        {
            var dto = new TradeDto { Account = "Account B", AccountType = "Type B", BuyQuantity = 5 };
            _service.Setup(service => service.UpdateAsync(1, dto, It.IsAny<string?>())).ReturnsAsync(true);

            var ok = Assert.IsType<OkObjectResult>(await _controller.Update(1, dto));

            Assert.Same(_list, ok.Value);
        }

        [Fact]
        public async Task Update_WithUnknownId_Returns404()
        {
            var dto = new TradeDto { Account = "Account B", AccountType = "Type B" };
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
