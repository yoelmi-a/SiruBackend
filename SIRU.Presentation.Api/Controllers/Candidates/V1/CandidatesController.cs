using Microsoft.AspNetCore.Mvc;
using SIRU.Core.Application.Dtos.Candidates;
using SIRU.Core.Application.Interfaces.Candidates;
using SIRU.Presentation.Api.Handlers;

namespace SIRU.Presentation.Api.Controllers.Candidates.V1
{
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

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CandidateDto>))]
        public async Task<IActionResult> GetAll()
        {
            var result = await _candidateService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CandidateDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _candidateService.GetByIdAsync(id);
            return result.Handle(HttpContext.Request.Path, dto => Ok(dto));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CandidateDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CandidateInsertDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _candidateService.AddAsync(dto);
            return result.Handle(HttpContext.Request.Path, dto =>
                CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(string id, [FromBody] CandidateUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _candidateService.UpdateAsync(id, dto);
            return result.Handle(HttpContext.Request.Path, () => NoContent());
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _candidateService.DeleteAsync(id);
            return result.Handle(HttpContext.Request.Path, () => NoContent());
        }
    }
}