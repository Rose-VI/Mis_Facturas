using AppFactura.Data.Models;

namespace AppFactura.Data;

public sealed record InvoiceDateGroup(DateTime Date, IReadOnlyList<Invoice> Invoices);

public static class InvoiceOrganizer
{
    public static IReadOnlyList<InvoiceDateGroup> GroupByDate(IEnumerable<Invoice> invoices) =>
        invoices
            .OrderByDescending(x => x.IssuedAt)
            .ThenByDescending(x => x.Id)
            .GroupBy(x => x.IssuedAt.Date)
            .Select(group => new InvoiceDateGroup(group.Key, group.ToList()))
            .ToList();
}
