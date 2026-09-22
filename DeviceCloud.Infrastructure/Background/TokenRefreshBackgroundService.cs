using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DeviceCloud.Domain.Repositories;
using DeviceCloud.Domain.Services;
using DeviceCloud.Infrastructure.Options;

namespace DeviceCloud.Infrastructure.Background;

/// <summary>
/// Token 后台刷新服务 - 每小时自动刷新
/// </summary>
public class TokenRefreshBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TokenRefreshBackgroundService> _logger;
    private readonly TimeSpan _refreshInterval;

    public TokenRefreshBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<TokenRefreshBackgroundService> logger,
        IOptions<QuectelOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        // 默认 1 小时刷新一次
        _refreshInterval = TimeSpan.FromHours(1);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Token refresh background service started");

        try
        {
            // 启动后先等待一段时间，避免启动时立即刷新
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RefreshTokenAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error refreshing token in background");
                }

                // 等待下一次刷新
                await Task.Delay(_refreshInterval, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // 应用停止时正常退出
        }

        _logger.LogInformation("Token refresh background service stopped");
    }

    /// <summary>
    /// 刷新 Token
    /// </summary>
    private async Task RefreshTokenAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var tokenRepository = scope.ServiceProvider.GetRequiredService<ITokenRepository>();
        var authService = scope.ServiceProvider.GetRequiredService<IQuectelAuthService>();

        var existingToken = await tokenRepository.GetAsync();

        // 如果没有 Token 或即将过期（30分钟内），则刷新
        if (existingToken == null || existingToken.ExpiresAt < DateTime.UtcNow.AddMinutes(30))
        {
            var lockToken = await tokenRepository.TryAcquireRefreshLockAsync(
                TimeSpan.FromSeconds(60), cancellationToken);

            if (lockToken == null)
            {
                _logger.LogDebug("Token refresh in progress by another instance, skip");
                return;
            }

            try
            {
                existingToken = await tokenRepository.GetAsync();
                if (existingToken != null && existingToken.ExpiresAt >= DateTime.UtcNow.AddMinutes(30))
                {
                    _logger.LogDebug("Token already refreshed, skip");
                    return;
                }

                _logger.LogInformation("Refreshing token... Current status: {Status}",
                    existingToken == null ? "No token" : "Expiring soon");

                var newToken = await authService.LoginWithAccessKeyAsync();
                await tokenRepository.SaveAsync(newToken);

                _logger.LogInformation("Token refreshed successfully. Expires at: {ExpiresAt}",
                    newToken.ExpiresAt.ToLocalTime());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh token");
            }
            finally
            {
                await tokenRepository.ReleaseRefreshLockAsync(lockToken);
            }
        }
        else
        {
            _logger.LogDebug("Token still valid, skipping refresh. Expires at: {ExpiresAt}",
                existingToken.ExpiresAt.ToLocalTime());
        }
    }
}
