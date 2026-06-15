namespace AppFactura.Data;

public sealed class InvoiceFileStore : IInvoiceFileStore
{
    private readonly string _evidenceDirectory;

    public InvoiceFileStore(string rootDirectory)
    {
        _evidenceDirectory = Path.Combine(rootDirectory, "evidences");
        Directory.CreateDirectory(_evidenceDirectory);
    }

    public async Task<string> ImportAsync(
        string sourcePath,
        string originalFileName,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(sourcePath))
            throw new FileNotFoundException("No se encontró la imagen seleccionada.", sourcePath);

        var extension = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".jpg";

        var destination = Path.Combine(_evidenceDirectory, $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}");

        await using var source = File.OpenRead(sourcePath);
        await using var target = File.Create(destination);
        await source.CopyToAsync(target, cancellationToken);
        return destination;
    }

    public void Delete(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        try
        {
            var fullPath = Path.GetFullPath(path);
            var root = Path.GetFullPath(_evidenceDirectory) + Path.DirectorySeparatorChar;
            if (fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch (IOException)
        {
            // A later operation can retry stale files.
        }
        catch (UnauthorizedAccessException)
        {
            // The database operation should remain usable if Android locks a file.
        }
    }
}
