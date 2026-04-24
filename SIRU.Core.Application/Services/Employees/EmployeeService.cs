using Mapster;
using SIRU.Core.Application.Dtos.Employees;
using SIRU.Core.Application.Interfaces.Employees;
using SIRU.Core.Application.Services.Common;
using SIRU.Core.Domain.Common.Pagination;
using SIRU.Core.Domain.Common.Results;
using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;

namespace SIRU.Core.Application.Services.Employees
{
    public class EmployeeService : ServiceBase<Employee, string, EmployeeDto, EmployeeInsertDto, EmployeeUpdateDto>, IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPositionRepository _positionRepository;

        public EmployeeService(IGenericRepository<Employee> repository, IEmployeeRepository employeeRepository, IPositionRepository positionRepository) : base(repository)
        {
            _employeeRepository = employeeRepository;
            _positionRepository = positionRepository;
        }

        public async Task<Result<IEnumerable<EmployeeHistoryDto>>> GetHistoryAsync(string employeeId)
        {
            var employee = await _repository.GetByIdAsync(employeeId);
            if (employee == null)
            {
                return Result.NotFound<IEnumerable<EmployeeHistoryDto>>(new List<string> { "Entity not found." });
            }

            var history = await _employeeRepository.GetEmployeeHistoryWithDetailsAsync(employeeId);
            var dtos = history.Adapt<IEnumerable<EmployeeHistoryDto>>();
            return Result<IEnumerable<EmployeeHistoryDto>>.Success(dtos);
        }

        public async Task<Result<IEnumerable<EmployeeListDto>>> GetAllAsync(bool? isActive = null)
        {
            var employees = isActive.HasValue
                ? await _repository.FindAsync(e => e.Status == isActive.Value)
                : await _repository.GetAllAsync();

            var dtos = employees.Adapt<IEnumerable<EmployeeListDto>>();
            return Result<IEnumerable<EmployeeListDto>>.Success(dtos);
        }

        public async Task<PaginatedResponse<EmployeeListDto>> Paginate(Pagination pagination, bool? isActive = null)
        {
            var paginatedEmployees = isActive.HasValue
                ? await _repository.PaginateWhere(pagination, e => e.Status == isActive.Value)
                : await _repository.Paginate(pagination);

            pagination.TotalCount = paginatedEmployees.Items.Count();
            var paginatedDtos = new PaginatedResponse<EmployeeListDto>
            {
                Items = paginatedEmployees.Items.Select(e => e.Adapt<EmployeeListDto>()),
                Pagination = paginatedEmployees.Pagination
            };
            return paginatedDtos;
        }

        protected override async Task<Result<Employee>> InsertPreProcessing(Employee entity, EmployeeInsertDto dto)
        {
            var existingCedula = await _repository.FindAsync(e => e.IdCard == dto.Cedula);
            if (existingCedula.Any())
            {
                return Result.Conflict<Employee>(new List<string> { "La cédula ya está registrada" });
            }
            entity.Status = true;
            return Result.Success<Employee>(entity);
        }

        protected override async Task<Result<Employee>> UpdatePreProcessing(Employee entity, EmployeeUpdateDto dto)
        {
            var existingCedula = await _repository.FindAsync(e => e.IdCard == dto.Cedula && e.Id != entity.Id);
            if (existingCedula.Any())
            {
                return Result.Conflict<Employee>(new List<string> { "La cédula ya está registrada" });
            }
            return Result.Success<Employee>(entity);
        }

        public async Task<Result<EmployeePositionDto>> AssignPositionAsync(EmployeePositionInsertDto dto)
        {
            var employee = await _repository.GetByIdAsync(dto.EmployeeId);
            if (employee == null)
            {
                return Result.NotFound<EmployeePositionDto>(new List<string> { "Empleado no encontrado." });
            }

            var position = await _positionRepository.GetByIdWithDepartmentAsync(dto.PositionId);
            if (position == null)
            {
                return Result.NotFound<EmployeePositionDto>(new List<string> { "Cargo no encontrado." });
            }

            var currentPosition = await _employeeRepository.GetCurrentPositionAsync(dto.EmployeeId);
            if (currentPosition != null && currentPosition.PositionId == dto.PositionId)
            {
                return Result.Conflict<EmployeePositionDto>(new List<string> { "El empleado ya tiene este cargo asignado." });
            }

            var employeePosition = new EmployeePosition
            {
                Id = 0,
                EmployeeId = dto.EmployeeId,
                PositionId = dto.PositionId,
                StartDate = dto.StartDate
            };

            await _employeeRepository.AddEmployeePositionAsync(employeePosition);

            var resultDto = employeePosition.Adapt<EmployeePositionDto>();
            return Result<EmployeePositionDto>.Success(resultDto);
        }
    }
}