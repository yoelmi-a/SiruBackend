using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SIRU.Core.Application.Interfaces.Common;

namespace SIRU.Infrastructure.Shared.Storage;

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public FileStorageService(IConfiguration configuration)
    {
        _basePath = configuration.GetValue<string>("FileStorage:CvBasePath") ?? "c:/sirus/cv-uploads";
    }

    public async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        var extension = Path.GetExtension(file.FileName);
        if (!extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only PDF files are allowed.");
        }

        var directory = Path.Combine(_basePath, folder);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var fileName = $"{Guid.CreateVersion7()}_{Path.GetFileNameWithoutExtension(file.FileName)}.pdf";
        var filePath = Path.Combine(directory, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return filePath;
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}