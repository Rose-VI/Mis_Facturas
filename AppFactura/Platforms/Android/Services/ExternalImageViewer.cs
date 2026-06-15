using Android.Content;
using Android.Webkit;
using AppFactura.Services;

namespace AppFactura.Platforms.Android.Services;

public sealed class ExternalImageViewer : IExternalImageViewer
{
    private const string SharedDirectoryName = "shared-evidences";

    public async Task OpenWithChooserAsync(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("No se encontró la evidencia.", path);

        var sharedPath = await CreateShareableCopyAsync(path);
        var context = Platform.CurrentActivity
            ?? global::Android.App.Application.Context;
        var authority = $"{context.PackageName}.fileProvider";
        var uri = AndroidX.Core.Content.FileProvider.GetUriForFile(
            context,
            authority,
            new Java.IO.File(sharedPath));
        var mimeType = MimeTypeMap.Singleton?.GetMimeTypeFromExtension(
            Path.GetExtension(sharedPath).TrimStart('.').ToLowerInvariant())
            ?? "image/*";

        var viewIntent = new Intent(Intent.ActionView);
        viewIntent.SetDataAndType(uri, mimeType);
        viewIntent.AddFlags(ActivityFlags.GrantReadUriPermission);
        if (Platform.CurrentActivity is null)
            viewIntent.AddFlags(ActivityFlags.NewTask);

        try
        {
            context.StartActivity(viewIntent);
        }
        catch (ActivityNotFoundException)
        {
            throw new InvalidOperationException("No hay una aplicación compatible para abrir esta imagen.");
        }
    }

    private static async Task<string> CreateShareableCopyAsync(string sourcePath)
    {
        var directory = Path.Combine(FileSystem.CacheDirectory, SharedDirectoryName);
        Directory.CreateDirectory(directory);
        DeleteExpiredCopies(directory);

        var extension = Path.GetExtension(sourcePath);
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".jpg";

        var destination = Path.Combine(directory, $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}");
        await using var source = File.OpenRead(sourcePath);
        await using var target = File.Create(destination);
        await source.CopyToAsync(target);
        return destination;
    }

    private static void DeleteExpiredCopies(string directory)
    {
        var expiration = DateTime.UtcNow.AddHours(-12);
        foreach (var file in Directory.EnumerateFiles(directory))
        {
            try
            {
                if (File.GetLastWriteTimeUtc(file) < expiration)
                    File.Delete(file);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
