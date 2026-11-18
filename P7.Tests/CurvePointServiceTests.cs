using Dot.Net.WebApi.Domain;
using Dot.Net.WebApi.Repositories;
using Dot.Net.WebApi.Services;
using Moq;
using P7CreateRestApi.Models;
using Xunit;

namespace P7.Tests
{
    public class CurvePointServiceTests
    {
        private readonly Mock<IDataRepository<CurvePoint>> _repository = new Mock<IDataRepository<CurvePoint>>();
        private readonly CurvePointService _service;

        public CurvePointServiceTests()
        {
            _service = new CurvePointService(_repository.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsScreenFields()
        {
            _repository.Setup(repository => repository.GetAllAsync()).ReturnsAsync(new List<CurvePoint>
            {
                new CurvePoint { Id = 45, CurveId = 12, Term = 14, CurvePointValue = 10 }
            });

            List<CurvePointDto> result = await _service.GetAllAsync();

            CurvePointDto dto = Assert.Single(result);
            Assert.Equal(45, dto.Id);
            Assert.Equal((byte)12, dto.CurveId);
            Assert.Equal(14, dto.Term);
            Assert.Equal(10, dto.CurvePointValue);
        }

        [Fact]
        public async Task GetByIdAsync_WithUnknownId_ReturnsNull()
        {
            _repository.Setup(repository => repository.GetByIdAsync(99)).ReturnsAsync((CurvePoint?)null);

            Assert.Null(await _service.GetByIdAsync(99));
        }

        [Fact]
        public async Task CreateAsync_SavesFieldsAndDates()
        {
            CurvePoint? saved = null;
            _repository.Setup(repository => repository.AddAsync(It.IsAny<CurvePoint>()))
                .Callback<CurvePoint>(curvePoint => saved = curvePoint)
                .Returns(Task.CompletedTask);
            DateTime before = DateTime.UtcNow;

            await _service.CreateAsync(new CurvePointDto { CurveId = 12, Term = 14, CurvePointValue = 10 });

            Assert.NotNull(saved);
            Assert.Equal((byte)12, saved!.CurveId);
            Assert.Equal(14, saved.Term);
            Assert.Equal(10, saved.CurvePointValue);
            Assert.InRange(saved.CreationDate!.Value, before, DateTime.UtcNow);
            Assert.Equal(saved.CreationDate, saved.AsOfDate);
        }

        [Fact]
        public async Task UpdateAsync_WithKnownId_ChangesFieldsAndKeepsCreationDate()
        {
            DateTime created = DateTime.UtcNow.AddDays(-3);
            var existing = new CurvePoint { Id = 5, CurveId = 1, Term = 1, CurvePointValue = 1, CreationDate = created, AsOfDate = created };
            _repository.Setup(repository => repository.GetByIdAsync(5)).ReturnsAsync(existing);

            bool updated = await _service.UpdateAsync(5, new CurvePointDto { CurveId = 12, Term = 14, CurvePointValue = 10 });

            Assert.True(updated);
            Assert.Equal((byte)12, existing.CurveId);
            Assert.Equal(14, existing.Term);
            Assert.Equal(10, existing.CurvePointValue);
            Assert.Equal(created, existing.CreationDate);
            Assert.True(existing.AsOfDate > created);
            _repository.Verify(repository => repository.UpdateAsync(existing), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithUnknownId_ReturnsFalseAndSavesNothing()
        {
            _repository.Setup(repository => repository.GetByIdAsync(99)).ReturnsAsync((CurvePoint?)null);

            Assert.False(await _service.UpdateAsync(99, new CurvePointDto { CurveId = 1 }));
            _repository.Verify(repository => repository.UpdateAsync(It.IsAny<CurvePoint>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WithKnownId_DeletesAndReturnsTrue()
        {
            var existing = new CurvePoint { Id = 5 };
            _repository.Setup(repository => repository.GetByIdAsync(5)).ReturnsAsync(existing);

            Assert.True(await _service.DeleteAsync(5));
            _repository.Verify(repository => repository.DeleteAsync(existing), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithUnknownId_ReturnsFalse()
        {
            _repository.Setup(repository => repository.GetByIdAsync(99)).ReturnsAsync((CurvePoint?)null);

            Assert.False(await _service.DeleteAsync(99));
            _repository.Verify(repository => repository.DeleteAsync(It.IsAny<CurvePoint>()), Times.Never);
        }
    }
}
