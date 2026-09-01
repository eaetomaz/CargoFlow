using CargoFlow.Domain.Common;

namespace CargoFlow.Domain.Entities.Occurrences;

public class OccurrenceAttachment : Entity
{
    public Guid OccurrenceId { get; set; }
    public Occurrence? Occurrence { get; set; }

    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
