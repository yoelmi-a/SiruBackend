using SIRU.Core.Application.Dtos.Emails;

namespace SIRU.Core.Application.Interfaces.Shared;

public interface IEmailService
{
    Task SendAsync(EmailRequestDto emailRequest);
}