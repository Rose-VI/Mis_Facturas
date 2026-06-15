namespace AppFactura.Services;

public interface IExternalImageViewer
{
    Task OpenWithChooserAsync(string path);
}
