namespace DeviceCloud.Domain.Services;

/// <summary>
/// 签名服务领域服务接口
/// </summary>
public interface ISignatureService
{
    /// <summary>
    /// 计算 SHA256 签名
    /// </summary>
    string ComputeSha256Signature(string data);
}
