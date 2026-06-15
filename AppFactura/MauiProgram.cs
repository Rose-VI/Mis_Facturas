using AppFactura.Data;
using AppFactura.Pages;
using AppFactura.Platforms.Android.Services;
using AppFactura.Services;
using AppFactura.ViewModels;
using Microsoft.Extensions.Logging;

namespace AppFactura;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "appfactura.db3");
        builder.Services.AddSingleton<IInvoiceDbContextFactory>(
            new InvoiceDbContextFactory(databasePath));
        builder.Services.AddSingleton<IInvoiceFileStore>(
            new InvoiceFileStore(FileSystem.AppDataDirectory));
        builder.Services.AddSingleton<IInvoiceRepository, InvoiceRepository>();
        builder.Services.AddSingleton<IMediaEvidenceService, MediaEvidenceService>();
        builder.Services.AddSingleton<IExternalImageViewer, ExternalImageViewer>();

        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<InvoiceEditViewModel>();
        builder.Services.AddTransient<InvoiceEditPage>();
        builder.Services.AddTransient<InvoiceDetailViewModel>();
        builder.Services.AddTransient<InvoiceDetailPage>();
        builder.Services.AddTransient<ImageViewerPage>();
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
