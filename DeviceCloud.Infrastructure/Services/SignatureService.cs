using System.Security.Cryptography;
using System.Text;
using DeviceCloud.Domain.Services;

namespace DeviceCloud.Infrastructure.Services;

/// <summary>
/// 签名服务实现
/// </summary>
public class SignatureService : ISignatureService
{
    public string ComputeSha256Signature(string data)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(bytes).ToLower();
    }
}
