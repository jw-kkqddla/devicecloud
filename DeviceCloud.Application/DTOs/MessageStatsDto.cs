namespace DeviceCloud.Application.DTOs;

public record MessageStatsDto(
    int Total,
    int UpCount,
    int DownCount,
    Dictionary<string, int> PropertyCounts
)
{
    public MessageStatsDto() : this(0, 0, 0, new Dictionary<string, int>()) { }
}