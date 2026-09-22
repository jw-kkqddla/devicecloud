using System.Text.Json.Serialization;

namespace DeviceCloud.Application.DTOs.Devices;

/// <summary>
/// 设备物模型数据项
/// </summary>
public class DeviceDataItem
{
    /// <summary>
    /// 设备Key
    /// </summary>
    [JsonPropertyName("deviceKey")]
    public string DeviceKey { get; set; } = string.Empty;

    /// <summary>
    /// 设备名称
    /// </summary>
    [JsonPropertyName("deviceName")]
    public string? DeviceName { get; set; }

    /// <summary>
    /// 物模型数据列表
    /// </summary>
    [JsonPropertyName("properties")]
    public List<DevicePropertyData>? Properties { get; set; }
}

/// <summary>
/// 设备属性数据
/// </summary>
public class DevicePropertyData
{
    /// <summary>
    /// 属性标识符
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 属性名称
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 属性值
    /// </summary>
    [JsonPropertyName("value")]
    public object? Value { get; set; }

    /// <summary>
    /// 数据类型
    /// </summary>
    [JsonPropertyName("dataType")]
    public string? DataType { get; set; }

    /// <summary>
    /// 时间戳（毫秒）
    /// </summary>
    [JsonPropertyName("time")]
    public long? Time { get; set; }

    /// <summary>
    /// 获取时间的本地时间
    /// </summary>
    public DateTime? TimeLocal =>
        Time.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(Time.Value).LocalDateTime : null;
}

/// <summary>
/// 读取设备物模型数据API响应
/// </summary>
public class ReadDeviceDataResponse
{
    /// <summary>
    /// 响应码（200表示成功）
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    [JsonPropertyName("msg")]
    public string? Msg { get; set; }

    /// <summary>
    /// 设备数据列表
    /// </summary>
    [JsonPropertyName("data")]
    public List<DeviceDataItem>? Data { get; set; }
}
