namespace AppFactura.Services;

public sealed record StagedMedia(string LocalPath, string FileName);

public interface IMediaEvidenceService
{
    Task<StagedMedia?> CaptureAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StagedMedia>> PickAsync(int maximumCount, CancellationToken cancellationToken = default);
    void DeleteStaged(string? path);
}
