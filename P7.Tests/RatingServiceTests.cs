using Dot.Net.WebApi.Domain;
using Dot.Net.WebApi.Repositories;
using Dot.Net.WebApi.Services;
using Moq;
using P7CreateRestApi.Models;
using Xunit;

namespace P7.Tests
{
    public class RatingServiceTests
    {
        private readonly Mock<IDataRepository<Rating>> _repository = new Mock<IDataRepository<Rating>>();
        private readonly RatingService _service;

        public RatingServiceTests()
        {
            _service = new RatingService(_repository.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEveryField()
        {
            _repository.Setup(repository => repository.GetAllAsync()).ReturnsAsync(new List<Rating>
            {
                new Rating { Id = 3, MoodysRating = "Aaa", SandPRating = "AAA", FitchRating = "AA+", OrderNumber = 1 }
            });

            List<RatingDto> result = await _service.GetAllAsync();

            RatingDto dto = Assert.Single(result);
            Assert.Equal(3, dto.Id);
            Assert.Equal("Aaa", dto.MoodysRating);
            Assert.Equal("AAA", dto.SandPRating);
            Assert.Equal("AA+", dto.FitchRating);
            Assert.Equal((byte)1, dto.OrderNumber);
        }

        [Fact]
        public async Task GetByIdAsync_WithUnknownId_ReturnsNull()
        {
            _repository.Setup(repository => repository.GetByIdAsync(99)).ReturnsAsync((Rating?)null);

            Assert.Null(await _service.GetByIdAsync(99));
        }

        [Fact]
        public async Task CreateAsync_SavesFieldsAndIgnoresDtoId()
        {
            Rating? saved = null;
            _repository.Setup(repository => repository.AddAsync(It.IsAny<Rating>()))
                .Callback<Rating>(rating => saved = rating)
                .Returns(Task.CompletedTask);

            await _service.CreateAsync(
                new RatingDto { Id = 42, MoodysRating = "Aaa", SandPRating = "AAA", FitchRating = "AA+", OrderNumber = 1 });

            Assert.NotNull(saved);
            Assert.Equal(0, saved!.Id);
            Assert.Equal("Aaa", saved.MoodysRating);
            Assert.Equal("AAA", saved.SandPRating);
            Assert.Equal("AA+", saved.FitchRating);
            Assert.Equal((byte)1, saved.OrderNumber);
        }

        [Fact]
        public async Task UpdateAsync_WithKnownId_ChangesFieldsAndKeepsId()
        {
            var existing = new Rating { Id = 5, MoodysRating = "B", SandPRating = "B", FitchRating = "B", OrderNumber = 9 };
            _repository.Setup(repository => repository.GetByIdAsync(5)).ReturnsAsync(existing);

            bool updated = await _service.UpdateAsync(
                5,
                new RatingDto { Id = 42, MoodysRating = "Aaa", SandPRating = "AAA", FitchRating = "AA+", OrderNumber = 1 });

            Assert.True(updated);
            Assert.Equal(5, existing.Id);
            Assert.Equal("Aaa", existing.MoodysRating);
            Assert.Equal("AAA", existing.SandPRating);
            Assert.Equal("AA+", existing.FitchRating);
            Assert.Equal((byte)1, existing.OrderNumber);
            _repository.Verify(repository => repository.UpdateAsync(existing), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithUnknownId_ReturnsFalseAndSavesNothing()
        {
            _repository.Setup(repository => repository.GetByIdAsync(99)).ReturnsAsync((Rating?)null);

            Assert.False(await _service.UpdateAsync(99, new RatingDto { MoodysRating = "A", SandPRating = "A", FitchRating = "A", OrderNumber = 1 }));
            _repository.Verify(repository => repository.UpdateAsync(It.IsAny<Rating>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WithKnownId_DeletesAndReturnsTrue()
        {
            var existing = new Rating { Id = 5 };
            _repository.Setup(repository => repository.GetByIdAsync(5)).ReturnsAsync(existing);

            Assert.True(await _service.DeleteAsync(5));
            _repository.Verify(repository => repository.DeleteAsync(existing), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithUnknownId_ReturnsFalse()
        {
            _repository.Setup(repository => repository.GetByIdAsync(99)).ReturnsAsync((Rating?)null);

            Assert.False(await _service.DeleteAsync(99));
            _repository.Verify(repository => repository.DeleteAsync(It.IsAny<Rating>()), Times.Never);
        }
    }
}
