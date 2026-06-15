using AppFactura.Data;
using AppFactura.Data.Models;
using AppFactura.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AppFactura.ViewModels;

public partial class MainViewModel(IInvoiceRepository repository) : ViewModelBase
{
    public ObservableCollection<InvoiceDayGroup> Groups { get; } = [];

    [ObservableProperty]
    private string? searchText;

    [ObservableProperty]
    private DateTime fromDate = DateTime.Today.AddMonths(-1);

    [ObservableProperty]
    private DateTime toDate = DateTime.Today;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageTitle))]
    private bool favoritesOnly;

    [ObservableProperty]
    private bool useDateFilter;

    [ObservableProperty]
    private bool isFilterPanelVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResults))]
    [NotifyPropertyChangedFor(nameof(HasNoResults))]
    [NotifyPropertyChangedFor(nameof(EmptyMessage))]
    private int resultCount;

    public bool HasResults => ResultCount > 0;
    public bool HasNoResults => ResultCount == 0;
    public string PageTitle => FavoritesOnly ? "Facturas favoritas" : "Mis facturas";
    public string EmptyMessage => string.IsNullOrWhiteSpace(SearchText) && !UseDateFilter && !FavoritesOnly
        ? "Aún no hay facturas registradas"
        : "No se encontraron facturas";

    partial void OnSearchTextChanged(string? value)
    {
        OnPropertyChanged(nameof(EmptyMessage));
        SearchCommand.Execute(null);
    }

    [RelayCommand]
    public Task LoadAsync() => RunBusyAsync(LoadCoreAsync);

    [RelayCommand]
    private Task SearchAsync() => LoadAsync();

    [RelayCommand]
    private void ToggleFilters() => IsFilterPanelVisible = !IsFilterPanelVisible;

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        IsFilterPanelVisible = false;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SearchText = null;
        UseDateFilter = false;
        FavoritesOnly = false;
        FromDate = DateTime.Today.AddMonths(-1);
        ToDate = DateTime.Today;
        IsFilterPanelVisible = false;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ShowFavoritesAsync()
    {
        FavoritesOnly = !FavoritesOnly;
        await LoadAsync();
    }

    [RelayCommand]
    private Task AddAsync() => Shell.Current.GoToAsync(nameof(InvoiceEditPage));

    private async Task LoadCoreAsync()
    {
        await repository.InitializeAsync();
        var filter = new InvoiceFilter(
            SearchText,
            UseDateFilter ? FromDate : null,
            UseDateFilter ? ToDate : null,
            FavoritesOnly);
        var invoices = await repository.GetAsync(filter);

        Groups.Clear();
        foreach (var dateGroup in InvoiceOrganizer.GroupByDate(invoices))
        {
            var cards = dateGroup.Invoices.Select(invoice => new InvoiceCardViewModel(
                invoice.Id,
                invoice.Title,
                invoice.IssuedAt,
                invoice.Evidences.FirstOrDefault()?.LocalPath,
                invoice.IsFavorite,
                OpenInvoiceAsync,
                ToggleFavoriteAsync));
            Groups.Add(new InvoiceDayGroup(dateGroup.Date, cards));
        }

        ResultCount = invoices.Count;
        OnPropertyChanged(nameof(EmptyMessage));
    }

    private Task OpenInvoiceAsync(int id) =>
        Shell.Current.GoToAsync($"{nameof(InvoiceDetailPage)}?id={id}");

    private async Task ToggleFavoriteAsync(int id)
    {
        await repository.ToggleFavoriteAsync(id);
        if (FavoritesOnly)
            await LoadAsync();
    }
}
