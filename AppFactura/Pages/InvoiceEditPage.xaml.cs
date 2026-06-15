using AppFactura.ViewModels;

namespace AppFactura.Pages;

public partial class InvoiceEditPage : ContentPage, IQueryAttributable
{
    private readonly InvoiceEditViewModel _viewModel;

    public InvoiceEditPage(InvoiceEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        int? id = null;
        if (query.TryGetValue("id", out var value) && int.TryParse(value?.ToString(), out var parsed))
            id = parsed;
        _ = _viewModel.LoadAsync(id);
    }
}
