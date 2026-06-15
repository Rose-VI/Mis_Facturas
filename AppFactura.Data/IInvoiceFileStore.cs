namespace AppFactura.Data;

public interface IInvoiceFileStore
{
    Task<string> ImportAsync(string sourcePath, string originalFileName, CancellationToken cancellationToken = default);
    void Delete(string? path);
}
