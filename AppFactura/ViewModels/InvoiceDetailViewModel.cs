using AppFactura.Data;
using AppFactura.Pages;
using AppFactura.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AppFactura.ViewModels;

public partial class InvoiceDetailViewModel(
    IInvoiceRepository repository,
    IExternalImageViewer imageViewer) : ViewModelBase
{
    private int _invoiceId;

    public ObservableCollection<string> EvidencePaths { get; } = [];

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string dateTimeText = string.Empty;

    [ObservableProperty]
    private string memorandum = "Sin memorándum";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FavoriteIcon))]
    private bool isFavorite;

    public string FavoriteIcon => IsFavorite ? "★" : "☆";

    public Task LoadAsync(int id)
    {
        _invoiceId = id;
        return RunBusyAsync(async () =>
        {
            var invoice = await repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("La factura ya no existe.");
            Title = invoice.Title;
            DateTimeText = invoice.IssuedAt.ToString("dddd, d 'de' MMMM 'de' yyyy · h:mm tt");
            Memorandum = string.IsNullOrWhiteSpace(invoice.Memorandum)
                ? "Sin memorándum"
                : invoice.Memorandum;
            IsFavorite = invoice.IsFavorite;
            EvidencePaths.Clear();
            foreach (var evidence in invoice.Evidences)
                EvidencePaths.Add(evidence.LocalPath);
        });
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync()
    {
        IsFavorite = await repository.ToggleFavoriteAsync(_invoiceId);
    }

    [RelayCommand]
    private Task EditAsync() =>
        Shell.Current.GoToAsync($"{nameof(InvoiceEditPage)}?id={_invoiceId}");

    [RelayCommand]
    private async Task DeleteAsync()
    {
        var confirmed = await Shell.Current.DisplayAlertAsync(
            "Eliminar factura",
            "Esta acción eliminará también todas sus evidencias. ¿Deseas continuar?",
            "Eliminar",
            "Cancelar");
        if (!confirmed)
            return;

        await RunBusyAsync(async () =>
        {
            await repository.DeleteAsync(_invoiceId);
            await Shell.Current.GoToAsync("..");
        });
    }

    [RelayCommand]
    private Task OpenEvidenceAsync(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Task.CompletedTask;
        return RunBusyAsync(() => imageViewer.OpenWithChooserAsync(path));
    }
}
