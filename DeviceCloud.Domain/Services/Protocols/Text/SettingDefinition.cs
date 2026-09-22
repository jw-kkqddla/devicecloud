namespace DeviceCloud.Domain.Services.Protocols.Text;

/// <summary>
/// 设置项值的格式化方式。
/// </summary>
public enum ValueFormat
{
    None,          // 直接拼接
    FormatToXxx,   // XX.X（Tools.FormatToXxx）
    Pad3,          // 三位补零
    Pad4,          // 四位补零
    Pad5,          // 五位补零
}

/// <summary>
/// 设置项定义：code + 模块 + 指令前缀 + 枚举/格式。
/// </summary>
public sealed class SettingDefinition
{
    /// <summary>前端标识（与后端编码器对应）。</summary>
    public required string Code { get; init; }

    /// <summary>分组标识（HEEP1/HEEP2），用于前端分组展示。</summary>
    public required string Module { get; init; }

    /// <summary>指令前缀。</summary>
    public required string Prefix { get; init; }

    /// <summary>值格式（枚举项忽略）。</summary>
    public ValueFormat Format { get; init; } = ValueFormat.None;

    /// <summary>枚举映射（非空时为枚举项）。</summary>
    public IReadOnlyDictionary<string, string>? EnumMap { get; init; }

    /// <summary>是否为无值指令（如 PF、^S???CLE，前端用按钮触发）。</summary>
    public bool NoValue { get; init; }
}
