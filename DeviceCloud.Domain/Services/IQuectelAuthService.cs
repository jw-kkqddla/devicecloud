namespace DeviceCloud.Domain.Services;

/// <summary>
/// 认证领域服务接口
/// </summary>
public interface IQuectelAuthService
{
    /// <summary>
    /// 使用 AccessKey 登录
    /// </summary>
    Task<Entities.QuectelToken> LoginWithAccessKeyAsync();
}
