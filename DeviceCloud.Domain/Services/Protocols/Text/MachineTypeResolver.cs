namespace DeviceCloud.Domain.Services.Protocols.Text;

/// <summary>
/// 设备型号识别。
/// 从 WpfApp1 MainWindowVM.AutoSelectedMachineType 移植（QPRTL 指令响应 → 机器类型）。
/// </summary>
public static class MachineTypeResolver
{
    /// <summary>
    /// 从 QPRTL 原始响应识别机器类型。
    /// </summary>
    /// <param name="rawResponse">QPRTL 指令的原始响应字符串</param>
    /// <returns>机器类型标识（HPVINV02/HPVINV04/.../BMS01/...）；无法识别返回 null</returns>
    public static string? Resolve(string? rawResponse)
    {
        if (string.IsNullOrEmpty(rawResponse))
            return null;

        // 原实现用固定 9 位 Substring 比较，此处用 StartsWith 更健壮
        if (rawResponse.StartsWith("(HPVINV01")) return "HPVINV01";
        if (rawResponse.StartsWith("(HPVINV02")) return "HPVINV02";
        if (rawResponse.StartsWith("(HPVINV03")) return "HPVINV03";
        if (rawResponse.StartsWith("(HPVINV04")) return "HPVINV04";
        if (rawResponse.StartsWith("(HPVINV05")) return "HPVINV05";
        if (rawResponse.StartsWith("(HPVINV06")) return "HPVINV06";
        if (rawResponse.StartsWith("(HPVINV07")) return "HPVINV07";
        if (rawResponse.StartsWith("(HPVINV08")) return "HPVINV08";
        if (rawResponse.StartsWith("(HPVINV09")) return "HPVINV09";
        if (rawResponse.StartsWith("(HPVINV10")) return "HPVINV10";
        if (rawResponse.StartsWith("(LPVINV02")) return "LPVINV02";
        if (rawResponse.StartsWith("(UPSCYX01")) return "UPSCYX01";
        if (rawResponse.StartsWith("LB6")) return "LB6";
        if (rawResponse.StartsWith("(BMS00001")) return "BMS01";
        if (rawResponse.StartsWith("(BMS00002")) return "BMS02";
        if (rawResponse.StartsWith("CG000001")) return "CG000001";
        if (rawResponse.StartsWith("(BMS00003")) return "BMS03";

        return null;
    }

    /// <summary>
    /// Resolve the machine-type group for model-specific enum mapping.
    /// Returns "A" (A/C group) or "B" (B/D group); null if unrecognized.
    /// </summary>
    public static string? ResolveGroup(string? rawResponse)
        => Resolve(rawResponse) switch
        {
            "HPVINV01" or "HPVINV03" => "A",
            "HPVINV02" or "HPVINV04" or "HPVINV05" => "B",
            _ => null,
        };

    /// <summary>
    /// 是否为逆变器型号（HEEP1/HEEP2 文本协议）。
    /// </summary>
    public static bool IsInverterModel(string? model)
        => !string.IsNullOrEmpty(model) &&
           (model.StartsWith("HPVINV") || model.StartsWith("LPVINV") ||
            model == "UPSCYX01" || model == "LB6");

    /// <summary>
    /// 是否为 BMS 型号（Modbus RTU 协议）。
    /// </summary>
    public static bool IsBmsModel(string? model)
        => !string.IsNullOrEmpty(model) && model.StartsWith("BMS");
}
