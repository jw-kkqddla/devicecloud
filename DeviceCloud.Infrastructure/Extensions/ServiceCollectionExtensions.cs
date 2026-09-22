using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DeviceCloud.Domain.Repositories;
using DeviceCloud.Domain.Services;
using DeviceCloud.Domain.Services.Devices;
using DeviceCloud.Domain.Services.Products;
using DeviceCloud.Domain.Services.Subscriptions;
using DeviceCloud.Infrastructure.Background;
using DeviceCloud.Infrastructure.Http;
using DeviceCloud.Infrastructure.Options;
using DeviceCloud.Infrastructure.Repositories;
using DeviceCloud.Infrastructure.Services;
using DeviceCloud.Infrastructure.Services.Devices;
using DeviceCloud.Infrastructure.Services.Products;
using DeviceCloud.Infrastructure.Services.Subscriptions;
using StackExchange.Redis;

namespace DeviceCloud.Infrastructure.Extensions;

/// <summary>
/// Infrastructure 层依赖注入扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 配置选项
        services.Configure<QuectelOptions>(
            configuration.GetSection(QuectelOptions.SectionName));
        services.Configure<RedisOptions>(
            configuration.GetSection(RedisOptions.SectionName));

        // 注册 Redis
        var redisConnectionString = configuration.GetSection("Redis:ConnectionString")?.Value ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            try
            {
                return ConnectionMultiplexer.Connect(redisConnectionString);
            }
            catch
            {
                // Redis 连接失败时返回 null，使用内存存储
                return null!;
            }
        });

        // 注册缓存服务（Redis 优先，不可用时降级为不缓存）
        services.AddSingleton<ICacheService>(sp =>
        {
            var redis = sp.GetService<IConnectionMultiplexer>();
            if (redis != null)
            {
                return new RedisCacheService(
                    redis,
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RedisCacheService>>());
            }
            return new NoOpCacheService();
        });

        // 注册仓储（Redis 优先，不可用时使用内存）
        services.AddSingleton<ITokenRepository>(sp =>
        {
            var redis = sp.GetService<IConnectionMultiplexer>();
            if (redis != null)
            {
                return new RedisTokenRepository(
                    redis,
                    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RedisOptions>>(),
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RedisTokenRepository>>());
            }
            // 回退到内存存储
            return new MemoryTokenRepository();
        });

        // 注册领域服务实现
        services.AddScoped<ISignatureService, SignatureService>();
        services.AddScoped<IQuectelAuthService, QuectelAuthApiService>();

        // 注册 HttpClient
        services.AddTransient<AuthHeaderHandler>();

        // 登录服务 - 不需要 Token，直接调用
        services.AddHttpClient<QuectelAuthApiService>();

        // 业务 API 服务 - 需要自动注入 Token
        // 使用命名客户端确保 Handler 被正确注入
        services.AddHttpClient<ProductApiService>()
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddHttpClient<DeviceApiService>()
            .AddHttpMessageHandler<AuthHeaderHandler>();

        // 设备云通用 API 客户端（用于访问 quecdatastorage 等额外接口）
        /*services.AddHttpClient<DeviceCloudApiClient>()
            .AddHttpMessageHandler<AuthHeaderHandler>();
        */

        services.AddHttpClient<SubscriptionApiService>()
            .AddHttpMessageHandler<AuthHeaderHandler>();

        // 注册接口到实现的映射
        services.AddScoped<IProductService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(ProductApiService));
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<QuectelOptions>>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ProductApiService>>();
            return new ProductApiService(httpClient, options, logger);
        });

        services.AddScoped<IDeviceService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(DeviceApiService));
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<QuectelOptions>>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<DeviceApiService>>();
            return new DeviceApiService(httpClient, options, logger);
        });

        services.AddScoped<ISubscriptionService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(SubscriptionApiService));
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<QuectelOptions>>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SubscriptionApiService>>();
            return new SubscriptionApiService(httpClient, options, logger);
        });

        // 注册后台服务（Token 自动刷新）
        services.AddHostedService<TokenRefreshBackgroundService>();

        return services;
    }
}
