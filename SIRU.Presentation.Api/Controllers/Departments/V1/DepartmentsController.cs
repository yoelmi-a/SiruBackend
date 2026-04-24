using Microsoft.AspNetCore.Mvc;
using SIRU.Core.Application.Dtos.Departments;
using SIRU.Core.Application.Interfaces.Departments;
using SIRU.Presentation.Api.Handlers;

namespace SIRU.Presentation.Api.Controllers.Departments.V1
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DepartmentDto>))]
        public async Task<IActionResult> GetAll()
        {
            var result = await _departmentService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DepartmentDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _departmentService.GetByIdAsync(id);
            return result.Handle(HttpContext.Request.Path, dto => Ok(dto));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(DepartmentDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] DepartmentInsertDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _departmentService.AddAsync(dto);
            return result.Handle(HttpContext.Request.Path, dto =>
                CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] DepartmentUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _departmentService.UpdateAsync(id, dto);
            return result.Handle(HttpContext.Request.Path, () => NoContent());
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _departmentService.DeleteAsync(id);
            return result.Handle(HttpContext.Request.Path, () => NoContent());
        }
    }
}