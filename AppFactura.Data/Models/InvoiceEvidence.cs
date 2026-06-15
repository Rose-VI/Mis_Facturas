using System.ComponentModel.DataAnnotations;

namespace AppFactura.Data.Models;

public sealed class InvoiceEvidence
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    [Required, MaxLength(500)]
    public string LocalPath { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
