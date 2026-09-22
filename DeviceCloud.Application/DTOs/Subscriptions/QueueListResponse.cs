using System.Text.Json.Serialization;

namespace DeviceCloud.Application.DTOs.Subscriptions;

/// <summary>
/// 队列列表响应
/// </summary>
public class QueueListResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("msg")]
    public string? Msg { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("pageNum")]
    public int PageNum { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("pages")]
    public int Pages { get; set; }

    [JsonPropertyName("data")]
    public List<QueueListDto>? Data { get; set; }
}
