using AppFactura.Data.Models;

namespace AppFactura.Data;

public static class InvoiceValidation
{
    public const int MaximumEvidenceCount = 10;

    public static IReadOnlyDictionary<string, string> Validate(InvoiceSaveRequest request)
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(request.Title))
            errors[nameof(request.Title)] = "El título es obligatorio.";
        else if (request.Title.Trim().Length > 160)
            errors[nameof(request.Title)] = "El título no puede superar 160 caracteres.";

        if (request.Memorandum?.Length > 2000)
            errors[nameof(request.Memorandum)] = "El memorándum no puede superar 2000 caracteres.";

        if (request.Evidences.Count == 0)
            errors[nameof(request.Evidences)] = "Agrega al menos una evidencia.";
        else if (request.Evidences.Count > MaximumEvidenceCount)
            errors[nameof(request.Evidences)] = $"Solo puedes agregar {MaximumEvidenceCount} evidencias.";

        return errors;
    }
}
