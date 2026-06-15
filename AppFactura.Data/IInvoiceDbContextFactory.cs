namespace AppFactura.Data;

public interface IInvoiceDbContextFactory
{
    InvoiceDbContext CreateDbContext();
}
