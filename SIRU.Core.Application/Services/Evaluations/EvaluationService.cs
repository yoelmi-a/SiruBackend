using Mapster;
using SIRU.Core.Application.Dtos.Evaluations;
using SIRU.Core.Application.Interfaces.Evaluations;
using SIRU.Core.Domain.Common.Results;
using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;

namespace SIRU.Core.Application.Services.Evaluations;

public class EvaluationService : IEvaluationService
{
    private readonly IGenericRepository<Evaluation> _evaluationRepository;
    private readonly IGenericRepository<Criterion> _criterionRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public EvaluationService(
        IGenericRepository<Evaluation> evaluationRepository,
        IGenericRepository<Criterion> criterionRepository,
        IEmployeeRepository employeeRepository)
    {
        _evaluationRepository = evaluationRepository;
        _criterionRepository = criterionRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<Result<EvaluationDto>> AddAsync(EvaluationInsertDto dto)
    {
        if (dto.Criteria == null || dto.Criteria.Count == 0)
        {
            return Result.Failure<EvaluationDto>(new List<string> { "Debe incluir al menos un criterio." });
        }

        var currentPosition = await _employeeRepository.GetCurrentPositionAsync(dto.EmployeeId);
        if (currentPosition == null)
        {
            return Result.Failure<EvaluationDto>(new List<string> { "Empleado no encontrado o no tiene posición activa." });
        }

        var existingCriteria = await _criterionRepository.FindAsync(
            c => dto.Criteria.Select(d => d.CriterionId).Contains(c.Id));
        var existingCriterionIds = existingCriteria.Select(c => c.Id).ToHashSet();
        var missingIds = dto.Criteria.Select(d => d.CriterionId).Except(existingCriterionIds).ToList();
        if (missingIds.Any())
        {
            return Result.Failure<EvaluationDto>(new List<string> { $"Criterio(s) no encontrado(s): {string.Join(", ", missingIds)}" });
        }

        var evaluation = dto.Adapt<Evaluation>();
        evaluation.EmployeePositionId = currentPosition.Id;
        evaluation.Date = dto.EvaluationDate;

        var criteriaList = dto.Criteria.Select(c => new EvaluationCriterion
        {
            EvaluationId = evaluation.Id,
            CriteriaId = c.CriterionId,
            Score = c.Score,
            Observation = c.Observation
        }).ToList();

        evaluation.Criteria = criteriaList;

        var averageScore = (float)Math.Round(
            evaluation.Criteria!.Average(c => c.Score), 2);
        evaluation.AverageScore = averageScore;

        await _evaluationRepository.AddAsync(evaluation);

        var dtoResult = new EvaluationDto
        {
            Id = evaluation.Id,
            EmployeeId = dto.EmployeeId,
            EmployeeFullName = $"{currentPosition.Employee!.Names} {currentPosition.Employee.LastNames}",
            PositionName = currentPosition.Position!.Name,
            Date = evaluation.Date,
            AverageScore = evaluation.AverageScore,
            Criteria = evaluation.Criteria!.Select(c => new EvaluationDetailDto
            {
                CriterionId = c.CriteriaId,
                CriterionName = existingCriteria.First(e => e.Id == c.CriteriaId).Name,
                Score = c.Score,
                Observation = c.Observation
            }).ToList()
        };

        return Result<EvaluationDto>.Success(dtoResult);
    }

    public async Task<Result<IEnumerable<EvaluationHistoryDto>>> GetByEmployeeIdAsync(string employeeId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
        {
            return Result.Failure<IEnumerable<EvaluationHistoryDto>>(new List<string> { "Empleado no encontrado." });
        }

        var evaluations = await _employeeRepository.GetEmployeeEvaluationsAsync(employeeId);

        var dtos = evaluations.Select(ev =>
        {
            var positionName = ev.EmployeePositionId > 0
                ? (ev.EmployeePosition?.Position?.Name ?? "N/A")
                : "N/A";

            return new EvaluationHistoryDto
            {
                Id = ev.Id,
                Date = ev.Date,
                AverageScore = ev.AverageScore,
                PositionName = positionName,
                Criteria = (ev.Criteria ?? []).Select(c => new EvaluationHistoryCriterionDto
                {
                    Name = c.Criterion?.Name ?? "N/A",
                    Score = c.Score,
                    Observation = c.Observation
                }).ToList()
            };
        }).ToList();
        return Result.Success<IEnumerable<EvaluationHistoryDto>>(dtos);
    }
}