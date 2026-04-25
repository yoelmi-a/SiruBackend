using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using SIRU.Core.Application.Dtos.Vacants;
using SIRU.Core.Application.Dtos.Vacancies;
using SIRU.Core.Application.Interfaces.Vacants;
using SIRU.Core.Domain.Common.Pagination;
using SIRU.Presentation.Api.Handlers;
using SIRU.Core.Domain.Common.Enums;

namespace SIRU.Presentation.Api.Controllers.Vacants.V1
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

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<VacantDto>))]
        public async Task<IActionResult> GetAll()
        {
            var result = await _vacantService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VacantDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _vacantService.GetByIdAsync(id);
            return result.Handle(HttpContext.Request.Path, dto => Ok(dto));
        }

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
            return result.Handle(HttpContext.Request.Path, vacantDto =>
                CreatedAtAction(nameof(GetById), new { id = vacantDto.Id }, vacantDto));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateVacantDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _vacantService.UpdateAsync(id, dto);
            return result.Handle(HttpContext.Request.Path, () => NoContent());
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _vacantService.DeleteAsync(id);
            return result.Handle(HttpContext.Request.Path, () => NoContent());
        }

        [HttpPost("{vacancyId}/applications")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(VacancyApplicationResultDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> ApplyToVacancy(string vacancyId, [FromForm] VacancyApplicationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _vacantService.ApplyToVacancyAsync(vacancyId, dto);
            if (!result.IsSuccess)
            {
                var error = result.Errors.FirstOrDefault() ?? "Unknown error";
                if (error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { Errors = result.Errors });
                if (error.Contains("not open", StringComparison.OrdinalIgnoreCase))
                    return Conflict(new { Errors = result.Errors });
                return BadRequest(new { Errors = result.Errors });
            }

            return CreatedAtAction(nameof(GetById), new { id = vacancyId }, result.Value);
        }

        [HttpPost("{vacancyId}/recalculate-scores")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RecalculateScores(string vacancyId)
        {
            var result = await _vacantService.RecalculateScoresAsync(vacancyId);
            return result.Handle(HttpContext.Request.Path, () => Accepted());
        }

        [HttpGet("{vacancyId}/applications")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponse<VacancyApplicationResultDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetApplications(string vacancyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var pagination = new Pagination(page, pageSize);
            var result = await _vacantService.GetApplicationsByVacancyAsync(vacancyId, pagination);
            return result.Handle(HttpContext.Request.Path, response => Ok(response));
        }

        [HttpPut("applications/{applicationId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponse<VacancyApplicationResultDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetApplicationStatus(string applicationId, CandidateStatus newStatus)
        {
            var result = await _vacantService.SetStatusOfVacancyCandidate(applicationId, newStatus);
            return result.Handle(HttpContext.Request.Path, () => Accepted());
        }



    }
}