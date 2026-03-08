using Microsoft.AspNetCore.Mvc;
using SIRU.Core.Application.Dtos.Vacant;
using SIRU.Core.Application.Interfaces.Vacant;

namespace SIRU.Presentation.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class VacantsController : ControllerBase
    {
        private readonly IVacantService _vacantService;

        public VacantsController(IVacantService vacantService)
        {
            _vacantService = vacantService;
        }

        /// <summary>
        /// Obtiene todas las vacantes registradas.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<VacantDto>))]
        public async Task<IActionResult> GetAll()
        {
            var result = await _vacantService.GetAllAsync();
            return Ok(result.Value);
        }

        /// <summary>
        /// Obtiene una vacante específica por su ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VacantDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _vacantService.GetByIdAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound(new { Errors = result.Error });
            }
            return Ok(result.Value);
        }

        /// <summary>
        /// Crea una nueva vacante.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(VacantDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] SaveVacantDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _vacantService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
        }

        /// <summary>
        /// Actualiza una vacante existente.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(string id, [FromBody] SaveVacantDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _vacantService.UpdateAsync(id, dto);
            if (!result.IsSuccess)
            {
                return NotFound(new { Errors = result.Error });
            }
            return NoContent();
        }

        /// <summary>
        /// Elimina una vacante por su ID.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _vacantService.DeleteAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound(new { Errors = result.Error });
            }
            return NoContent();
        }
    }
}
