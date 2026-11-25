using Dot.Net.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Controllers
{
    [ApiController]
    [Route("rulenames")]
    public class RuleNameController : ControllerBase
    {
        private readonly IRuleNameService _ruleNameService;

        public RuleNameController(IRuleNameService ruleNameService)
        {
            _ruleNameService = ruleNameService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _ruleNameService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            RuleNameDto? ruleName = await _ruleNameService.GetByIdAsync(id);

            if (ruleName == null)
            {
                return NotFound(NotFoundMessage(id));
            }

            return Ok(ruleName);
        }

        // [ApiController] checks the DTO first: an invalid body never reaches this method (400 with the messages).
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RuleNameDto ruleName)
        {
            await _ruleNameService.CreateAsync(ruleName);

            return Ok(await _ruleNameService.GetAllAsync());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RuleNameDto ruleName)
        {
            if (!await _ruleNameService.UpdateAsync(id, ruleName))
            {
                return NotFound(NotFoundMessage(id));
            }

            return Ok(await _ruleNameService.GetAllAsync());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _ruleNameService.DeleteAsync(id))
            {
                return NotFound(NotFoundMessage(id));
            }

            return Ok(await _ruleNameService.GetAllAsync());
        }

        private static string NotFoundMessage(int id)
        {
            return $"Aucune règle avec l'identifiant {id}.";
        }
    }
}
