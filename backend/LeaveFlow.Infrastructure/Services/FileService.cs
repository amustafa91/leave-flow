using LeaveFlow.Core.Interfaces;

namespace LeaveFlow.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly string _uploadDirectory;

    public FileService()
    {
        // Save to wwwroot/uploads which is standard for static files in ASP.NET Core
        _uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(_uploadDirectory))
        {
            Directory.CreateDirectory(_uploadDirectory);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folderName)
    {
        var folderPath = Path.Combine(_uploadDirectory, folderName);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(folderPath, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream);
        }

        // Return relative path for URL (e.g., /uploads/leaves/file.jpg)
        return $"/uploads/{folderName}/{uniqueFileName}";
    }

    public void DeleteFile(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return;

        // Convert relative path (from DB) to absolute filesystem path
        // relativePath starts with /uploads/...
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.TrimStart('/'));

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public async Task<byte[]> GetFileAsync(string relativePath)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.TrimStart('/'));

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        return await File.ReadAllBytesAsync(filePath);
    }
}
