using Microsoft.EntityFrameworkCore;

namespace AppFactura.Data;

public sealed class InvoiceDbContextFactory(string databasePath) : IInvoiceDbContextFactory
{
    public InvoiceDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<InvoiceDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;

        return new InvoiceDbContext(options);
    }
}
