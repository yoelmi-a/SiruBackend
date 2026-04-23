namespace SIRU.Core.Application.Dtos.Emails;

public class EmailRequestDto
{
    public required string Subject { get; set; }
    public required string HtmlBody { get; set; }
    public required ICollection<string> To { get; set; }
}