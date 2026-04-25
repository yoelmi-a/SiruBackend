using Mapster;
using SIRU.Core.Application.Dtos.Vacants;
using SIRU.Core.Application.Dtos.Vacancies;
using SIRU.Core.Application.Interfaces.Common;
using SIRU.Core.Application.Interfaces.Vacants;
using SIRU.Core.Application.Services.Common;
using SIRU.Core.Domain.Common.Enums;
using SIRU.Core.Domain.Common.Pagination;
using SIRU.Core.Domain.Common.Results;
using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;

namespace SIRU.Core.Application.Services.Vacants;

public class VacantService : ServiceBase<Vacant, string, VacantDto, SaveVacantDto, UpdateVacantDto>, IVacantService
{
    private readonly ICandidateRepository _candidateRepository;
    private readonly IVacancyCandidateRepository _vacancyCandidateRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IRankingQueue _rankingQueue;

    public VacantService(
        IGenericRepository<Vacant> repository,
        ICandidateRepository candidateRepository,
        IVacancyCandidateRepository vacancyCandidateRepository,
        IFileStorageService fileStorageService,
        IRankingQueue rankingQueue) : base(repository)
    {
        _candidateRepository = candidateRepository;
        _vacancyCandidateRepository = vacancyCandidateRepository;
        _fileStorageService = fileStorageService;
        _rankingQueue = rankingQueue;
    }

    public async Task<Result<VacancyApplicationResultDto>> ApplyToVacancyAsync(string vacantId, VacancyApplicationDto dto)
    {
        var vacant = await _repository.GetByIdAsync(vacantId);
        if (vacant is null)
        {
            return Result.Failure<VacancyApplicationResultDto>(new List<string> { "Vacancy not found." });
        }

        if (vacant.Status != VacantStatus.Open)
        {
            return Result.Failure<VacancyApplicationResultDto>(new List<string> { "Vacancy is not open for applications." });
        }

        if (dto.CvFile is null || dto.CvFile.Length == 0)
        {
            return Result.Failure<VacancyApplicationResultDto>(new List<string> { "CV file is required." });
        }

        var contentType = _fileStorageService.GetContentType(dto.CvFile.FileName);
        if (contentType != "application/pdf")
        {
            return Result.Failure<VacancyApplicationResultDto>(new List<string> { "Only PDF files are accepted." });
        }

        const long MAX_FILE_SIZE = 10 * 1024 * 1024;
        if (dto.CvFile.Length > MAX_FILE_SIZE)
        {
            return Result.Failure<VacancyApplicationResultDto>(new List<string> { "File size must not exceed 10 MB." });
        }

        var candidate = await _candidateRepository.FindByEmailAsync(dto.CandidateEmail);
        if (candidate is null)
        {
            candidate = dto.Adapt<Candidate>();
            candidate.Names = dto.CandidateNames;
            candidate.LastNames = dto.CandidateLastNames;
            candidate.Email = dto.CandidateEmail;
            candidate.PhoneNumber = dto.CandidatePhoneNumber;
            await _candidateRepository.AddAsync(candidate);
        }

        var cvUrl = await _fileStorageService.SaveFileAsync(dto.CvFile, "cvs");

        var vacancyCandidate = dto.Adapt<VacancyCandidate>();
        vacancyCandidate.VacantId = vacantId;
        vacancyCandidate.CandidateId = candidate.Id;
        vacancyCandidate.CvUrl = cvUrl;

        await _vacancyCandidateRepository.AddAsync(vacancyCandidate);

        _ = _rankingQueue.EnqueueAsync(vacancyCandidate.Id);

        var resultDto = new VacancyApplicationResultDto
        {
            ApplicationId = vacancyCandidate.Id,
            VacantId = vacantId,
            CandidateId = candidate.Id,
            CandidateFullName = $"{candidate.Names} {candidate.LastNames}",
            CvUrl = cvUrl,
            Status = CandidateStatus.Pending,
            Score = 0.0f
        };

        return Result<VacancyApplicationResultDto>.Success(resultDto);
    }

    public async Task<Result> SetStatusOfVacancyCandidate(string applicationId, CandidateStatus newStatus)
    {
        var vacancyCandidate = await _vacancyCandidateRepository.GetByIdAsync(applicationId);
        if (vacancyCandidate is null)
        {
            return Result.NotFound("Application not found.");
        }
        if (vacancyCandidate.Status is CandidateStatus.Hired or CandidateStatus.Rejected)
            return Result.BadRequest("Cannot change status of an application that is already finalized.");

        if (vacancyCandidate.Status == CandidateStatus.Pending && newStatus != CandidateStatus.EvaluationProcess)
            return Result.BadRequest("Pending candidates can only move to EvaluationProcess.");

        if (vacancyCandidate.Status == CandidateStatus.EvaluationProcess
            && newStatus is not (CandidateStatus.Hired or CandidateStatus.Rejected))
            return Result.BadRequest("You can only Hire or Reject this candidate now.");
        vacancyCandidate.Status = newStatus;
        await _vacancyCandidateRepository.UpdateAsync(vacancyCandidate);
        return Result.Success();
    }

    public async Task<Result> RecalculateScoresAsync(string vacantId)
    {
        var vacant = await _repository.GetByIdAsync(vacantId);
        if (vacant is null)
        {
            return Result.NotFound("Vacancy not found.");
        }

        var candidates = await _vacancyCandidateRepository.GetAllByVacancyIdAsync(vacantId);
        foreach (var candidate in candidates)
        {
            _ = _rankingQueue.EnqueueAsync(candidate.Id);
        }

        return Result.Success();
    }

    public async Task<Result<PaginatedResponse<VacancyApplicationResultDto>>> GetApplicationsByVacancyAsync(string vacantId, Pagination pagination)
    {
        var vacant = await _repository.GetByIdAsync(vacantId);
        if (vacant is null)
        {
            return Result.NotFound<PaginatedResponse<VacancyApplicationResultDto>>(new List<string> { "Vacancy not found." });
        }

        var paginatedResult = await _vacancyCandidateRepository.GetPaginatedByVacancyIdAsync(vacantId, pagination);
        var mappedItems = paginatedResult.Items.Adapt<List<VacancyApplicationResultDto>>();

        var response = new PaginatedResponse<VacancyApplicationResultDto>
        {
            Items = mappedItems,
            Pagination = paginatedResult.Pagination
        };

        return Result<PaginatedResponse<VacancyApplicationResultDto>>.Success(response);
    }

    protected override async Task<Result<Vacant>> InsertPreProcessing(Vacant entity, SaveVacantDto dto)
    {
        entity.Id = Guid.NewGuid().ToString();
        entity.PublicationDate = DateTime.UtcNow;
        entity.Status = VacantStatus.Open;
        return Result<Vacant>.Success(entity);
    }

    protected override async Task<Result<Vacant>> UpdatePreProcessing(Vacant entity, UpdateVacantDto dto)
    {
        return Result<Vacant>.Success(entity);
    }
}