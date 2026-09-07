namespace CargoFlow.Application.Occurrences;

public record OccurrenceAttachmentDto(Guid Id, string FileUrl, string FileName, string UploadedAt);

public record OccurrenceDto(
    Guid Id,
    Guid? TripId,
    Guid VehicleId,
    string VehiclePlate,
    Guid DriverId,
    string DriverName,
    string Type,
    string OccurredAt,
    string Location,
    string Description,
    Guid? ResponsibleUserId,
    string Status,
    string? ResolvedAt,
    List<OccurrenceAttachmentDto> Attachments);

public record CreateOccurrenceRequest(
    Guid? TripId,
    Guid VehicleId,
    Guid DriverId,
    string Type,
    string OccurredAt,
    string Location,
    string Description,
    Guid? ResponsibleUserId);

public record AddOccurrenceAttachmentRequest(string FileUrl, string FileName);
