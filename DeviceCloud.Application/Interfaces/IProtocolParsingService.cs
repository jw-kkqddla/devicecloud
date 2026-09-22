namespace DeviceCloud.Application.Interfaces;

/// <summary>
/// 协议解析应用服务接口。
/// 把物模型属性（code = 模块名）的原始响应字符串解析成具体字段字典。
/// </summary>
public interface IProtocolParsingService
{
    /// <summary>
    /// 是否支持解析指定模块。
    /// </summary>
    bool Supports(string code);

    /// <summary>
    /// 解析模块原始响应。
    /// </summary>
    /// <param name="code">模块标识（对应物模型属性 code，如 "HSTS"）</param>
    /// <param name="rawValue">设备上报的原始响应字符串</param>
    /// <returns>字段名 → 显示值；不支持或解析失败返回空字典</returns>
    IReadOnlyDictionary<string, string> Parse(string code, string rawValue);
}
