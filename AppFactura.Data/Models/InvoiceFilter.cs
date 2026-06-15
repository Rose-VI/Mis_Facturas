namespace AppFactura.Data.Models;

public sealed record InvoiceFilter(
    string? SearchText = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    bool FavoritesOnly = false);
