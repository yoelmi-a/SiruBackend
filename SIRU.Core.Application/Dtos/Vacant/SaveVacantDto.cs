namespace SIRU.Core.Application.Dtos.Vacant
{
    public class SaveVacantDto
    {
        public string? Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Profile { get; set; }
        public required int Status { get; set; }
    }
}
