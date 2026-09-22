using DeviceCloud.Domain.Entities;
using DeviceCloud.Domain.Repositories;

namespace DeviceCloud.Infrastructure.Repositories;

/// <summary>
/// 内存令牌仓储实现（生产环境应使用分布式缓存如 Redis）
/// </summary>
public class MemoryTokenRepository : ITokenRepository
{
    private QuectelToken? _token;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public Task<QuectelToken?> GetAsync()
    {
        return Task.FromResult(_token);
    }

    public Task SaveAsync(QuectelToken token)
    {
        _token = token;
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        _token = null;
        return Task.CompletedTask;
    }

    public async Task<string?> TryAcquireRefreshLockAsync(TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var acquired = await _refreshLock.WaitAsync(0, cancellationToken);
        return acquired ? Guid.NewGuid().ToString("N") : null;
    }

    public Task ReleaseRefreshLockAsync(string lockToken)
    {
        _refreshLock.Release();
        return Task.CompletedTask;
    }
}
