using AppFactura.Controls;
using System.Collections.ObjectModel;

namespace AppFactura.Pages;

public sealed class ImageViewerPage : ContentPage, IQueryAttributable
{
    private readonly ObservableCollection<string> _paths = [];
    private readonly CarouselView _carousel;

    public ImageViewerPage()
    {
        Shell.SetNavBarIsVisible(this, false);
        BackgroundColor = Colors.Black;

        _carousel = new CarouselView
        {
            ItemsSource = _paths,
            ItemTemplate = new DataTemplate(() =>
            {
                var image = new ZoomableImage();
                image.SetBinding(ZoomableImage.SourceProperty, ".");
                return image;
            })
        };

        var close = new Button
        {
            Text = "✕",
            FontSize = 22,
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb("#66000000"),
            WidthRequest = 48,
            HeightRequest = 48,
            CornerRadius = 24,
            Margin = new Thickness(14, 18)
        };
        close.Clicked += async (_, _) => await Shell.Current.GoToAsync("..");

        var indicator = new IndicatorView
        {
            IndicatorColor = Color.FromArgb("#70808080"),
            SelectedIndicatorColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 0, 0, 24)
        };
        _carousel.IndicatorView = indicator;

        var grid = new Grid();
        grid.Children.Add(_carousel);
        grid.Children.Add(indicator);
        grid.Children.Add(close);
        Content = grid;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _paths.Clear();
        if (query.TryGetValue("paths", out var pathsValue))
        {
            var decoded = Uri.UnescapeDataString(pathsValue?.ToString() ?? string.Empty);
            foreach (var path in decoded.Split('|', StringSplitOptions.RemoveEmptyEntries))
                _paths.Add(path);
        }

        if (query.TryGetValue("index", out var indexValue) &&
            int.TryParse(indexValue?.ToString(), out var index))
        {
            _carousel.Position = Math.Clamp(index, 0, Math.Max(0, _paths.Count - 1));
        }
    }
}
