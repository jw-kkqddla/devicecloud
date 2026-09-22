using Microsoft.Extensions.Logging;
using DeviceCloud.Domain.Repositories;
using DeviceCloud.Domain.Services;

namespace DeviceCloud.Infrastructure.Http;

/// <summary>
/// 认证头处理器 - 自动注入 Token
/// </summary>
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ITokenRepository _tokenRepository;
    private readonly IQuectelAuthService _authService;
    private readonly ILogger<AuthHeaderHandler> _logger;

    public AuthHeaderHandler(
        ITokenRepository tokenRepository,
        IQuectelAuthService authService,
        ILogger<AuthHeaderHandler> logger)
    {
        _tokenRepository = tokenRepository;
        _authService = authService;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("AuthHeaderHandler executing for: {Url}", request.RequestUri);

        // 确保请求有 Authorization 头
        var token = await GetOrCreateTokenAsync(cancellationToken);

        if (token != null)
        {
            // 平台的 access_token 已包含前缀（如 "QJWT xxx"），直接作为 Authorization header
            request.Headers.TryAddWithoutValidation("Authorization", token.AccessToken);

            _logger.LogDebug("Authorization header added: {TokenPrefix}...",
                token.AccessToken.Substring(0, Math.Min(20, token.AccessToken.Length)));
        }
        else
        {
            _logger.LogWarning("No token available for request");
        }

        return await base.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// 获取或创建 Token
    /// </summary>
    private async Task<Domain.Entities.QuectelToken?> GetOrCreateTokenAsync(CancellationToken cancellationToken)
    {
        var token = await _tokenRepository.GetAsync();

        // Token 有效则直接返回
        if (token != null && !token.IsExpired)
        {
            return token;
        }

        var lockToken = await _tokenRepository.TryAcquireRefreshLockAsync(
            TimeSpan.FromSeconds(30), cancellationToken);

        if (lockToken == null)
        {
            _logger.LogDebug("Token refresh in progress by another instance, waiting...");
            await Task.Delay(500, cancellationToken);
            return await _tokenRepository.GetAsync();
        }

        try
        {
            token = await _tokenRepository.GetAsync();
            if (token != null && !token.IsExpired)
            {
                _logger.LogDebug("Token already refreshed by another instance");
                return token;
            }

            _logger.LogInformation("Token expired or missing, refreshing...");

            token = await _authService.LoginWithAccessKeyAsync();
            await _tokenRepository.SaveAsync(token);

            _logger.LogInformation("Token refreshed successfully");

            return token;
        }
        finally
        {
            await _tokenRepository.ReleaseRefreshLockAsync(lockToken);
        }
    }
}
