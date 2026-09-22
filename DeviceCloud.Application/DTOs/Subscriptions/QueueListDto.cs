using System.Text.Json.Serialization;

namespace DeviceCloud.Application.DTOs.Subscriptions;

/// <summary>
/// 队列信息
/// </summary>
public class QueueListDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("queueName")]
    public string QueueName { get; set; } = string.Empty;

    [JsonPropertyName("queueDesc")]
    public string? QueueDesc { get; set; }

    [JsonPropertyName("metadata")]
    public string? Metadata { get; set; }

    [JsonPropertyName("userId")]
    public long UserId { get; set; }

    [JsonPropertyName("createTime")]
    public long CreateTime { get; set; }

    [JsonPropertyName("updateTime")]
    public long? UpdateTime { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("entUserId")]
    public long EntUserId { get; set; }

    [JsonPropertyName("isFree")]
    public int IsFree { get; set; }

    /// <summary>
    /// 创建时间本地时间
    /// </summary>
    public DateTime CreateTimeLocal => DateTimeOffset.FromUnixTimeMilliseconds(CreateTime).LocalDateTime;

    /// <summary>
    /// 更新时间本地时间
    /// </summary>
    public DateTime? UpdateTimeLocal => UpdateTime.HasValue
        ? DateTimeOffset.FromUnixTimeMilliseconds(UpdateTime.Value).LocalDateTime
        : null;

    /// <summary>
    /// 状态显示
    /// </summary>
    public string StatusDisplay => Status switch
    {
        0 => "正常",
        _ => "未知"
    };

    /// <summary>
    /// 状态徽章颜色
    /// </summary>
    public string StatusBadge => Status switch
    {
        0 => "success",
        _ => "secondary"
    };
}
