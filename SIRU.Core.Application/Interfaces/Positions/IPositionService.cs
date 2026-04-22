using SIRU.Core.Application.Dtos.Positions;
using SIRU.Core.Application.Interfaces.Common;
using SIRU.Core.Domain.Entities;

namespace SIRU.Core.Application.Interfaces.Positions
{
    public interface IPositionService : IServiceBase<Position, int, PositionDto, PositionInsertDto, PositionUpdateDto>
    {
    }
}
