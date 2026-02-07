namespace LeaveFlow.Core.Interfaces;

public interface IFileService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folderName);
    void DeleteFile(string filePath);
    Task<byte[]> GetFileAsync(string filePath);
}
