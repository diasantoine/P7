using Dot.Net.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Controllers
{
    [ApiController]
    [Route("bidlists")]
    public class BidListController : ControllerBase
    {
        private readonly IBidListService _bidListService;

        public BidListController(IBidListService bidListService)
        {
            _bidListService = bidListService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _bidListService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            BidListDto? bidList = await _bidListService.GetByIdAsync(id);

            if (bidList == null)
            {
                return NotFound(NotFoundMessage(id));
            }

            return Ok(bidList);
        }

        // [ApiController] checks the DTO first: an invalid body never reaches this method (400 with the messages).
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BidListDto bidList)
        {
            await _bidListService.CreateAsync(bidList, User?.Identity?.Name);

            return Ok(await _bidListService.GetAllAsync());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BidListDto bidList)
        {
            if (!await _bidListService.UpdateAsync(id, bidList, User?.Identity?.Name))
            {
                return NotFound(NotFoundMessage(id));
            }

            return Ok(await _bidListService.GetAllAsync());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _bidListService.DeleteAsync(id))
            {
                return NotFound(NotFoundMessage(id));
            }

            return Ok(await _bidListService.GetAllAsync());
        }

        private static string NotFoundMessage(int id)
        {
            return $"Aucune offre avec l'identifiant {id}.";
        }
    }
}
