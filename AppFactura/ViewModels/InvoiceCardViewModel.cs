using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppFactura.ViewModels;

public partial class InvoiceCardViewModel : ObservableObject
{
    private readonly Func<int, Task> _open;
    private readonly Func<int, Task> _toggleFavorite;

    public InvoiceCardViewModel(
        int id,
        string title,
        DateTime issuedAt,
        string? thumbnailPath,
        bool isFavorite,
        Func<int, Task> open,
        Func<int, Task> toggleFavorite)
    {
        Id = id;
        Title = title;
        IssuedAt = issuedAt;
        ThumbnailPath = thumbnailPath;
        IsFavorite = isFavorite;
        _open = open;
        _toggleFavorite = toggleFavorite;
    }

    public int Id { get; }
    public string Title { get; }
    public DateTime IssuedAt { get; }
    public string TimeText => IssuedAt.ToString("h:mm tt");
    public string? ThumbnailPath { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FavoriteIcon))]
    private bool isFavorite;

    public string FavoriteIcon => IsFavorite ? "★" : "☆";

    [RelayCommand]
    private Task OpenAsync() => _open(Id);

    [RelayCommand]
    private async Task ToggleFavoriteAsync()
    {
        await _toggleFavorite(Id);
        IsFavorite = !IsFavorite;
    }
}
