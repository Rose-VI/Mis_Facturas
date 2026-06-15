using AppFactura.Data;
using AppFactura.Data.Models;

namespace AppFactura.Tests;

public sealed class InvoiceRepositoryTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"appfactura-tests-{Guid.NewGuid():N}");
    private readonly InvoiceRepository _repository;

    public InvoiceRepositoryTests()
    {
        Directory.CreateDirectory(_root);
        var factory = new InvoiceDbContextFactory(Path.Combine(_root, "test.db3"));
        _repository = new InvoiceRepository(factory, new InvoiceFileStore(_root));
    }

    [Fact]
    public async Task Create_search_filter_update_and_delete_manage_database_and_files()
    {
        await _repository.InitializeAsync();
        var firstSource = CreateSource("first.jpg");
        var secondSource = CreateSource("second.jpg");

        var id = await _repository.SaveAsync(new InvoiceSaveRequest(
            null,
            "Supermercado Central",
            new DateTime(2026, 6, 15, 14, 30, 0),
            "Compra semanal",
            true,
            [
                new(null, firstSource, "first.jpg", 0),
                new(null, secondSource, "second.jpg", 1)
            ]));

        var created = await _repository.GetByIdAsync(id);
        Assert.NotNull(created);
        Assert.Equal(2, created.Evidences.Count);
        Assert.All(created.Evidences, evidence => Assert.True(File.Exists(evidence.LocalPath)));

        var search = await _repository.GetAsync(new InvoiceFilter("semanal"));
        Assert.Single(search);
        var favorites = await _repository.GetAsync(new InvoiceFilter(FavoritesOnly: true));
        Assert.Single(favorites);
        var outsideRange = await _repository.GetAsync(
            new InvoiceFilter(FromDate: new DateTime(2026, 6, 16), ToDate: new DateTime(2026, 6, 20)));
        Assert.Empty(outsideRange);

        var removedPath = created.Evidences[1].LocalPath;
        await _repository.SaveAsync(new InvoiceSaveRequest(
            id,
            "Supermercado actualizado",
            created.IssuedAt,
            "Nota actualizada",
            false,
            [
                new(created.Evidences[0].Id, created.Evidences[0].LocalPath, created.Evidences[0].FileName, 0)
            ]));

        var updated = await _repository.GetByIdAsync(id);
        Assert.NotNull(updated);
        Assert.Single(updated.Evidences);
        Assert.False(updated.IsFavorite);
        Assert.False(File.Exists(removedPath));

        var retainedPath = updated.Evidences[0].LocalPath;
        await _repository.DeleteAsync(id);
        Assert.Null(await _repository.GetByIdAsync(id));
        Assert.False(File.Exists(retainedPath));
    }

    [Fact]
    public async Task ToggleFavorite_changes_persisted_state()
    {
        await _repository.InitializeAsync();
        var source = CreateSource("toggle.jpg");
        var id = await _repository.SaveAsync(new InvoiceSaveRequest(
            null,
            "Factura",
            DateTime.Now,
            null,
            false,
            [new(null, source, "toggle.jpg", 0)]));

        var state = await _repository.ToggleFavoriteAsync(id);

        Assert.True(state);
        Assert.True((await _repository.GetByIdAsync(id))!.IsFavorite);
    }

    private string CreateSource(string name)
    {
        var path = Path.Combine(_root, name);
        File.WriteAllBytes(path, [0xFF, 0xD8, 0xFF, 0xD9]);
        return path;
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_root, true);
        }
        catch (IOException)
        {
        }
    }
}
