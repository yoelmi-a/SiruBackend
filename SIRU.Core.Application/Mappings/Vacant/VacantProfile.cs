using AutoMapper;
using SIRU.Core.Application.Dtos.Vacant;
using DomainEntities = SIRU.Core.Domain.Entities;

namespace SIRU.Core.Application.Mappings.Vacant
{
    public class VacantProfile : Profile
    {
        public VacantProfile()
        {
            CreateMap<DomainEntities.Vacant, VacantDto>().ReverseMap();

            CreateMap<DomainEntities.Vacant, SaveVacantDto>()
                .ReverseMap()
                .ForMember(dest => dest.Candidates, opt => opt.Ignore());
        }
    }
}
