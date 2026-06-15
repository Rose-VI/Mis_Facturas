using AppFactura.Data.Models;

namespace AppFactura.Data;

public interface IInvoiceRepository
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Invoice>> GetAsync(InvoiceFilter filter, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(InvoiceSaveRequest request, CancellationToken cancellationToken = default);
    Task<bool> ToggleFavoriteAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
