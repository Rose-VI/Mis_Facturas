using AppFactura.ViewModels;

namespace AppFactura.Pages;

public partial class InvoiceDetailPage : ContentPage, IQueryAttributable
{
    private readonly InvoiceDetailViewModel _viewModel;
    private int _id;

    public InvoiceDetailPage(InvoiceDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var value) && int.TryParse(value?.ToString(), out _id))
            _ = _viewModel.LoadAsync(_id);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_id > 0)
            _ = _viewModel.LoadAsync(_id);
    }

    private async void OnBackClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");

    private async void OnActionsClicked(object? sender, EventArgs e)
    {
        var action = await DisplayActionSheetAsync("Acciones", "Cancelar", null, "Editar", "Eliminar");
        if (action == "Editar")
            _viewModel.EditCommand.Execute(null);
        else if (action == "Eliminar")
            _viewModel.DeleteCommand.Execute(null);
    }
}
