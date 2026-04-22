using SIRU.Core.Application.Dtos.Evaluations;
using SIRU.Core.Domain.Common.Results;
using SIRU.Core.Domain.Interfaces;

namespace SIRU.Core.Application.Interfaces.Evaluations;

public interface IEvaluationService
{
    Task<Result<EvaluationDto>> AddAsync(EvaluationInsertDto dto);
}