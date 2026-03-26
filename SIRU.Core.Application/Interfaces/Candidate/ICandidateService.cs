using SIRU.Core.Application.Interfaces.Generic;
using SIRU.Core.Domain.Entities;
using SIRU.Core.Application.Dtos.Candidate;

namespace SIRU.Core.Application.Interfaces.Candidate
{
    public interface ICandidateService : IGenericService<SIRU.Core.Domain.Entities.Candidate, CandidateDto, string>
    {
    }
}
