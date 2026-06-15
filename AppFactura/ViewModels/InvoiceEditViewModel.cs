using AppFactura.Data;
using AppFactura.Data.Models;
using AppFactura.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AppFactura.ViewModels;

public partial class InvoiceEditViewModel(
    IInvoiceRepository repository,
    IMediaEvidenceService mediaService) : ViewModelBase
{
    private int? _invoiceId;

    public ObservableCollection<EvidenceItemViewModel> Evidences { get; } = [];

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private DateTime invoiceDate = DateTime.Today;

    [ObservableProperty]
    private TimeSpan invoiceTime = DateTime.Now.TimeOfDay;

    [ObservableProperty]
    private string? memorandum;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FavoriteIcon))]
    private bool isFavorite;

    [ObservableProperty]
    private string? titleError;

    [ObservableProperty]
    private string? evidenceError;

    [ObservableProperty]
    private string pageTitle = "Nueva factura";

    public string FavoriteIcon => IsFavorite ? "★" : "☆";
    public string EvidenceCountText => $"{Evidences.Count}/{InvoiceValidation.MaximumEvidenceCount}";

    public async Task LoadAsync(int? id)
    {
        _invoiceId = id;
        if (id is null)
        {
            InvoiceDate = DateTime.Today;
            InvoiceTime = DateTime.Now.TimeOfDay;
            return;
        }

        await RunBusyAsync(async () =>
        {
            var invoice = await repository.GetByIdAsync(id.Value)
                ?? throw new KeyNotFoundException("La factura ya no existe.");
            Title = invoice.Title;
            InvoiceDate = invoice.IssuedAt.Date;
            InvoiceTime = invoice.IssuedAt.TimeOfDay;
            Memorandum = invoice.Memorandum;
            IsFavorite = invoice.IsFavorite;
            PageTitle = "Editar factura";
            Evidences.Clear();
            foreach (var evidence in invoice.Evidences)
            {
                Evidences.Add(new EvidenceItemViewModel
                {
                    ExistingEvidenceId = evidence.Id,
                    LocalPath = evidence.LocalPath,
                    FileName = evidence.FileName
                });
            }
            NotifyEvidenceChanged();
        });
    }

    [RelayCommand]
    private void ToggleFavorite() => IsFavorite = !IsFavorite;

    [RelayCommand]
    private Task CaptureAsync() => AddMediaAsync(() => mediaService.CaptureAsync());

    [RelayCommand]
    private Task PickAsync() => AddMediaAsync(async () =>
    {
        var remaining = InvoiceValidation.MaximumEvidenceCount - Evidences.Count;
        var items = await mediaService.PickAsync(remaining);
        return items;
    });

    [RelayCommand]
    private void RemoveEvidence(EvidenceItemViewModel? evidence)
    {
        if (evidence is null)
            return;
        if (evidence.IsStaged)
            mediaService.DeleteStaged(evidence.LocalPath);
        Evidences.Remove(evidence);
        NotifyEvidenceChanged();
    }

    [RelayCommand]
    private void MoveLeft(EvidenceItemViewModel? evidence)
    {
        if (evidence is null)
            return;
        var index = Evidences.IndexOf(evidence);
        if (index > 0)
            Evidences.Move(index, index - 1);
    }

    [RelayCommand]
    private void MoveRight(EvidenceItemViewModel? evidence)
    {
        if (evidence is null)
            return;
        var index = Evidences.IndexOf(evidence);
        if (index >= 0 && index < Evidences.Count - 1)
            Evidences.Move(index, index + 1);
    }

    [RelayCommand]
    private Task SaveAsync() => RunBusyAsync(async () =>
    {
        var issuedAt = InvoiceDate.Date.Add(InvoiceTime);
        var request = new InvoiceSaveRequest(
            _invoiceId,
            Title,
            issuedAt,
            Memorandum,
            IsFavorite,
            Evidences.Select((item, index) => new EvidenceSaveItem(
                item.ExistingEvidenceId,
                item.LocalPath,
                item.FileName,
                index)).ToList());

        var errors = InvoiceValidation.Validate(request);
        TitleError = errors.GetValueOrDefault(nameof(request.Title));
        EvidenceError = errors.GetValueOrDefault(nameof(request.Evidences));
        if (errors.Count > 0)
            return;

        await repository.SaveAsync(request);
        CleanupStaged();
        await Shell.Current.GoToAsync("..");
    });

    [RelayCommand]
    private async Task CancelAsync()
    {
        CleanupStaged();
        await Shell.Current.GoToAsync("..");
    }

    public void CleanupStaged()
    {
        foreach (var evidence in Evidences.Where(x => x.IsStaged))
            mediaService.DeleteStaged(evidence.LocalPath);
    }

    private Task AddMediaAsync(Func<Task<StagedMedia?>> source) => RunBusyAsync(async () =>
    {
        if (Evidences.Count >= InvoiceValidation.MaximumEvidenceCount)
        {
            EvidenceError = $"Solo puedes agregar {InvoiceValidation.MaximumEvidenceCount} evidencias.";
            return;
        }

        var item = await source();
        if (item is not null)
            AddStaged(item);
    });

    private Task AddMediaAsync(Func<Task<IReadOnlyList<StagedMedia>>> source) => RunBusyAsync(async () =>
    {
        if (Evidences.Count >= InvoiceValidation.MaximumEvidenceCount)
        {
            EvidenceError = $"Solo puedes agregar {InvoiceValidation.MaximumEvidenceCount} evidencias.";
            return;
        }

        foreach (var item in await source())
            AddStaged(item);
    });

    private void AddStaged(StagedMedia media)
    {
        Evidences.Add(new EvidenceItemViewModel
        {
            LocalPath = media.LocalPath,
            FileName = media.FileName
        });
        EvidenceError = null;
        NotifyEvidenceChanged();
    }

    private void NotifyEvidenceChanged() => OnPropertyChanged(nameof(EvidenceCountText));
}
