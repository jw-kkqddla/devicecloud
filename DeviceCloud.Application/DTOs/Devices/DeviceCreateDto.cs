using System.Text.Json.Serialization;

namespace DeviceCloud.Application.DTOs.Devices;

/// <summary>
/// 创建设备请求
/// </summary>
public class DeviceCreateRequest
{
    /// <summary>
    /// 产品Key
    /// </summary>
    [JsonPropertyName("productKey")]
    public string ProductKey { get; set; } = string.Empty;

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
    /// 序列号
    /// </summary>
    [JsonPropertyName("sn")]
    public string? Sn { get; set; }

    /// <summary>
    /// 认证模式
    /// </summary>
    [JsonPropertyName("authMode")]
    public int? AuthMode { get; set; }

    /// <summary>
    /// 预共享密钥
    /// </summary>
    [JsonPropertyName("psk")]
    public string? Psk { get; set; }

    /// <summary>
    /// 指纹
    /// </summary>
    [JsonPropertyName("fingerPrint")]
    public string? FingerPrint { get; set; }
}

/// <summary>
/// 创建设备响应
/// </summary>
public class DeviceCreateResponse
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
    /// 响应数据
    /// </summary>
    [JsonPropertyName("data")]
    public DeviceCreateData? Data { get; set; }
}

/// <summary>
/// 创建设备数据
/// </summary>
public class DeviceCreateData
{
    /// <summary>
    /// 设备Key
    /// </summary>
    [JsonPropertyName("deviceKey")]
    public string? DeviceKey { get; set; }

    /// <summary>
    /// 设备密钥
    /// </summary>
    [JsonPropertyName("deviceSecret")]
    public string? DeviceSecret { get; set; }
}

/// <summary>
/// 删除设备请求
/// </summary>
public class DeviceDeleteRequest
{
    /// <summary>
    /// 产品Key
    /// </summary>
    [JsonPropertyName("productKey")]
    public string ProductKey { get; set; } = string.Empty;

    /// <summary>
    /// 设备Key
    /// </summary>
    [JsonPropertyName("deviceKey")]
    public string DeviceKey { get; set; } = string.Empty;
}

/// <summary>
/// 删除设备响应
/// </summary>
public class DeviceDeleteResponse
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
}

/// <summary>
/// 更新设备请求
/// </summary>
public class DeviceUpdateRequest
{
    /// <summary>
    /// 产品Key
    /// </summary>
    [JsonPropertyName("productKey")]
    public string ProductKey { get; set; } = string.Empty;

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
    /// 序列号
    /// </summary>
    [JsonPropertyName("sn")]
    public string? Sn { get; set; }
}

/// <summary>
/// 更新设备响应
/// </summary>
public class DeviceUpdateResponse
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
}
