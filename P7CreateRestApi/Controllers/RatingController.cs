using Dot.Net.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Controllers
{
    [ApiController]
    [Route("ratings")]
    public class RatingController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public RatingController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _ratingService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            RatingDto? rating = await _ratingService.GetByIdAsync(id);

            if (rating == null)
            {
                return NotFound(NotFoundMessage(id));
            }

            return Ok(rating);
        }

        // [ApiController] checks the DTO first: an invalid body never reaches this method (400 with the messages).
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RatingDto rating)
        {
            await _ratingService.CreateAsync(rating);

            return Ok(await _ratingService.GetAllAsync());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RatingDto rating)
        {
            if (!await _ratingService.UpdateAsync(id, rating))
            {
                return NotFound(NotFoundMessage(id));
            }

            return Ok(await _ratingService.GetAllAsync());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _ratingService.DeleteAsync(id))
            {
                return NotFound(NotFoundMessage(id));
            }

            return Ok(await _ratingService.GetAllAsync());
        }

        private static string NotFoundMessage(int id)
        {
            return $"Aucune notation avec l'identifiant {id}.";
        }
    }
}
