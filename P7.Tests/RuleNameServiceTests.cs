using Dot.Net.WebApi.Domain;
using Dot.Net.WebApi.Repositories;
using Dot.Net.WebApi.Services;
using Moq;
using P7CreateRestApi.Models;
using Xunit;

namespace P7.Tests
{
    public class RuleNameServiceTests
    {
        private readonly Mock<IDataRepository<RuleName>> _repository = new Mock<IDataRepository<RuleName>>();
        private readonly RuleNameService _service;

        public RuleNameServiceTests()
        {
            _service = new RuleNameService(_repository.Object);
        }

        private static RuleNameDto FullDto()
        {
            return new RuleNameDto
            {
                Id = 42,
                Name = "Rule A",
                Description = "Description A",
                Json = "{\"a\":1}",
                Template = "Template A",
                SqlStr = "SELECT 1",
                SqlPart = "WHERE 1 = 1"
            };
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEveryField()
        {
            _repository.Setup(repository => repository.GetAllAsync()).ReturnsAsync(new List<RuleName>
            {
                new RuleName { Id = 3, Name = "Rule A", Description = "D", Json = "J", Template = "T", SqlStr = "S", SqlPart = "P" }
            });

            List<RuleNameDto> result = await _service.GetAllAsync();

            RuleNameDto dto = Assert.Single(result);
            Assert.Equal(3, dto.Id);
            Assert.Equal("Rule A", dto.Name);
            Assert.Equal("D", dto.Description);
            Assert.Equal("J", dto.Json);
            Assert.Equal("T", dto.Template);
            Assert.Equal("S", dto.SqlStr);
            Assert.Equal("P", dto.SqlPart);
        }

        [Fact]
        public async Task GetByIdAsync_WithUnknownId_ReturnsNull()
        {
            _repository.Setup(repository => repository.GetByIdAsync(99)).ReturnsAsync((RuleName?)null);

            Assert.Null(await _service.GetByIdAsync(99));
        }

        [Fact]
        public async Task CreateAsync_SavesFieldsAndIgnoresDtoId()
        {
            RuleName? saved = null;
            _repository.Setup(repository => repository.AddAsync(It.IsAny<RuleName>()))
                .Callback<RuleName>(ruleName => saved = ruleName)
                .Returns(Task.CompletedTask);

            await _service.CreateAsync(FullDto());

            Assert.NotNull(saved);
            Assert.Equal(0, saved!.Id);
            Assert.Equal("Rule A", saved.Name);
            Assert.Equal("Description A", saved.Description);
            Assert.Equal("{\"a\":1}", saved.Json);
            Assert.Equal("Template A", saved.Template);
            Assert.Equal("SELECT 1", saved.SqlStr);
            Assert.Equal("WHERE 1 = 1", saved.SqlPart);
        }

        [Fact]
        public async Task UpdateAsync_WithKnownId_ChangesFieldsAndKeepsId()
        {
            var existing = new RuleName { Id = 5, Name = "Old", Description = "Old", Json = "Old" };
            _repository.Setup(repository => repository.GetByIdAsync(5)).ReturnsAsync(existing);

            bool updated = await _service.UpdateAsync(5, new RuleNameDto { Id = 42, Name = "New", Description = "New description" });

            Assert.True(updated);
            Assert.Equal(5, existing.Id);
            Assert.Equal("New", existing.Name);
            Assert.Equal("New description", existing.Description);
            Assert.Null(existing.Json);
            _repository.Verify(repository => repository.UpdateAsync(existing), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithUnknownId_ReturnsFalseAndSavesNothing()
        {
            _repository.Setup(repository => repository.GetByIdAsync(99)).ReturnsAsync((RuleName?)null);

            Assert.False(await _service.UpdateAsync(99, FullDto()));
            _repository.Verify(repository => repository.UpdateAsync(It.IsAny<RuleName>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WithKnownId_DeletesAndReturnsTrue()
        {
            var existing = new RuleName { Id = 5 };
            _repository.Setup(repository => repository.GetByIdAsync(5)).ReturnsAsync(existing);

            Assert.True(await _service.DeleteAsync(5));
            _repository.Verify(repository => repository.DeleteAsync(existing), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithUnknownId_ReturnsFalse()
        {
            _repository.Setup(repository => repository.GetByIdAsync(99)).ReturnsAsync((RuleName?)null);

            Assert.False(await _service.DeleteAsync(99));
            _repository.Verify(repository => repository.DeleteAsync(It.IsAny<RuleName>()), Times.Never);
        }
    }
}
