namespace UpperCube.Application.Abstractions.Media;

public interface IImageStorage
{
    Task<string> SaveAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default);

    Task DeleteAsync(string path, CancellationToken ct = default);
}