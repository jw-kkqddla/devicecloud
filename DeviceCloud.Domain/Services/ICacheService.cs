namespace DeviceCloud.Domain.Services;

/// <summary>
/// 分布式缓存服务接口
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// 获取缓存项
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置缓存项
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除缓存项
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 按前缀删除缓存项
    /// </summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}
