using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AppFactura.ViewModels;

public sealed class InvoiceDayGroup : ObservableCollection<InvoiceCardViewModel>
{
    private readonly List<InvoiceCardViewModel> _allItems;
    private bool _isExpanded = true;

    public InvoiceDayGroup(DateTime date, IEnumerable<InvoiceCardViewModel> items)
    {
        Date = date.Date;
        _allItems = items.ToList();
        foreach (var item in _allItems)
            Add(item);
        ToggleCommand = new RelayCommand(Toggle);
    }

    public DateTime Date { get; }
    public string DisplayDate => Date.ToString("dddd, d 'de' MMMM");
    public int TotalCount => _allItems.Count;
    public IRelayCommand ToggleCommand { get; }

    public bool IsExpanded
    {
        get => _isExpanded;
        private set
        {
            if (_isExpanded == value)
                return;
            _isExpanded = value;
            OnPropertyChanged(new(nameof(IsExpanded)));
            OnPropertyChanged(new(nameof(Arrow)));
        }
    }

    public string Arrow => IsExpanded ? "⌄" : "›";

    private void Toggle()
    {
        IsExpanded = !IsExpanded;
        Clear();
        if (IsExpanded)
        {
            foreach (var item in _allItems)
                Add(item);
        }
    }
}
