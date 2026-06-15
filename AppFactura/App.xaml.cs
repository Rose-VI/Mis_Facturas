using AppFactura.Data;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactura;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Light;
        _services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Resolve pages only after App.xaml has loaded its global resources.
        var shell = _services.GetRequiredService<AppShell>();
        var repository = _services.GetRequiredService<IInvoiceRepository>();
        var window = new Window(shell);
        _ = InitializeDatabaseAsync(window, repository);
        return window;
    }

    private static async Task InitializeDatabaseAsync(
        Window window,
        IInvoiceRepository repository)
    {
        try
        {
            await repository.InitializeAsync();
        }
        catch (Exception ex)
        {
            await window.Page!.DisplayAlertAsync(
                "Error de almacenamiento",
                $"No se pudo preparar la base de datos. {ex.Message}",
                "Entendido");
        }
    }
}
