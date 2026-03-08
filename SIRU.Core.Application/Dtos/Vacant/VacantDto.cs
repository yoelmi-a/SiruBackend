namespace SIRU.Core.Application.Dtos.Vacant
{
    public class VacantDto
    {
        public required string Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Profile { get; set; }
        public required DateTime PublicationDate { get; set; }
        public DateTime? HiringDate { get; set; }
        public required int Status { get; set; }
    }
}
