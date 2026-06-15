namespace AppFactura.Data.Models;

public sealed record EvidenceSaveItem(
    int? ExistingEvidenceId,
    string LocalPath,
    string FileName,
    int SortOrder);

public sealed record InvoiceSaveRequest(
    int? Id,
    string Title,
    DateTime IssuedAt,
    string? Memorandum,
    bool IsFavorite,
    IReadOnlyList<EvidenceSaveItem> Evidences);
