using Microsoft.AspNetCore.Http;

namespace SIRU.Core.Application.Interfaces.Common;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string folder);
    Task<bool> DeleteFileAsync(string filePath);
    string GetContentType(string fileName);
}