using CommunityToolkit.Mvvm.ComponentModel;

namespace AppFactura.ViewModels;

public partial class EvidenceItemViewModel : ObservableObject
{
    public int? ExistingEvidenceId { get; init; }
    public required string LocalPath { get; init; }
    public required string FileName { get; init; }
    public bool IsStaged => ExistingEvidenceId is null;
}
