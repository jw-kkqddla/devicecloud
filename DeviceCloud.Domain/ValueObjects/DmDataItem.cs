namespace DeviceCloud.Domain.ValueObjects;

public record DmDataItem
{
    public int Id { get; init; }
    public string? Value { get; init; }
    public string? Type { get; init; }
}