using Microsoft.AspNetCore.Mvc;
using SIRU.Core.Application.Dtos.Evaluations;
using SIRU.Core.Application.Interfaces.Evaluations;
using SIRU.Presentation.Api.Handlers;

namespace SIRU.Presentation.Api.Controllers.Evaluations.V1;

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
        return result.Handle(HttpContext.Request.Path, evaluationDto =>
            CreatedAtAction(nameof(Create), new { id = evaluationDto.Id }, evaluationDto));
    }
}