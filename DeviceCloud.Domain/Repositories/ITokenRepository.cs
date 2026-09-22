namespace DeviceCloud.Domain.Repositories;

/// <summary>
/// 令牌仓储接口
/// </summary>
public interface ITokenRepository
{
    /// <summary>
    /// 获取当前令牌
    /// </summary>
    Task<Entities.QuectelToken?> GetAsync();

    /// <summary>
    /// 保存令牌
    /// </summary>
    Task SaveAsync(Entities.QuectelToken token);

    /// <summary>
    /// 清除令牌
    /// </summary>
    Task ClearAsync();

    Task<string?> TryAcquireRefreshLockAsync(TimeSpan expiry, CancellationToken cancellationToken = default);

    Task ReleaseRefreshLockAsync(string lockToken);
}
