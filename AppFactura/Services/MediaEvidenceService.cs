namespace AppFactura.Services;

public sealed class MediaEvidenceService : IMediaEvidenceService
{
    public async Task<StagedMedia?> CaptureAsync(CancellationToken cancellationToken = default)
    {
        if (!MediaPicker.Default.IsCaptureSupported)
            throw new NotSupportedException("Este dispositivo no permite tomar fotografías.");

        var permission = await Permissions.RequestAsync<Permissions.Camera>();
        if (permission != PermissionStatus.Granted)
            throw new UnauthorizedAccessException("Se necesita permiso para usar la cámara.");

        var result = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
        {
            Title = "Fotografiar factura",
            MaximumWidth = 1800,
            MaximumHeight = 1800,
            CompressionQuality = 85,
            RotateImage = true,
            PreserveMetaData = false
        });

        return result is null ? null : await StageAsync(result, cancellationToken);
    }

    public async Task<IReadOnlyList<StagedMedia>> PickAsync(
        int maximumCount,
        CancellationToken cancellationToken = default)
    {
        if (maximumCount <= 0)
            return [];

        var results = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
        {
            Title = "Seleccionar evidencias",
            SelectionLimit = maximumCount,
            MaximumWidth = 1800,
            MaximumHeight = 1800,
            CompressionQuality = 85,
            RotateImage = true,
            PreserveMetaData = false
        });

        var staged = new List<StagedMedia>();
        try
        {
            foreach (var result in results.Take(maximumCount))
                staged.Add(await StageAsync(result, cancellationToken));
            return staged;
        }
        catch
        {
            foreach (var item in staged)
                DeleteStaged(item.LocalPath);
            throw;
        }
    }

    public void DeleteStaged(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        try
        {
            var fullPath = Path.GetFullPath(path);
            var cacheRoot = Path.GetFullPath(FileSystem.CacheDirectory) + Path.DirectorySeparatorChar;
            if (fullPath.StartsWith(cacheRoot, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static async Task<StagedMedia> StageAsync(
        FileResult result,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(result.FileName);
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".jpg";

        var stagedPath = Path.Combine(FileSystem.CacheDirectory, $"invoice_{Guid.NewGuid():N}{extension}");
        await using var source = await result.OpenReadAsync();
        await using var destination = File.Create(stagedPath);
        await source.CopyToAsync(destination, cancellationToken);
        return new StagedMedia(stagedPath, result.FileName);
    }
}
