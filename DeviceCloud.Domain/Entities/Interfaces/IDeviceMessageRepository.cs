using DeviceCloud.Domain.Entities;

namespace DeviceCloud.Domain.Interfaces;

public interface IDeviceMessageRepository
{
    Task<(IReadOnlyList<DeviceMessage> Records, string? NextToken)>
        GetMessagesAsync(string? nextToken = null, int limit = 20);

    Task<IReadOnlyList<PropertyDefinition>> GetPropertyDefinitionsAsync();
}