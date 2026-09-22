using DeviceCloud.Application.DTOs;

namespace DeviceCloud.Application.Interfaces;

public interface IDeviceMessageAppService
{
    Task<DeviceMessageResult> GetFormattedMessagesAsync(string? nextToken = null);
}

public record DeviceMessageResult(
    IReadOnlyList<MessageDisplayDto> Messages,
    MessageStatsDto Stats,
    string? NextToken,
    bool HasNextPage
);