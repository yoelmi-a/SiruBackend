using SIRU.Core.Application.Dtos.Evaluations;
using SIRU.Core.Application.Interfaces.Common;
using SIRU.Core.Domain.Entities;

namespace SIRU.Core.Application.Interfaces.Evaluations;

/// <summary>
/// Service interface for evaluation criteria CRUD operations.
/// </summary>
public interface ICriterionService : IServiceBase<Criterion, int, CriterionDto, CriterionInsertDto, CriterionUpdateDto>
{
}