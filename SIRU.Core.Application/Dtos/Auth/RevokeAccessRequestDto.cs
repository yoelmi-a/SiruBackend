namespace SIRU.Core.Application.Dtos.Auth
{
    public class RevokeAccessRequestDto : RevokeSessionRequest
    {
        public required string ActionMadeByAccountId { get; set; }
    }
}
