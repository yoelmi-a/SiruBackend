using Mapster;
using SIRU.Core.Application.Dtos.Evaluations;
using SIRU.Core.Application.Interfaces.Evaluations;
using SIRU.Core.Application.Services.Common;
using SIRU.Core.Domain.Common.Results;
using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;

namespace SIRU.Core.Application.Services.Evaluations;

/// <summary>
/// Service for managing evaluation criteria.
/// </summary>
public class CriterionService : ServiceBase<Criterion, int, CriterionDto, CriterionInsertDto, CriterionUpdateDto>, ICriterionService
{
    private readonly IGenericRepository<Criterion> _criterionRepository;

    public CriterionService(IGenericRepository<Criterion> repository) : base(repository)
    {
        _criterionRepository = repository;
    }

    public override async Task<Result> DeleteAsync(int id)
    {
        var criterion = await _criterionRepository.GetByIdAsync(id);
        if (criterion == null)
        {
            return Result.Failure(new List<string> { "Entity not found." });
        }

        var isInUse = await _criterionRepository.FindAsync(c => c.Evaluations != null && c.Evaluations.Any(e => e.CriteriaId == id));
        if (isInUse.Any())
        {
            return Result.Failure(new List<string> { "No se puede eliminar el criterio porque está siendo utilizado en al menos una evaluación." });
        }

        await _criterionRepository.RemoveAsync(criterion);
        return Result.Success();
    }

    protected override async Task<Result<Criterion>> InsertPreProcessing(Criterion entity, CriterionInsertDto dto)
    {
        var existing = await _criterionRepository.FindAsync(c => c.Name == dto.Name);
        if (existing.Any())
        {
            return Result.Failure<Criterion>(new List<string> { "El nombre del criterio ya está registrado." });
        }
        return Result<Criterion>.Success(entity);
    }

    protected override async Task<Result<Criterion>> UpdatePreProcessing(Criterion entity, CriterionUpdateDto dto)
    {
        var existing = await _criterionRepository.FindAsync(c => c.Name == dto.Name && c.Id != entity.Id);
        if (existing.Any())
        {
            return Result.Failure<Criterion>(new List<string> { "El nombre del criterio ya está registrado." });
        }
        return Result<Criterion>.Success(entity);
    }
}