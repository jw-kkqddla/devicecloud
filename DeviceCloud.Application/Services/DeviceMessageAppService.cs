using DeviceCloud.Application.DTOs;
using DeviceCloud.Application.Interfaces;
using DeviceCloud.Domain.Entities;
using DeviceCloud.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DeviceCloud.Application.Services;

public class DeviceMessageAppService(
    IDeviceMessageRepository repository,
    ILogger<DeviceMessageAppService> logger)
    : IDeviceMessageAppService
{
    private IReadOnlyList<PropertyDefinition>? _properties;
    private Dictionary<int, PropertyDefinition>? _propertyMap;

    public async Task<DeviceMessageResult> GetFormattedMessagesAsync(string? nextToken = null)
    {
        await EnsurePropertiesLoadedAsync();

        // 1. 获取数据
        var (records, newNextToken) = await repository.GetMessagesAsync(nextToken);

        // 2. 构建展示列表
        var displayList = BuildDisplayList(records);

        // 3. 计算统计
        var stats = CalculateStats(records);

        return new DeviceMessageResult(
            displayList,
            stats,
            newNextToken,
            !string.IsNullOrEmpty(newNextToken)
        );
    }

    private async Task EnsurePropertiesLoadedAsync()
    {
        if (_properties == null)
        {
            _properties = await repository.GetPropertyDefinitionsAsync();
            _propertyMap = _properties.ToDictionary(p => p.Id, p => p);
            logger.LogInformation("加载了 {Count} 个属性定义", _properties.Count);
        }
    }

    private List<MessageDisplayDto> BuildDisplayList(IReadOnlyList<DeviceMessage> records)
    {
        var result = new List<MessageDisplayDto>();
        var index = 1;

        foreach (var record in records)
        {
            var dmItems = record.ParseDmData();
            var isUp = record.MsgType == Domain.Enums.MessageType.Up;

            if (dmItems.Count != 0)
            {
                result.AddRange(dmItems.Select(item =>
                {
                    var property = _propertyMap!.GetValueOrDefault(item.Id);
                    return new MessageDisplayDto(
                        index++,
                        isUp ? "上行" : "下行",
                        item.Id,
                        property?.Name ?? $"未知属性({item.Id})",
                        property?.Code ?? $"unknown_{item.Id}",
                        item.Value,
                        property?.FormatDisplayValue(item.Value) ?? item.Value ?? "—",
                        record.GetCreateTimeLocal(),
                        record.Ticket,
                        isUp
                    );
                }));
            }
            else
            {
                result.Add(new MessageDisplayDto(
                    index++,
                    isUp ? "上行" : "下行",
                    0,
                    "无数据",
                    "",
                    null,
                    "—",
                    record.GetCreateTimeLocal(),
                    record.Ticket,
                    isUp
                ));
            }
        }

        return result;
    }

    private MessageStatsDto CalculateStats(IReadOnlyList<DeviceMessage> records)
    {
        var stats = new MessageStatsDto
        {
            Total = records.Count,
            UpCount = records.Count(r => r.MsgType == Domain.Enums.MessageType.Up),
            DownCount = records.Count(r => r.MsgType == Domain.Enums.MessageType.Down)
        };

        foreach (var record in records)
        {
            var dmItems = record.ParseDmData();
            foreach (var item in dmItems)
            {
                var property = _propertyMap!.GetValueOrDefault(item.Id);
                var key = property?.Name ?? $"属性{item.Id}";
                stats.PropertyCounts.TryGetValue(key, out var count);
                stats.PropertyCounts[key] = count + 1;
            }
        }

        return stats;
    }
}