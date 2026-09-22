namespace DeviceCloud.Domain.Services.Protocols.Text;

/// <summary>
/// 文本协议模块解析器接口。
/// 每个物模型属性（code = 模块名，如 HSTS/HEEP1）对应一个解析器，
/// 把该模块的原始响应字符串解析成「字段名 → 显示值」字典。
/// </summary>
public interface IModuleParser
{
    /// <summary>
    /// 模块标识（对应物模型属性 code，如 "HSTS"、"HEEP1"）。
    /// </summary>
    string Code { get; }

    /// <summary>
    /// 解析原始响应字符串。
    /// </summary>
    /// <param name="rawValue">模块的原始响应字符串（设备上报的 value）</param>
    /// <returns>字段名 → 显示值；解析失败返回空字典</returns>
    IReadOnlyDictionary<string, string> Parse(string rawValue);
}
