using AutoMapper;
using SIRU.Core.Application.Dtos.Candidate;
using SIRU.Core.Domain.Entities;

namespace SIRU.Core.Application.Mappings.Candidates
{
    public class CandidateProfile : Profile
    {
        public CandidateProfile()
        {
            CreateMap<Candidate, CandidateDto>().ReverseMap();
            CreateMap<CandidateInsertDto, Candidate>();
            CreateMap<CandidateUpdateDto, Candidate>();
        }
    }
}
