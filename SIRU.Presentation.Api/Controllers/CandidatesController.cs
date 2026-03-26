using Microsoft.AspNetCore.Mvc;
using SIRU.Core.Application.Dtos.Candidate;
using SIRU.Core.Application.Interfaces.Candidate;
using SIRU.Core.Application.Interfaces.Generic;
using SIRU.Core.Application.Services.Candidate;

namespace SIRU.Presentation.Api.Controllers
{
    /// <summary>
    /// Controlador para gestionar candidatos.
    /// Proporciona endpoints para crear, consultar, actualizar y eliminar candidatos.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CandidatesController : ControllerBase
    {
        private readonly ICandidateService _candidateService;

        public CandidatesController(ICandidateService candidateService)
        {
            _candidateService = candidateService;
        }
        /// </remarks>
        /// <returns>Una lista de todos los candidatos con sus datos completos.</returns>
        /// <response code="200">Lista de candidatos obtenida exitosamente.</response>
        /// <response code="500">Error interno del servidor al recuperar candidatos.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CandidateDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _candidateService.GetAllAsync();
            return Ok(result.Value);
        }
        /// <returns>Los detalles del candidato solicitado.</returns>
        /// <response code="200">Candidato encontrado y retornado exitosamente.</response>
        /// <response code="404">No se encontró un candidato con el ID especificado.</response>
        /// <response code="400">El ID proporcionado es inválido.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CandidateDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _candidateService.GetByIdAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound(new { Errors = result.Error });
            }
            return Ok(result.Value);
        }
        /// <returns>El candidato creado incluyendo su ID asignado automáticamente.</returns>
        /// <response code="201">Candidato creado exitosamente.</response>
        /// <response code="400">Los datos proporcionados no son válidos (ej: email duplicado, falta CV).</response>
        /// <response code="500">Error interno del servidor al crear el candidato.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CandidateDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CandidateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _candidateService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Value?.Id }, result.Value);
        }
        /// <returns>Un mensaje de confirmación de la actualización.</returns>
        /// <response code="200">Candidato actualizado exitosamente.</response>
        /// <response code="404">No se encontró un candidato con el ID especificado.</response>
        /// <response code="400">Los datos proporcionados no son válidos.</response>
        /// <response code="500">Error interno del servidor al actualizar el candidato.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(string id, [FromBody] CandidateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _candidateService.UpdateAsync(id, dto);
            if (!result.IsSuccess)
            {
                return NotFound(new { Errors = result.Error });
            }
            return Ok(new { Message = "Candidato actualizado exitosamente" });
        }
        /// <returns>Un mensaje de confirmación de la eliminación.</returns>
        /// <response code="200">Candidato eliminado exitosamente.</response>
        /// <response code="404">No se encontró un candidato con el ID especificado.</response>
        /// <response code="400">El ID proporcionado es inválido.</response>
        /// <response code="500">Error interno del servidor al eliminar el candidato.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _candidateService.DeleteAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound(new { Errors = result.Error });
            }
            return Ok(new { Message = "Candidato eliminado exitosamente" });
        }
    }
}