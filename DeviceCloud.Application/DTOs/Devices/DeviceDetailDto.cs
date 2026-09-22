using System.Text.Json.Serialization;

namespace DeviceCloud.Application.DTOs.Devices;

/// <summary>
/// 设备详情
/// </summary>
public class DeviceDetailDto
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
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// 序列号
    /// </summary>
    [JsonPropertyName("sn")]
    public string? Sn { get; set; }

    /// <summary>
    /// 设备二维码
    /// </summary>
    [JsonPropertyName("deviceQrCode")]
    public string? DeviceQrCode { get; set; }

    /// <summary>
    /// 是否激活 (0: 未激活, 1: 已激活)
    /// </summary>
    [JsonPropertyName("isActived")]
    public int IsActived { get; set; }

    /// <summary>
    /// 激活时间（毫秒时间戳）
    /// </summary>
    [JsonPropertyName("activedTime")]
    public long? ActivedTime { get; set; }

    /// <summary>
    /// 是否认证 (0: 未认证, 1: 已认证)
    /// </summary>
    [JsonPropertyName("isVerified")]
    public int IsVerified { get; set; }

    /// <summary>
    /// 是否虚拟设备 (0: 否, 1: 是)
    /// </summary>
    [JsonPropertyName("isVirtual")]
    public int IsVirtual { get; set; }

    /// <summary>
    /// 数据格式
    /// </summary>
    [JsonPropertyName("dataFmt")]
    public int DataFmt { get; set; }

    /// <summary>
    /// 认证模式
    /// </summary>
    [JsonPropertyName("authMode")]
    public int AuthMode { get; set; }

    /// <summary>
    /// 设备状态 (0: 离线, 1: 在线)
    /// </summary>
    [JsonPropertyName("deviceStatus")]
    public int DeviceStatus { get; set; }

    /// <summary>
    /// 首次连接时间（毫秒时间戳）
    /// </summary>
    [JsonPropertyName("firstConnTime")]
    public long? FirstConnTime { get; set; }

    /// <summary>
    /// 最后连接时间（毫秒时间戳）
    /// </summary>
    [JsonPropertyName("lastConnTime")]
    public long? LastConnTime { get; set; }

    /// <summary>
    /// 最后离线时间（毫秒时间戳）
    /// </summary>
    [JsonPropertyName("lastOfflineTime")]
    public long? LastOfflineTime { get; set; }

    /// <summary>
    /// 创建时间（毫秒时间戳）
    /// </summary>
    [JsonPropertyName("createTime")]
    public long CreateTime { get; set; }

    /// <summary>
    /// 更新时间（毫秒时间戳）
    /// </summary>
    [JsonPropertyName("updateTime")]
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 时间偏移
    /// </summary>
    [JsonPropertyName("timeOffset")]
    public string? TimeOffset { get; set; }

    /// <summary>
    /// 时区ID
    /// </summary>
    [JsonPropertyName("timeZoneId")]
    public string? TimeZoneId { get; set; }

    /// <summary>
    /// 获取创建时间的本地时间
    /// </summary>
    public DateTime CreateTimeLocal =>
        DateTimeOffset.FromUnixTimeMilliseconds(CreateTime).LocalDateTime;

    /// <summary>
    /// 获取更新时间的本地时间
    /// </summary>
    public DateTime? UpdateTimeLocal =>
        UpdateTime.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(UpdateTime.Value).LocalDateTime : null;

    /// <summary>
    /// 获取激活时间的本地时间
    /// </summary>
    public DateTime? ActivedTimeLocal =>
        ActivedTime.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(ActivedTime.Value).LocalDateTime : null;

    /// <summary>
    /// 获取首次连接时间的本地时间
    /// </summary>
    public DateTime? FirstConnTimeLocal =>
        FirstConnTime.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(FirstConnTime.Value).LocalDateTime : null;

    /// <summary>
    /// 获取最后连接时间的本地时间
    /// </summary>
    public DateTime? LastConnTimeLocal =>
        LastConnTime.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(LastConnTime.Value).LocalDateTime : null;

    /// <summary>
    /// 获取最后离线时间的本地时间
    /// </summary>
    public DateTime? LastOfflineTimeLocal =>
        LastOfflineTime.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(LastOfflineTime.Value).LocalDateTime : null;

    /// <summary>
    /// 设备状态显示
    /// </summary>
    public string DeviceStatusDisplay => DeviceStatus switch
    {
        1 => "在线",
        _ => "离线"
    };

    /// <summary>
    /// 设备状态徽章样式
    /// </summary>
    public string DeviceStatusBadge => DeviceStatus switch
    {
        1 => "success",
        _ => "secondary"
    };

    /// <summary>
    /// 激活状态显示
    /// </summary>
    public string IsActivedDisplay => IsActived switch
    {
        1 => "已激活",
        _ => "未激活"
    };

    /// <summary>
    /// 数据格式显示
    /// </summary>
    public string DataFmtDisplay => DataFmt switch
    {
        1 => "自定义",
        2 => "透传",
        3 => "JSON",
        _ => "未知"
    };
}

/// <summary>
/// 设备详情API响应
/// </summary>
public class DeviceDetailResponse
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
    /// 设备详情数据
    /// </summary>
    [JsonPropertyName("data")]
    public DeviceDetailDto? Data { get; set; }
}
