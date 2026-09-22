namespace DeviceCloud.Domain.Services.Subscriptions;

/// <summary>
/// 消息订阅领域服务接口
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// 获取队列列表
    /// </summary>
    Task<string> GetQueuesAsync();
}
