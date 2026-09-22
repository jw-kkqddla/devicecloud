using DeviceCloud.Domain.Services;

namespace DeviceCloud.Infrastructure.Services;

/// <summary>
/// 空缓存服务（Redis 不可用时的降级实现，不缓存，直接透传）
/// </summary>
public class NoOpCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        => Task.FromResult<T?>(default);

    public Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
