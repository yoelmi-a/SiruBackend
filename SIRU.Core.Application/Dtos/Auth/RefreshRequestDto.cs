namespace SIRU.Core.Application.Dtos.Auth
{
    public class RefreshRequestDto
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required string DeviceInfo { get; set; }
    }
}
