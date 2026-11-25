using Dot.Net.WebApi.Domain;
using Dot.Net.WebApi.Repositories;
using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Services
{
    public class RatingService : IRatingService
    {
        private readonly IDataRepository<Rating> _repository;

        public RatingService(IDataRepository<Rating> repository)
        {
            _repository = repository;
        }

        public async Task<List<RatingDto>> GetAllAsync()
        {
            List<Rating> ratings = await _repository.GetAllAsync();

            return ratings.Select(ToDto).ToList();
        }

        public async Task<RatingDto?> GetByIdAsync(int id)
        {
            Rating? rating = await _repository.GetByIdAsync(id);

            return rating == null ? null : ToDto(rating);
        }

        public async Task<RatingDto> CreateAsync(RatingDto dto)
        {
            var rating = new Rating();
            Apply(dto, rating);

            await _repository.AddAsync(rating);

            return ToDto(rating);
        }

        public async Task<bool> UpdateAsync(int id, RatingDto dto)
        {
            Rating? rating = await _repository.GetByIdAsync(id);

            if (rating == null)
            {
                return false;
            }

            Apply(dto, rating);

            await _repository.UpdateAsync(rating);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            Rating? rating = await _repository.GetByIdAsync(id);

            if (rating == null)
            {
                return false;
            }

            await _repository.DeleteAsync(rating);

            return true;
        }

        // The id is never copied from the DTO: it comes from the database or from the route.
        private static void Apply(RatingDto dto, Rating rating)
        {
            rating.MoodysRating = dto.MoodysRating;
            rating.SandPRating = dto.SandPRating;
            rating.FitchRating = dto.FitchRating;
            rating.OrderNumber = dto.OrderNumber;
        }

        private static RatingDto ToDto(Rating rating)
        {
            return new RatingDto
            {
                Id = rating.Id,
                MoodysRating = rating.MoodysRating,
                SandPRating = rating.SandPRating,
                FitchRating = rating.FitchRating,
                OrderNumber = rating.OrderNumber
            };
        }
    }
}
