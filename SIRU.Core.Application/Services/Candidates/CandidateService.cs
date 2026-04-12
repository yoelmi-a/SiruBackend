using AutoMapper;
using SIRU.Core.Application.Dtos.Candidate;
using SIRU.Core.Application.Interfaces.Candidates;
using SIRU.Core.Application.Services.Common;
using SIRU.Core.Domain.Entities;
using SIRU.Core.Domain.Interfaces;

namespace SIRU.Core.Application.Services.Candidates
{
    public class CandidateService : ServiceBase<Candidate, string, CandidateDto, CandidateInsertDto, CandidateUpdateDto>, ICandidateService
    {
        private readonly ICandidateRepository _candidateRepository;

        public CandidateService(ICandidateRepository candidateRepository, IMapper mapper) : base(candidateRepository, mapper)
        {
            _candidateRepository = candidateRepository;
        }
    }
}
