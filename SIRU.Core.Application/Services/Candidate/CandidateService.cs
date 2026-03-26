using AutoMapper;
using SIRU.Core.Application.Dtos.Candidate;
using SIRU.Core.Application.Interfaces.Candidate;
using SIRU.Core.Application.Services.Generic;
using SIRU.Core.Domain.Interfaces;

namespace SIRU.Core.Application.Services.Candidate
{
    public class CandidateService : GenericService<SIRU.Core.Domain.Entities.Candidate, CandidateDto, string>, ICandidateService
    {
        public CandidateService(
            IGenericRepository<SIRU.Core.Domain.Entities.Candidate> repository,
            IMapper mapper
        ) : base(repository, mapper)
        {
        }
    }
}
