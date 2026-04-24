using Microsoft.AspNetCore.Mvc;
using SIRU.Core.Application.Dtos.Employees;
using SIRU.Core.Application.Dtos.Evaluations;
using SIRU.Core.Application.Interfaces.Evaluations;
using SIRU.Core.Application.Interfaces.Employees;
using SIRU.Core.Domain.Common.Pagination;
using SIRU.Presentation.Api.Handlers;

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
            return result.Handle(HttpContext.Request.Path, dto => Ok(dto));
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
            return result.Handle(HttpContext.Request.Path, dto =>
                CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto));
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
            return result.Handle(HttpContext.Request.Path, dto => Ok(dto));
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _employeeService.DeleteAsync(id);
            return result.Handle(HttpContext.Request.Path, () => NoContent());
        }

        [HttpGet("{id}/history")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmployeeHistoryDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHistory(string id)
        {
            var result = await _employeeService.GetHistoryAsync(id);
            return result.Handle(HttpContext.Request.Path, dtos => Ok(dtos));
        }

        [HttpGet("{id}/evaluations")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EvaluationHistoryDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEvaluations(string id)
        {
            var result = await _evaluationService.GetByEmployeeIdAsync(id);
            return result.Handle(HttpContext.Request.Path, dtos => Ok(dtos));
        }

        [HttpPost("{id}/positions")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(EmployeePositionDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AssignPosition(string id, [FromBody] EmployeePositionInsertDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dto.EmployeeId = id;
            var result = await _employeeService.AssignPositionAsync(dto);
            return result.Handle(HttpContext.Request.Path, positionDto =>
                CreatedAtAction(nameof(GetById), new { id = id }, positionDto));
        }
    }
}