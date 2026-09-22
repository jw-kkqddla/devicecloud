namespace DeviceCloud.Application.Interfaces;

/// <summary>
/// 指令编码应用服务接口。
/// 把设置项（枚举/数值）编码为含 CRC + 回车的透传下发指令（Hex 字符串）。
/// </summary>
public interface ICommandEncodingService
{
    /// <summary>
    /// 是否支持该设置项。
    /// </summary>
    bool SupportsSetting(string setting);

    /// <summary>
    /// 编码设置指令（HEEP1/HEEP2/特殊指令）。
    /// </summary>
    /// <param name="setting">设置项 code（如 "WorkingMode"、"StrongChargeVoltage"）</param>
    /// <param name="value">设置值（枚举或数值字符串）</param>
    /// <param name="machineType">单字母机型（A/B/C/D），仅工作模式/充电优先顺序枚举依赖它</param>
    /// <returns>Hex 指令字符串（ASCII 指令 + CRC16 + 0x0D），用于 raw/sendData 透传下发</returns>
    string EncodeSetting(string setting, string value, string machineType = "");

    /// <summary>
    /// 从 QPRTL 原始响应识别机型分组（"A"=A/C 组、"B"=B/D 组；无法识别返回 null）。
    /// </summary>
    string? ResolveMachineTypeGroup(string rawResponse);

    /// <summary>
    /// 从 QPRTL 原始响应识别具体机型（如 "HPVINV04"）。
    /// </summary>
    string? ResolveMachineModel(string rawResponse);
}
