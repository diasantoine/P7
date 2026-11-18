using Dot.Net.WebApi.Domain;
using Dot.Net.WebApi.Repositories;
using Dot.Net.WebApi.Services;
using Moq;
using P7CreateRestApi.Models;
using Xunit;

namespace P7.Tests
{
    public class BidListServiceTests
    {
        private readonly Mock<IDataRepository<BidList>> _repository = new Mock<IDataRepository<BidList>>();
        private readonly BidListService _service;

        public BidListServiceTests()
        {
            _service = new BidListService(_repository.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsOnlyScreenFields()
        {
            _repository.Setup(repository => repository.GetAllAsync()).ReturnsAsync(new List<BidList>
            {
                new BidList { BidListId = 1, Account = "Account A", BidType = "Type A", BidQuantity = 10, Trader = "hidden" }
            });

            List<BidListDto> result = await _service.GetAllAsync();

            BidListDto dto = Assert.Single(result);
            Assert.Equal(1, dto.BidListId);
            Assert.Equal("Account A", dto.Account);
            Assert.Equal("Type A", dto.BidType);
            Assert.Equal(10, dto.BidQuantity);
        }

        [Fact]
        public async Task GetByIdAsync_WithUnknownId_ReturnsNull()
        {
            _repository.Setup(repository => repository.GetByIdAsync(99)).ReturnsAsync((BidList?)null);

            Assert.Null(await _service.GetByIdAsync(99));
        }

        [Fact]
        public async Task CreateAsync_SavesFieldsAndCreationAudit()
        {
            BidList? saved = null;
            _repository.Setup(repository => repository.AddAsync(It.IsAny<BidList>()))
                .Callback<BidList>(bidList => saved = bidList)
                .Returns(Task.CompletedTask);
            DateTime before = DateTime.UtcNow;

            await _service.CreateAsync(
                new BidListDto { Account = "Account A", BidType = "Type A", BidQuantity = 10 },
                "alice");

            Assert.NotNull(saved);
            Assert.Equal("Account A", saved!.Account);
            Assert.Equal("Type A", saved.BidType);
            Assert.Equal(10, saved.BidQuantity);
            Assert.Equal("alice", saved.CreationName);
            Assert.InRange(saved.CreationDate!.Value, before, DateTime.UtcNow);
            Assert.Null(saved.RevisionDate);
        }

        [Fact]
        public async Task UpdateAsync_WithKnownId_ChangesFieldsAndRevisionAudit()
        {
            var existing = new BidList { BidListId = 5, Account = "Old", BidType = "Old", BidQuantity = 1, CreationName = "bob" };
            _repository.Setup(repository => repository.GetByIdAsync(5)).ReturnsAsync(existing);

            bool updated = await _service.UpdateAsync(
                5,
                new BidListDto { Account = "New", BidType = "New type", BidQuantity = 20 },
                "alice");

            Assert.True(updated);
            Assert.Equal("New", existing.Account);
            Assert.Equal("New type", existing.BidType);
            Assert.Equal(20, existing.BidQuantity);
            Assert.Equal("alice", existing.RevisionName);
            Assert.NotNull(existing.RevisionDate);
            Assert.Equal("bob", existing.CreationName);
            _repository.Verify(repository => repository.UpdateAsync(existing), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithUnknownId_ReturnsFalseAndSavesNothing()
        {
            _repository.Setup(repository => repository.GetByIdAsync(99)).ReturnsAsync((BidList?)null);

            bool updated = await _service.UpdateAsync(99, new BidListDto { Account = "A", BidType = "T" }, "alice");

            Assert.False(updated);
            _repository.Verify(repository => repository.UpdateAsync(It.IsAny<BidList>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WithKnownId_DeletesAndReturnsTrue()
        {
            var existing = new BidList { BidListId = 5 };
            _repository.Setup(repository => repository.GetByIdAsync(5)).ReturnsAsync(existing);

            Assert.True(await _service.DeleteAsync(5));
            _repository.Verify(repository => repository.DeleteAsync(existing), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithUnknownId_ReturnsFalse()
        {
            _repository.Setup(repository => repository.GetByIdAsync(99)).ReturnsAsync((BidList?)null);

            Assert.False(await _service.DeleteAsync(99));
            _repository.Verify(repository => repository.DeleteAsync(It.IsAny<BidList>()), Times.Never);
        }
    }
}
