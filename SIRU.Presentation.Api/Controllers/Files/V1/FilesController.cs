using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SIRU.Core.Domain.Settings;

namespace SIRU.Presentation.Api.Controllers.Files.V1;

[ApiController]
[Route("api/files")]
[Produces("application/json")]
public class FilesController : ControllerBase
{
    private readonly FileStorageSettings _settings;

    public FilesController(IOptions<FileStorageSettings> settings)
    {
        _settings = settings.Value;
    }

    [HttpGet("cv/{fileName}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetCv(string fileName)
    {
        var filePath = Path.Combine(_settings.CvBasePath, "cvs", fileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        return PhysicalFile(filePath, "application/pdf");
    }
}