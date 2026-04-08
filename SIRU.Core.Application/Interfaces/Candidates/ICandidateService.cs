using SIRU.Core.Application.Dtos.Candidate;
using SIRU.Core.Application.Interfaces.Common;
using SIRU.Core.Domain.Entities;

namespace SIRU.Core.Application.Interfaces.Candidates
{
    public interface ICandidateService : IServiceBase<Candidate, string, CandidateDto, CandidateInsertDto, CandidateUpdateDto>
    {
    }
}
