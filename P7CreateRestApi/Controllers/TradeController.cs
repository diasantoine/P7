using Dot.Net.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Controllers
{
    [ApiController]
    [Route("trades")]
    public class TradeController : ControllerBase
    {
        private readonly ITradeService _tradeService;

        public TradeController(ITradeService tradeService)
        {
            _tradeService = tradeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _tradeService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            TradeDto? trade = await _tradeService.GetByIdAsync(id);

            if (trade == null)
            {
                return NotFound(NotFoundMessage(id));
            }

            return Ok(trade);
        }

        // [ApiController] checks the DTO first: an invalid body never reaches this method (400 with the messages).
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TradeDto trade)
        {
            await _tradeService.CreateAsync(trade, User?.Identity?.Name);

            return Ok(await _tradeService.GetAllAsync());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TradeDto trade)
        {
            if (!await _tradeService.UpdateAsync(id, trade, User?.Identity?.Name))
            {
                return NotFound(NotFoundMessage(id));
            }

            return Ok(await _tradeService.GetAllAsync());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _tradeService.DeleteAsync(id))
            {
                return NotFound(NotFoundMessage(id));
            }

            return Ok(await _tradeService.GetAllAsync());
        }

        private static string NotFoundMessage(int id)
        {
            return $"Aucune transaction avec l'identifiant {id}.";
        }
    }
}
