using Mapster;
using SIRU.Core.Application.Dtos.Candidates;
using SIRU.Core.Application.Dtos.Departments;
using SIRU.Core.Application.Dtos.Employees;
using SIRU.Core.Application.Dtos.Evaluations;
using SIRU.Core.Application.Dtos.Positions;
using SIRU.Core.Application.Dtos.Vacants;
using DomainEntities = SIRU.Core.Domain.Entities;
using SIRUEnums = SIRU.Core.Domain.Common.Enums;

namespace SIRU.Core.Application.Mappings;

public static class MappingConfig
{
    public static void RegisterMappings()
    {
        TypeAdapterConfig<DomainEntities.Vacant, VacantDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString());

        TypeAdapterConfig<SaveVacantDto, DomainEntities.Vacant>.NewConfig()
            .Map(dest => dest.Status, src => SIRUEnums.VacantStatus.Open)
            .Map(dest => dest.PublicationDate, src => DateTime.UtcNow);

        TypeAdapterConfig<UpdateVacantDto, DomainEntities.Vacant>.NewConfig()
            .Map(dest => dest.Status, src => EnumParse<SIRUEnums.VacantStatus>(src.Status));

        TypeAdapterConfig<DomainEntities.Candidate, CandidateDto>.NewConfig()
            .Map(dest => dest.Names, src => src.Names)
            .Map(dest => dest.LastNames, src => src.LastNames);

        TypeAdapterConfig<CandidateInsertDto, DomainEntities.Candidate>.NewConfig();
        TypeAdapterConfig<CandidateUpdateDto, DomainEntities.Candidate>.NewConfig();

        TypeAdapterConfig<DomainEntities.Department, DepartmentDto>.NewConfig();
        TypeAdapterConfig<DepartmentInsertDto, DomainEntities.Department>.NewConfig();
        TypeAdapterConfig<DepartmentUpdateDto, DomainEntities.Department>.NewConfig();

        TypeAdapterConfig<DomainEntities.Position, PositionDto>.NewConfig()
            .Map(dest => dest.DepartmentName, src => src.Department != null ? src.Department.Name : null);

        TypeAdapterConfig<PositionInsertDto, DomainEntities.Position>.NewConfig();
        TypeAdapterConfig<PositionUpdateDto, DomainEntities.Position>.NewConfig();

        TypeAdapterConfig<EmployeeInsertDto, DomainEntities.Employee>.NewConfig()
            .Map(dest => dest.Names, src => src.FirstName)
            .Map(dest => dest.LastNames, src => src.LastName)
            .Map(dest => dest.IdCard, src => src.Cedula)
            .Map(dest => dest.Birthdate, src => src.DateOfBirth);

        TypeAdapterConfig<EmployeeUpdateDto, DomainEntities.Employee>.NewConfig()
            .Map(dest => dest.Names, src => src.FirstName)
            .Map(dest => dest.LastNames, src => src.LastName)
            .Map(dest => dest.IdCard, src => src.Cedula)
            .Map(dest => dest.Birthdate, src => src.DateOfBirth);

        TypeAdapterConfig<DomainEntities.Employee, EmployeeDto>.NewConfig()
            .Map(dest => dest.FirstName, src => src.Names)
            .Map(dest => dest.LastName, src => src.LastNames)
            .Map(dest => dest.Cedula, src => src.IdCard)
            .Map(dest => dest.DateOfBirth, src => src.Birthdate)
            .Map(dest => dest.IsActive, src => src.Status);

        TypeAdapterConfig<DomainEntities.Employee, EmployeeListDto>.NewConfig()
            .Map(dest => dest.FullName, src => $"{src.Names} {src.LastNames}")
            .Map(dest => dest.Cedula, src => src.IdCard)
            .Map(dest => dest.IsActive, src => src.Status);

        TypeAdapterConfig<DomainEntities.EmployeePosition, EmployeeHistoryDto>.NewConfig()
            .Map(dest => dest.PositionName, src => src.Position != null ? src.Position.Name : null)
            .Map(dest => dest.DepartmentName, src => src.Position != null && src.Position.Department != null ? src.Position.Department.Name : null);

        TypeAdapterConfig<CriterionInsertDto, DomainEntities.Criterion>.NewConfig();
        TypeAdapterConfig<CriterionUpdateDto, DomainEntities.Criterion>.NewConfig();

        TypeAdapterConfig<EvaluationInsertDto, DomainEntities.Evaluation>.NewConfig()
            .Map(dest => dest.Id, src => Guid.CreateVersion7().ToString());

        TypeAdapterConfig.GlobalSettings.Compile();
    }

    private static T EnumParse<T>(string value) where T : struct
    {
        return Enum.TryParse<T>(value, true, out var result) ? result : default;
    }
}