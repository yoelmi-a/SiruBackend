namespace SIRU.Infrastructure.Identity.Entities
{
    public class UserSession
    {
        public required string Id { get; set; }
        public required string RefreshTokenHash { get; set; }
        public required string AccountId { get; set; }
        public required string DeviceInfo { get; set; }
        public required DateTime ExpiresAt { get; set; }
        public required bool IsActive { get; set; }
    }
}
