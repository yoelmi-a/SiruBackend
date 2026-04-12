using AutoMapper;
using SIRU.Core.Application.Dtos.Position;
using SIRU.Core.Domain.Entities;

namespace SIRU.Core.Application.Mappings.Positions
{
    public class PositionProfile : Profile
    {
        public PositionProfile()
        {
            CreateMap<Position, PositionDto>()
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null))
                .ReverseMap();

            CreateMap<PositionInsertDto, Position>();
            CreateMap<PositionUpdateDto, Position>();
        }
    }
}
