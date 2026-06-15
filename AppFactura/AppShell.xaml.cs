using AppFactura.Pages;

namespace AppFactura;

public partial class AppShell : Shell
{
    public AppShell(MainPage mainPage)
    {
        InitializeComponent();
        Items.Add(new ShellContent
        {
            Route = nameof(MainPage),
            Content = mainPage
        });

        Routing.RegisterRoute(nameof(InvoiceEditPage), typeof(InvoiceEditPage));
        Routing.RegisterRoute(nameof(InvoiceDetailPage), typeof(InvoiceDetailPage));
        Routing.RegisterRoute(nameof(ImageViewerPage), typeof(ImageViewerPage));
    }
}
