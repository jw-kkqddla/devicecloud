using DeviceCloud.Application.Common;
using DeviceCloud.Application.DTOs;
using DeviceCloud.Application.Interfaces;
using DeviceCloud.Domain.Repositories;
using DeviceCloud.Domain.Services;

namespace DeviceCloud.Application.Services;

/// <summary>
/// 移远应用服务实现
/// </summary>
public class QuectelAppService(
    IQuectelAuthService authService,
    ITokenRepository tokenRepository) : IQuectelAppService
{
    public async Task<ApiResponse<TokenData>> LoginAsync()
    {
        try
        {
            var token = await authService.LoginWithAccessKeyAsync();
            await tokenRepository.SaveAsync(token);

            var data = new TokenData
            {
                AccessToken = token.AccessToken,
                TokenType = token.TokenType,
                ExpiresIn = token.ExpiresIn,
                RefreshToken = token.RefreshToken,
                ExpiresAt = token.ExpiresAt
            };

            return ApiResponse.Ok(data, "登录成功");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail<TokenData>(ex.Message, "AUTH_ERROR");
        }
    }

    public async Task<ApiResponse<TokenData>?> GetCurrentTokenAsync()
    {
        var token = await tokenRepository.GetAsync();
        if (token == null)
            return null;

        if (token.IsExpired)
            return ApiResponse.Fail<TokenData>("令牌已过期", "TOKEN_EXPIRED");

        var data = new TokenData
        {
            AccessToken = token.AccessToken,
            TokenType = token.TokenType,
            ExpiresIn = token.ExpiresIn,
            RefreshToken = token.RefreshToken,
            ExpiresAt = token.ExpiresAt
        };

        return ApiResponse.Ok(data);
    }
}
