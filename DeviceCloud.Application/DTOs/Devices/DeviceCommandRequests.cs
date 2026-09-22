namespace DeviceCloud.Application.DTOs.Devices;

/// <summary>
/// 下发指令请求
/// </summary>
public class SendCommandRequest
{
    public string ProductKey { get; set; } = string.Empty;
    public string DeviceKey { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public string? Encode { get; set; } = "Text";
    public bool? IsCache { get; set; } = false;
    public bool? IsCover { get; set; } = false;
    public int? Qos { get; set; }
    public int? CacheTime { get; set; }
}

/// <summary>
/// 下发数据项
/// </summary>
public class SendDataItem
{
    public string Identifier { get; set; } = string.Empty;
    public object? Value { get; set; }
}

/// <summary>
/// 设置指令请求
/// </summary>
public class SendSettingRequest
{
    public string ProductKey { get; set; } = string.Empty;
    public string DeviceKey { get; set; } = string.Empty;
    public string Setting { get; set; } = string.Empty;   // WorkingMode / StrongChargeVoltage ...
    public string Value { get; set; } = string.Empty;      // SUB / 43.5 ...
    public string? MachineType { get; set; }               // 可选，单字母 A/B/C/D
    public bool? IsCache { get; set; } = false;
    public bool? IsCover { get; set; } = false;
    public int? Qos { get; set; }
    public int? CacheTime { get; set; }
}

/// <summary>
/// 查询历史数据（供前端图表调用）
/// </summary>
public class HistoryRequest
{
    public string ProductKey { get; set; } = string.Empty;
    public string DeviceKey { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;
    public long? StartTime { get; set; }
    public long? EndTime { get; set; }
    public int Limit { get; set; } = 100;
    public int Offset { get; set; } = 0;
}

/// <summary>
/// 读取设备数据请求
/// </summary>
public class ReadDeviceDataRequest
{
    public string ProductKey { get; set; } = string.Empty;
    public List<string> Devices { get; set; } = new();
    public List<string> Data { get; set; } = new();

    public string DataJson => System.Text.Json.JsonSerializer.Serialize(Data);
}
