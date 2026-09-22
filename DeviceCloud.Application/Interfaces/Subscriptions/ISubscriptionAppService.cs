using DeviceCloud.Application.Common;
using DeviceCloud.Application.DTOs.Subscriptions;

namespace DeviceCloud.Application.Interfaces.Subscriptions;

/// <summary>
/// 消息订阅应用服务接口
/// </summary>
public interface ISubscriptionAppService
{
    /// <summary>
    /// 获取队列列表
    /// </summary>
    Task<ApiResponse<List<QueueListDto>>> GetQueuesAsync();
}
