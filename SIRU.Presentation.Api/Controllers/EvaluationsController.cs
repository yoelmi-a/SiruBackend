using Microsoft.AspNetCore.Mvc;
using SIRU.Core.Application.Dtos.Evaluations;
using SIRU.Core.Application.Interfaces.Evaluations;

namespace SIRU.Presentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EvaluationsController : ControllerBase
{
    private readonly IEvaluationService _evaluationService;

    public EvaluationsController(IEvaluationService evaluationService)
    {
        _evaluationService = evaluationService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(EvaluationDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] EvaluationInsertDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _evaluationService.AddAsync(dto);
        if (!result.IsSuccess)
        {
            var errorMessage = result.Errors.FirstOrDefault() ?? "";
            if (errorMessage.Contains("no encontrado") || errorMessage.Contains("posición activa"))
            {
                return NotFound(new { Errors = result.Errors });
            }
            return BadRequest(new { Errors = result.Errors });
        }

        return CreatedAtAction(nameof(Create), new { id = result.Value!.Id }, result.Value);
    }
}