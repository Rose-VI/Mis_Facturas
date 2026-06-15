using System.ComponentModel.DataAnnotations;

namespace AppFactura.Data.Models;

public sealed class Invoice
{
    public int Id { get; set; }

    [Required, MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    public DateTime IssuedAt { get; set; }

    [MaxLength(2000)]
    public string? Memorandum { get; set; }

    public bool IsFavorite { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<InvoiceEvidence> Evidences { get; set; } = [];
}
