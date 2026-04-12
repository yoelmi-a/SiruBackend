namespace SIRU.Core.Application.Dtos.Accounts
{
    public class GetAccountDto
    {
        public required string Id { get; set; }
        public required bool IsVerified { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
