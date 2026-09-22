using DeviceCloud.Application.Common;
using DeviceCloud.Application.DTOs;

namespace DeviceCloud.Application.Interfaces;

/// <summary>
/// 移远应用服务接口
/// </summary>
public interface IQuectelAppService
{
    /// <summary>
    /// 登录获取令牌
    /// </summary>
    Task<ApiResponse<TokenData>> LoginAsync();

    /// <summary>
    /// 获取当前令牌状态
    /// </summary>
    Task<ApiResponse<TokenData>?> GetCurrentTokenAsync();
}
