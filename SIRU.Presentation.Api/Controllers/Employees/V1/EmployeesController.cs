using Microsoft.AspNetCore.Mvc;
using SIRU.Core.Application.Dtos.Employees;
using SIRU.Core.Application.Dtos.Evaluations;
using SIRU.Core.Application.Interfaces.Evaluations;
using SIRU.Core.Application.Interfaces.Employees;
using SIRU.Core.Domain.Common.Pagination;

namespace SIRU.Presentation.Api.Controllers.Employees.V1
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly IEvaluationService _evaluationService;

    public EmployeesController(IEmployeeService employeeService, IEvaluationService evaluationService)
    {
        _employeeService = employeeService;
        _evaluationService = evaluationService;
    }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponse<EmployeeListDto>))]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool? isActive = null)
        {
            var pagination = new Pagination { PageNumber = page, PageSize = pageSize };
            var result = await _employeeService.Paginate(pagination, isActive);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EmployeeDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _employeeService.GetByIdAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound(new { Errors = result.Errors });
            }
            return Ok(result.Value);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(EmployeeDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] EmployeeInsertDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _employeeService.AddAsync(dto);
            if (!result.IsSuccess)
            {
                return Conflict(new { Errors = result.Errors });
            }
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EmployeeDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(string id, [FromBody] EmployeeUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _employeeService.UpdateAsync(id, dto);
            if (!result.IsSuccess)
            {
                if (result.Errors.Contains("no encontrado"))
                {
                    return NotFound(new { Errors = result.Errors });
                }
                return Conflict(new { Errors = result.Errors });
            }
            return Ok(result.Value);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _employeeService.DeleteAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound(new { Errors = result.Errors });
            }
            return NoContent();
        }

        [HttpGet("{id}/history")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmployeeHistoryDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHistory(string id)
        {
            var result = await _employeeService.GetHistoryAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound(new { Errors = result.Errors });
            }
            return Ok(result.Value);
        }

        [HttpGet("{id}/evaluations")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EvaluationHistoryDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEvaluations(string id)
        {
            var result = await _evaluationService.GetByEmployeeIdAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound(new { Errors = result.Errors });
            }
            return Ok(result.Value);
        }
    }
}