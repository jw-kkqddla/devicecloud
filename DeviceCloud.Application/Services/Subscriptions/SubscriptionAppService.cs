using System.Text.Json;
using DeviceCloud.Application.Common;
using DeviceCloud.Application.DTOs.Subscriptions;
using DeviceCloud.Application.Interfaces.Subscriptions;
using DeviceCloud.Domain.Services.Subscriptions;

namespace DeviceCloud.Application.Services.Subscriptions;

/// <summary>
/// 消息订阅应用服务实现
/// </summary>
public class SubscriptionAppService : ISubscriptionAppService
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionAppService(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    public async Task<ApiResponse<List<QueueListDto>>> GetQueuesAsync()
    {
        try
        {
            var jsonContent = await _subscriptionService.GetQueuesAsync();

            var response = JsonSerializer.Deserialize<QueueListResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (response == null)
            {
                return ApiResponse.Fail<List<QueueListDto>>("API 响应解析失败", "PARSE_ERROR");
            }

            if (response.Code != 200)
            {
                return ApiResponse.Fail<List<QueueListDto>>(response.Msg ?? "获取队列列表失败", "API_ERROR");
            }

            return ApiResponse.Ok(response.Data ?? new List<QueueListDto>(), "获取队列列表成功");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail<List<QueueListDto>>(ex.Message, "API_ERROR");
        }
    }
}
