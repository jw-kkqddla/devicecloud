namespace DeviceCloud.Domain.Services.Protocols.Text.ModuleParsers;

/// <summary>
/// HEEP2 模块解析器。协议 5.16。
/// 响应格式：(A BBB.B CCC DDD.D EEE.E FFF.F G HHH.H III JJJ KKK LLMM NNPP OOOOOOOOOOOOOOOO
/// </summary>
public class Heep2Parser : IModuleParser
{
    public string Code => "HEEP2";

    private static readonly IReadOnlyDictionary<string, string> EnableMap = new Dictionary<string, string>
    {
        ["0"] = "失能", ["1"] = "使能",
    };

    public IReadOnlyDictionary<string, string> Parse(string rawValue)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(rawValue) || TextFieldParser.IsCrcError(rawValue))
            return result;

        var v = TextFieldParser.SplitFields(rawValue);

        try
        {
            result["并机模式"] = TextFieldParser.Map(TextFieldParser.BitAt(v[0], 1), EnableMap);
            result["并机模式关闭电压"] = TextFieldParser.NumberWithUnit(v[1], "V");
            result["并机模式关闭SOC"] = TextFieldParser.NumberWithUnit(v[2], "%");
            result["电池低电告警电压"] = TextFieldParser.NumberWithUnit(v[3], "V");
            result["返回市电模式电压"] = TextFieldParser.NumberWithUnit(v[4], "V");
            result["返回电池模式电压"] = TextFieldParser.NumberWithUnit(v[5], "V");
            result["电池均衡模式"] = TextFieldParser.Map(TextFieldParser.BitAt(v[6], 0), EnableMap);
            result["电池均衡电压"] = TextFieldParser.NumberWithUnit(v[7], "V");
            result["均衡时间"] = TextFieldParser.NumberWithUnit(v[8], "分钟");
            result["均衡超时"] = TextFieldParser.NumberWithUnit(v[9], "分钟");
            result["均衡间隔"] = TextFieldParser.NumberWithUnit(v[10], "天");

            // Values[11]：LLMM（市电充电开启/关闭时间，小时）
            result["市电充电开启时间"] = TextFieldParser.NumberWithUnit(TextFieldParser.Substring(v[11], 0, 2), "小时");
            result["市电充电关闭时间"] = TextFieldParser.NumberWithUnit(TextFieldParser.Substring(v[11], 2, 2), "小时");

            // Values[12]：NNPP（输出开启/关闭时间，小时）
            result["输出开启时间"] = TextFieldParser.NumberWithUnit(TextFieldParser.Substring(v[12], 0, 2), "小时");
            result["输出关闭时间"] = TextFieldParser.NumberWithUnit(TextFieldParser.Substring(v[12], 2, 2), "小时");
        }
        catch (Exception)
        {
            // 忽略解析异常
        }

        return result;
    }
}
