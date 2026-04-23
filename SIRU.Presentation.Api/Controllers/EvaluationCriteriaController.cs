using Microsoft.AspNetCore.Mvc;
using SIRU.Core.Application.Dtos.Evaluations;
using SIRU.Core.Application.Interfaces.Evaluations;

namespace SIRU.Presentation.Api.Controllers;

/// <summary>
/// Controller for managing evaluation criteria.
/// </summary>
[ApiController]
[Route("api/evaluation-criteria")]
[Produces("application/json")]
public class EvaluationCriteriaController : ControllerBase
{
    private readonly ICriterionService _criterionService;

    public EvaluationCriteriaController(ICriterionService criterionService)
    {
        _criterionService = criterionService;
    }

    /// <summary>
    /// Retrieves all evaluation criteria.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CriterionDto>))]
    public async Task<IActionResult> GetAll()
    {
        var result = await _criterionService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single evaluation criterion by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CriterionDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _criterionService.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            return NotFound(new { Errors = result.Errors });
        }
        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new evaluation criterion.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CriterionDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CriterionInsertDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _criterionService.AddAsync(dto);
        if (!result.IsSuccess)
        {
            if (result.Errors.Any(e => e.Contains("ya está registrado")))
            {
                return Conflict(new { Errors = result.Errors });
            }
            return BadRequest(new { Errors = result.Errors });
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Updates an existing evaluation criterion.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CriterionDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] CriterionUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _criterionService.UpdateAsync(id, dto);
        if (!result.IsSuccess)
        {
            if (result.Errors.Any(e => e.Contains("not found")))
            {
                return NotFound(new { Errors = result.Errors });
            }
            if (result.Errors.Any(e => e.Contains("ya está registrado")))
            {
                return Conflict(new { Errors = result.Errors });
            }
            return BadRequest(new { Errors = result.Errors });
        }
        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes an evaluation criterion.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _criterionService.DeleteAsync(id);
        if (!result.IsSuccess)
        {
            if (result.Errors.Any(e => e.Contains("not found")))
            {
                return NotFound(new { Errors = result.Errors });
            }
            if (result.Errors.Any(e => e.Contains("eliminar")))
            {
                return Conflict(new { Errors = result.Errors });
            }
            return BadRequest(new { Errors = result.Errors });
        }
        return NoContent();
    }
}