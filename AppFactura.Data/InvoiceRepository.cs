using AppFactura.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace AppFactura.Data;

public sealed class InvoiceRepository(
    IInvoiceDbContextFactory contextFactory,
    IInvoiceFileStore fileStore) : IInvoiceRepository
{
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _isInitialized;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
            return;

        await _initializationLock.WaitAsync(cancellationToken);
        try
        {
            if (_isInitialized)
                return;
            await using var db = contextFactory.CreateDbContext();
            await db.Database.MigrateAsync(cancellationToken);
            _isInitialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public async Task<IReadOnlyList<Invoice>> GetAsync(
        InvoiceFilter filter,
        CancellationToken cancellationToken = default)
    {
        await using var db = contextFactory.CreateDbContext();
        IQueryable<Invoice> query = db.Invoices
            .AsNoTracking()
            .Include(x => x.Evidences);

        var search = filter.SearchText?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search}%";
            query = query.Where(x =>
                EF.Functions.Like(x.Title, pattern) ||
                (x.Memorandum != null && EF.Functions.Like(x.Memorandum, pattern)));
        }

        var fromDate = filter.FromDate;
        var toDate = filter.ToDate;
        if (fromDate is not null && toDate is not null && fromDate > toDate)
            (fromDate, toDate) = (toDate, fromDate);

        if (fromDate is not null)
            query = query.Where(x => x.IssuedAt >= fromDate.Value.Date);

        if (toDate is not null)
        {
            var exclusiveEnd = toDate.Value.Date.AddDays(1);
            query = query.Where(x => x.IssuedAt < exclusiveEnd);
        }

        if (filter.FavoritesOnly)
            query = query.Where(x => x.IsFavorite);

        var invoices = await query
            .OrderByDescending(x => x.IssuedAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var invoice in invoices)
            invoice.Evidences = invoice.Evidences.OrderBy(x => x.SortOrder).ToList();

        return invoices;
    }

    public async Task<Invoice?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = contextFactory.CreateDbContext();
        var invoice = await db.Invoices
            .AsNoTracking()
            .Include(x => x.Evidences)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (invoice is not null)
            invoice.Evidences = invoice.Evidences.OrderBy(x => x.SortOrder).ToList();

        return invoice;
    }

    public async Task<int> SaveAsync(InvoiceSaveRequest request, CancellationToken cancellationToken = default)
    {
        var errors = InvoiceValidation.Validate(request);
        if (errors.Count > 0)
            throw new ArgumentException(string.Join(" ", errors.Values));

        await using var db = contextFactory.CreateDbContext();
        var now = DateTime.Now;
        Invoice invoice;

        if (request.Id is null)
        {
            invoice = new Invoice { CreatedAt = now };
            db.Invoices.Add(invoice);
        }
        else
        {
            invoice = await db.Invoices
                .Include(x => x.Evidences)
                .SingleOrDefaultAsync(x => x.Id == request.Id.Value, cancellationToken)
                ?? throw new KeyNotFoundException("La factura ya no existe.");
        }

        invoice.Title = request.Title.Trim();
        invoice.IssuedAt = request.IssuedAt;
        invoice.Memorandum = string.IsNullOrWhiteSpace(request.Memorandum) ? null : request.Memorandum.Trim();
        invoice.IsFavorite = request.IsFavorite;
        invoice.UpdatedAt = now;

        var requestedExistingIds = request.Evidences
            .Where(x => x.ExistingEvidenceId.HasValue)
            .Select(x => x.ExistingEvidenceId!.Value)
            .ToHashSet();

        var removed = invoice.Evidences.Where(x => !requestedExistingIds.Contains(x.Id)).ToList();
        var importedPaths = new List<string>();

        try
        {
            foreach (var item in request.Evidences.OrderBy(x => x.SortOrder))
            {
                if (item.ExistingEvidenceId is int existingId)
                {
                    var evidence = invoice.Evidences.SingleOrDefault(x => x.Id == existingId)
                        ?? throw new InvalidOperationException("Una evidencia seleccionada ya no existe.");
                    evidence.SortOrder = item.SortOrder;
                    continue;
                }

                var importedPath = await fileStore.ImportAsync(
                    item.LocalPath,
                    item.FileName,
                    cancellationToken);
                importedPaths.Add(importedPath);

                invoice.Evidences.Add(new InvoiceEvidence
                {
                    LocalPath = importedPath,
                    FileName = item.FileName,
                    SortOrder = item.SortOrder
                });
            }

            db.InvoiceEvidences.RemoveRange(removed);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            foreach (var path in importedPaths)
                fileStore.Delete(path);
            throw;
        }

        foreach (var evidence in removed)
            fileStore.Delete(evidence.LocalPath);

        return invoice.Id;
    }

    public async Task<bool> ToggleFavoriteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = contextFactory.CreateDbContext();
        var invoice = await db.Invoices.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("La factura ya no existe.");
        invoice.IsFavorite = !invoice.IsFavorite;
        invoice.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync(cancellationToken);
        return invoice.IsFavorite;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = contextFactory.CreateDbContext();
        var invoice = await db.Invoices
            .Include(x => x.Evidences)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (invoice is null)
            return;

        var paths = invoice.Evidences.Select(x => x.LocalPath).ToList();
        db.Invoices.Remove(invoice);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var path in paths)
            fileStore.Delete(path);
    }
}
