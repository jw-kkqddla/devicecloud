using System.Text.Json;
using Microsoft.Extensions.Options;
using DeviceCloud.Domain.Entities;
using DeviceCloud.Domain.Services;
using DeviceCloud.Domain.ValueObjects;
using DeviceCloud.Infrastructure.Options;

namespace DeviceCloud.Infrastructure.Services;

/// <summary>
/// 认证 API 服务实现
/// </summary>
public class QuectelAuthApiService(
    HttpClient httpClient,
    IOptions<QuectelOptions> options,
    ISignatureService signatureService) : IQuectelAuthService
{
    public async Task<QuectelToken> LoginWithAccessKeyAsync()
    {
        var opt = options.Value;
        var credential = new AccessKeyCredential(opt.AccessKey, opt.AccessSecret);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var usernameRaw = credential.BuildSignString(timestamp);
        var password = signatureService.ComputeSha256Signature(usernameRaw + credential.AccessSecret);
        var usernameEncoded = Uri.EscapeDataString(usernameRaw);

        var url = $"{opt.BaseUrl}/v2/quecauth/accessKeyAuthrize/accessKeyLogin" +
                  $"?username={usernameEncoded}" +
                  $"&password={password}" +
                  $"&grant_type=password";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Accept", "application/json");

        var response = await httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        var loginResult = JsonSerializer.Deserialize<QuectelLoginResponse>(content, JsonOptions)
            ?? throw new InvalidOperationException("登录响应解析失败");

        return QuectelToken.Create(
            loginResult.access_token,
            loginResult.token_type,
            loginResult.expires_in,
            loginResult.refresh_token);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private class QuectelLoginResponse
    {
        public string access_token { get; set; } = string.Empty;
        public string token_type { get; set; } = string.Empty;
        public int expires_in { get; set; }
        public string? refresh_token { get; set; }
    }
}
