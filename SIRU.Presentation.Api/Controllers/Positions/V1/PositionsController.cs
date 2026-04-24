using Microsoft.AspNetCore.Mvc;
using SIRU.Core.Application.Dtos.Positions;
using SIRU.Core.Application.Interfaces.Positions;
using SIRU.Presentation.Api.Handlers;

namespace SIRU.Presentation.Api.Controllers.Positions.V1
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PositionsController : ControllerBase
    {
        private readonly IPositionService _positionService;

        public PositionsController(IPositionService positionService)
        {
            _positionService = positionService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PositionDto>))]
        public async Task<IActionResult> GetAll()
        {
            var result = await _positionService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PositionDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _positionService.GetByIdAsync(id);
            return result.Handle(HttpContext.Request.Path, dto => Ok(dto));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PositionDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] PositionInsertDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _positionService.AddAsync(dto);
            return result.Handle(HttpContext.Request.Path, dto =>
                CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] PositionUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _positionService.UpdateAsync(id, dto);
            return result.Handle(HttpContext.Request.Path, () => NoContent());
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _positionService.DeleteAsync(id);
            return result.Handle(HttpContext.Request.Path, () => NoContent());
        }
    }
}