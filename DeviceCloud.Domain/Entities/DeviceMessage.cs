using DeviceCloud.Domain.Enums;
using DeviceCloud.Domain.ValueObjects;
using System.Text.Json;

namespace DeviceCloud.Domain.Entities;

public class DeviceMessage
{
    public string? Data { get; set; }
    public string? DmData { get; set; }
    public string? ExtData { get; set; }
    public string? SourceType { get; set; }
    public MessageType MsgType { get; set; }
    public int SendStatus { get; set; }
    public string Ticket { get; set; } = string.Empty;
    public string? Cmd { get; set; }
    public long CreateTime { get; set; }
    public long? SendTime { get; set; }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // 领域行为：解析dmData
    public List<DmDataItem> ParseDmData()
    {
        if (string.IsNullOrEmpty(DmData))
            return [];

        try
        {
            return MsgType switch
            {
                MessageType.Up => ParseUpMessage(),
                MessageType.Down => ParseDownMessage(),
                _ => []
            };
        }
        catch (JsonException ex)
        {
            // 使用.NET 9的日志记录
            Console.WriteLine($"解析dmData失败: {ex.Message}");
            return [];
        }
    }

    private List<DmDataItem> ParseUpMessage()
    {
        var items = JsonSerializer.Deserialize<List<DmDataItem>>(DmData!, _jsonOptions);
        return items ?? [];
    }

    private List<DmDataItem> ParseDownMessage()
    {
        var idList = JsonSerializer.Deserialize<List<string>>(DmData!, _jsonOptions);
        return idList?
            .Where(id => int.TryParse(id, out _))
            .Select(id => new DmDataItem { Id = int.Parse(id), Value = null })
            .ToList() ?? [];
    }

    // 获取创建时间（本地时间）
    public DateTime GetCreateTimeLocal()
        => DateTimeOffset.FromUnixTimeMilliseconds(CreateTime).LocalDateTime;
}