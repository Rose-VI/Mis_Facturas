using AppFactura.Data;
using AppFactura.Data.Models;

namespace AppFactura.Tests;

public sealed class InvoiceValidationTests
{
    [Fact]
    public void Validate_requires_title_and_evidence()
    {
        var request = new InvoiceSaveRequest(
            null,
            " ",
            DateTime.Now,
            null,
            false,
            []);

        var errors = InvoiceValidation.Validate(request);

        Assert.Contains(nameof(request.Title), errors.Keys);
        Assert.Contains(nameof(request.Evidences), errors.Keys);
    }

    [Fact]
    public void Validate_rejects_more_than_ten_evidences()
    {
        var evidences = Enumerable.Range(0, 11)
            .Select(index => new EvidenceSaveItem(null, $"image-{index}.jpg", $"image-{index}.jpg", index))
            .ToList();
        var request = new InvoiceSaveRequest(null, "Factura", DateTime.Now, null, false, evidences);

        var errors = InvoiceValidation.Validate(request);

        Assert.Contains(nameof(request.Evidences), errors.Keys);
    }
}
