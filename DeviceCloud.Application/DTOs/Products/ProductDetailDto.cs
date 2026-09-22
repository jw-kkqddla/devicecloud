using System.Text.Json.Serialization;

namespace DeviceCloud.Application.DTOs.Products;

/// <summary>
/// 产品详情
/// </summary>
public class ProductDetailDto
{
    /// <summary>
    /// 产品Key
    /// </summary>
    [JsonPropertyName("productKey")]
    public string ProductKey { get; set; } = string.Empty;

    /// <summary>
    /// 产品名称
    /// </summary>
    [JsonPropertyName("productName")]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 接入类型
    /// </summary>
    [JsonPropertyName("accessType")]
    public int AccessType { get; set; }

    /// <summary>
    /// 联网方式
    /// </summary>
    [JsonPropertyName("netWay")]
    public string? NetWay { get; set; }

    /// <summary>
    /// 数据格式
    /// </summary>
    [JsonPropertyName("dataFmt")]
    public int DataFmt { get; set; }

    /// <summary>
    /// Logo路径
    /// </summary>
    [JsonPropertyName("logoPath")]
    public string? LogoPath { get; set; }

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
    /// 连接平台
    /// </summary>
    [JsonPropertyName("connectPlatform")]
    public int ConnectPlatform { get; set; }

    /// <summary>
    /// 产品配置
    /// </summary>
    [JsonPropertyName("productConfig")]
    public ProductConfigDto? ProductConfig { get; set; }

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
    /// 获取接入类型显示名称
    /// </summary>
    public string AccessTypeDisplay => AccessType switch
    {
        1 => "网关产品",
        2 => "网关子设备",
        _ => "普通产品"
    };

    /// <summary>
    /// 获取数据格式显示名称
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
/// 产品配置
/// </summary>
public class ProductConfigDto
{
    /// <summary>
    /// AI能力状态
    /// </summary>
    [JsonPropertyName("aiCapabilityStatus")]
    public bool AiCapabilityStatus { get; set; }
}

/// <summary>
/// 产品详情API响应
/// </summary>
public class ProductDetailResponse
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
    /// 产品详情数据
    /// </summary>
    [JsonPropertyName("data")]
    public ProductDetailDto? Data { get; set; }
}
