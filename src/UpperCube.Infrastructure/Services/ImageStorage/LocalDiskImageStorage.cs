using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using UpperCube.Application.Abstractions.Media;

namespace UpperCube.Infrastructure.Services.ImageStorage;

public sealed class LocalDiskImageStorage(IConfiguration configuration, IHostEnvironment environment) : IImageStorage
{
    private static readonly IReadOnlyDictionary<string, string> AllowedExtensions = new Dictionary<string, string>
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp"
    };

    public async Task<string> SaveAsync(Stream stream, string fileName, string contentType,
        CancellationToken ct = default)
    {
        if (!AllowedExtensions.TryGetValue(contentType, out var extension))
            throw new InvalidOperationException("Unsupported image content type.");

        var rootPath = ResolveRootPath();
        Directory.CreateDirectory(rootPath);

        var sourceExtension = Path.GetExtension(fileName);
        if (!string.IsNullOrWhiteSpace(sourceExtension)
            && AllowedExtensions.Values.Contains(sourceExtension, StringComparer.OrdinalIgnoreCase))
            extension = sourceExtension.ToLowerInvariant();

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(rootPath, storedFileName);

        await using var fileStream = File.Create(fullPath);
        await stream.CopyToAsync(fileStream, ct);

        return $"/uploads/{storedFileName}";
    }

    public Task DeleteAsync(string path, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(path)) return Task.CompletedTask;

        var rootPath = ResolveRootPath();
        var fileName = Path.GetFileName(path);
        var fullPath = Path.Combine(rootPath, fileName);

        if (File.Exists(fullPath)) File.Delete(fullPath);

        return Task.CompletedTask;
    }

    private string ResolveRootPath()
    {
        var configuredRoot = configuration["ImageStorage:RootPath"] ?? "wwwroot/uploads";
        return Path.IsPathRooted(configuredRoot)
            ? configuredRoot
            : Path.Combine(environment.ContentRootPath, configuredRoot);
    }
}