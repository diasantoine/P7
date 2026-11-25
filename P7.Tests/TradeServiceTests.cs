using Dot.Net.WebApi.Domain;
using Dot.Net.WebApi.Repositories;
using Dot.Net.WebApi.Services;
using Moq;
using P7CreateRestApi.Models;
using Xunit;

namespace P7.Tests
{
    public class TradeServiceTests
    {
        private readonly Mock<IDataRepository<Trade>> _repository = new Mock<IDataRepository<Trade>>();
        private readonly TradeService _service;

        public TradeServiceTests()
        {
            _service = new TradeService(_repository.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsOnlyScreenFields()
        {
            _repository.Setup(repository => repository.GetAllAsync()).ReturnsAsync(new List<Trade>
            {
                new Trade { TradeId = 1, Account = "Account A", AccountType = "Type A", BuyQuantity = 10, Trader = "hidden" }
            });

            List<TradeDto> result = await _service.GetAllAsync();

            TradeDto dto = Assert.Single(result);
            Assert.Equal(1, dto.TradeId);
            Assert.Equal("Account A", dto.Account);
            Assert.Equal("Type A", dto.AccountType);
            Assert.Equal(10, dto.BuyQuantity);
        }

        [Fact]
        public async Task GetByIdAsync_WithUnknownId_ReturnsNull()
        {
            _repository.Setup(repository => repository.GetByIdAsync(99)).ReturnsAsync((Trade?)null);

            Assert.Null(await _service.GetByIdAsync(99));
        }

        [Fact]
        public async Task CreateAsync_SavesFieldsAndCreationAudit()
        {
            Trade? saved = null;
            _repository.Setup(repository => repository.AddAsync(It.IsAny<Trade>()))
                .Callback<Trade>(trade => saved = trade)
                .Returns(Task.CompletedTask);
            DateTime before = DateTime.UtcNow;

            await _service.CreateAsync(
                new TradeDto { Account = "Account A", AccountType = "Type A", BuyQuantity = 10 },
                "alice");

            Assert.NotNull(saved);
            Assert.Equal("Account A", saved!.Account);
            Assert.Equal("Type A", saved.AccountType);
            Assert.Equal(10, saved.BuyQuantity);
            Assert.Equal("alice", saved.CreationName);
            Assert.InRange(saved.CreationDate!.Value, before, DateTime.UtcNow);
            Assert.Null(saved.RevisionDate);
        }

        [Fact]
        public async Task UpdateAsync_WithKnownId_ChangesFieldsAndRevisionAudit()
        {
            var existing = new Trade { TradeId = 5, Account = "Old", AccountType = "Old", BuyQuantity = 1, CreationName = "bob" };
            _repository.Setup(repository => repository.GetByIdAsync(5)).ReturnsAsync(existing);

            bool updated = await _service.UpdateAsync(
                5,
                new TradeDto { Account = "New", AccountType = "New type", BuyQuantity = 20 },
                "alice");

            Assert.True(updated);
            Assert.Equal("New", existing.Account);
            Assert.Equal("New type", existing.AccountType);
            Assert.Equal(20, existing.BuyQuantity);
            Assert.Equal("alice", existing.RevisionName);
            Assert.NotNull(existing.RevisionDate);
            Assert.Equal("bob", existing.CreationName);
            _repository.Verify(repository => repository.UpdateAsync(existing), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithUnknownId_ReturnsFalseAndSavesNothing()
        {
            _repository.Setup(repository => repository.GetByIdAsync(99)).ReturnsAsync((Trade?)null);

            bool updated = await _service.UpdateAsync(99, new TradeDto { Account = "A", AccountType = "T" }, "alice");

            Assert.False(updated);
            _repository.Verify(repository => repository.UpdateAsync(It.IsAny<Trade>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WithKnownId_DeletesAndReturnsTrue()
        {
            var existing = new Trade { TradeId = 5 };
            _repository.Setup(repository => repository.GetByIdAsync(5)).ReturnsAsync(existing);

            Assert.True(await _service.DeleteAsync(5));
            _repository.Verify(repository => repository.DeleteAsync(existing), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithUnknownId_ReturnsFalse()
        {
            _repository.Setup(repository => repository.GetByIdAsync(99)).ReturnsAsync((Trade?)null);

            Assert.False(await _service.DeleteAsync(99));
            _repository.Verify(repository => repository.DeleteAsync(It.IsAny<Trade>()), Times.Never);
        }
    }
}
