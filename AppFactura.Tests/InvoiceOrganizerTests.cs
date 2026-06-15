using AppFactura.Data;
using AppFactura.Data.Models;

namespace AppFactura.Tests;

public sealed class InvoiceOrganizerTests
{
    [Fact]
    public void GroupByDate_orders_days_and_invoices_newest_first()
    {
        var invoices = new[]
        {
            new Invoice { Id = 1, Title = "A", IssuedAt = new DateTime(2026, 6, 14, 9, 0, 0) },
            new Invoice { Id = 2, Title = "B", IssuedAt = new DateTime(2026, 6, 15, 8, 0, 0) },
            new Invoice { Id = 3, Title = "C", IssuedAt = new DateTime(2026, 6, 15, 17, 0, 0) }
        };

        var groups = InvoiceOrganizer.GroupByDate(invoices);

        Assert.Equal(new DateTime(2026, 6, 15), groups[0].Date);
        Assert.Equal(new[] { 3, 2 }, groups[0].Invoices.Select(x => x.Id));
        Assert.Equal(new DateTime(2026, 6, 14), groups[1].Date);
    }
}
