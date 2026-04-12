using SIRU.Core.Application.Dtos.Position;
using SIRU.Core.Application.Interfaces.Common;
using SIRU.Core.Domain.Entities;

namespace SIRU.Core.Application.Interfaces.Positions
{
    public interface IPositionService : IServiceBase<Position, int, PositionDto, PositionInsertDto, PositionUpdateDto>
    {
    }
}
