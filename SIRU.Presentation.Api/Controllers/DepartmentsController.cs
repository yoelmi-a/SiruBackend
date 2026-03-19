using Microsoft.AspNetCore.Mvc;
using SIRU.Core.Application.Dtos.Department;
using SIRU.Core.Application.Interfaces.Departments;

namespace SIRU.Presentation.Api.Controllers
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
            if (!result.IsSuccess)
            {
                return NotFound(new { Errors = result.Error });
            }
            return Ok(result.Value);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(DepartmentDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] DepartmentInsertDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _departmentService.AddAsync(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(new { Errors = result.Error });
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] DepartmentUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _departmentService.UpdateAsync(id, dto);
            if (!result.IsSuccess)
            {
                if (result.Error.Contains("not found"))
                    return NotFound(new { Errors = result.Error });
                
                return BadRequest(new { Errors = result.Error });
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _departmentService.DeleteAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound(new { Errors = result.Error });
            }
            return NoContent();
        }
    }
}
