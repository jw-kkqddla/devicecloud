namespace DeviceCloud.Domain.Services.Protocols.Common;

/// <summary>
/// 协议解析通用工具。
/// 从 WpfApp1.Convert.Tools 移植，剥离 WPF 依赖（App.GetText、DependencyProperty）。
/// </summary>
public static class ProtocolTools
{
    /// <summary>
    /// 根据 0/1 返回开关状态（原文依赖多语言 App.GetText，此处固定返回中文）。
    /// </summary>
    public static string ConvertState(string? state)
    {
        if (string.IsNullOrEmpty(state)) return string.Empty;

        return state switch
        {
            "1" => "开启",
            "0" => "关闭",
            _ => state
        };
    }

    /// <summary>
    /// 去除数字字符串的前导零（支持正负号与小数点）。
    /// </summary>
    public static string RemoveLeadingZeros(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        // 检查是否含有 +、- 符号
        if (input.Contains('+') || input.Contains('-'))
        {
            string tag = input.Substring(0, 1);
            input = input.Substring(1);
            return $"{tag}{RemoveLeadingZerosCore(input)}";
        }

        return RemoveLeadingZerosCore(input);
    }

    private static string RemoveLeadingZerosCore(string input)
    {
        if (input.Contains('.'))
        {
            var parts = input.Split('.');
            string integerPart = parts[0].TrimStart('0');
            string decimalPart = parts.Length > 1 ? parts[1] : string.Empty;

            if (string.IsNullOrEmpty(integerPart))
                integerPart = "0";

            return $"{integerPart}.{decimalPart}";
        }

        string result = input.TrimStart('0');
        return string.IsNullOrEmpty(result) ? "0" : result;
    }

    /// <summary>
    /// 两位补零：60/6 → 60/06。
    /// </summary>
    public static string PadToTwoDigits(string input)
        => int.TryParse(input, out int number) ? number.ToString("D2") : string.Empty;

    /// <summary>
    /// 三位补零：60/6 → 060。
    /// </summary>
    public static string PadToThreeDigits(string input)
        => int.TryParse(input, out int number) ? number.ToString("D3") : string.Empty;

    /// <summary>
    /// 四位补零：60/6 → 0060/0006。
    /// </summary>
    public static string PadToFourDigits(string input)
        => int.TryParse(input, out int number) ? number.ToString("D4") : string.Empty;

    /// <summary>
    /// 五位补零：60/6 → 00060/00006。
    /// </summary>
    public static string PadToFiveDigits(string input)
        => int.TryParse(input, out int number) ? number.ToString("D5") : string.Empty;

    /// <summary>
    /// 格式化为两位整数 + 一位小数：XX/X → XX.X。
    /// </summary>
    public static string FormatToXxx(string input)
    {
        if (int.TryParse(input, out int integerValue))
        {
            return integerValue.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)
                .PadLeft(4, '0');
        }

        if (input.Contains('.'))
        {
            var parts = input.Split('.');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int whole) &&
                int.TryParse(parts[1], out int fraction))
            {
                return $"{whole:D2}.{fraction}0".Substring(0, 4);
            }
        }

        return string.Empty;
    }
}
