using Dot.Net.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Controllers
{
    [ApiController]
    [Route("curvepoints")]
    public class CurvePointController : ControllerBase
    {
        private readonly ICurvePointService _curvePointService;

        public CurvePointController(ICurvePointService curvePointService)
        {
            _curvePointService = curvePointService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _curvePointService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            CurvePointDto? curvePoint = await _curvePointService.GetByIdAsync(id);

            if (curvePoint == null)
            {
                return NotFound(NotFoundMessage(id));
            }

            return Ok(curvePoint);
        }

        // [ApiController] checks the DTO first: an invalid body never reaches this method (400 with the messages).
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CurvePointDto curvePoint)
        {
            await _curvePointService.CreateAsync(curvePoint);

            return Ok(await _curvePointService.GetAllAsync());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CurvePointDto curvePoint)
        {
            if (!await _curvePointService.UpdateAsync(id, curvePoint))
            {
                return NotFound(NotFoundMessage(id));
            }

            return Ok(await _curvePointService.GetAllAsync());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _curvePointService.DeleteAsync(id))
            {
                return NotFound(NotFoundMessage(id));
            }

            return Ok(await _curvePointService.GetAllAsync());
        }

        private static string NotFoundMessage(int id)
        {
            return $"Aucun point de courbe avec l'identifiant {id}.";
        }
    }
}
