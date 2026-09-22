using DeviceCloud.Domain.Services.Protocols.Common;

namespace DeviceCloud.Domain.Services.Protocols.Text;

/// <summary>
/// 文本协议字段解析辅助。
/// 从 WpfApp1 各模块 AnalyseStringToElement 的公共模式提取：
/// 按空格 split、Substring(j,1) 位提取、去前导零、拼单位、枚举查表。
/// </summary>
public static class TextFieldParser
{
    /// <summary>
    /// 判断原始响应是否为 CRC 异常（WpfApp1 约定以 "-1" 开头）。
    /// </summary>
    public static bool IsCrcError(string? rawValue)
        => !string.IsNullOrEmpty(rawValue) && rawValue.StartsWith("-1");

    /// <summary>
    /// 按空格分割原始响应。
    /// </summary>
    public static string[] SplitFields(string rawValue) => rawValue.Split(' ');

    /// <summary>
    /// 提取字段中某一位的字符（对应 WpfApp1 的 Values[i].Substring(j, 1)）。
    /// </summary>
    public static string BitAt(string field, int index)
        => index >= 0 && index < field.Length ? field.Substring(index, 1) : string.Empty;

    /// <summary>
    /// 提取子串（安全，越界返回空）。
    /// </summary>
    public static string Substring(string field, int start, int length)
        => field.Length >= start + length ? field.Substring(start, length) : string.Empty;

    /// <summary>
    /// 提取数值并去前导零（对应 Tools.RemoveLeadingZeros）。
    /// </summary>
    public static string Number(string field) => ProtocolTools.RemoveLeadingZeros(field);

    /// <summary>
    /// 提取数值、去前导零并拼接单位。
    /// </summary>
    public static string NumberWithUnit(string field, string unit)
        => $"{ProtocolTools.RemoveLeadingZeros(field)}{unit}";

    /// <summary>
    /// 枚举查表：value 命中返回映射文案，未命中返回原值。
    /// </summary>
    public static string Map(string? value, IReadOnlyDictionary<string, string> map)
    {
        if (value is null) return string.Empty;
        return map.TryGetValue(value, out var mapped) ? mapped : value;
    }

    /// <summary>
    /// 通用 0/1 布尔：0→否，1→是（未命中返回原值）。
    /// </summary>
    public static string YesNo(string value)
        => value switch { "0" => "否", "1" => "是", _ => value };
}
