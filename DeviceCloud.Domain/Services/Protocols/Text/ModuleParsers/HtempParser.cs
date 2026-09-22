namespace DeviceCloud.Domain.Services.Protocols.Text.ModuleParsers;

/// <summary>
/// HTEMP（温度）模块解析器。协议 5.9。
/// 响应格式：(AAA BBB CCC DDD EEE FFF GGG HI JJJ KKKOOOOOOOOOO
/// </summary>
public class HtempParser : IModuleParser
{
    public string Code => "HTEMP";

    private static readonly IReadOnlyDictionary<string, string> OnOffMap = new Dictionary<string, string>
    {
        ["0"] = "关", ["1"] = "开",
    };

    public IReadOnlyDictionary<string, string> Parse(string rawValue)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(rawValue) || TextFieldParser.IsCrcError(rawValue))
            return result;

        var v = TextFieldParser.SplitFields(rawValue);

        try
        {
            result["PV温度"] = TextFieldParser.NumberWithUnit(v[0].TrimStart('('), "℃");
            result["逆变温度"] = TextFieldParser.NumberWithUnit(v[1], "℃");
            result["升压温度"] = TextFieldParser.NumberWithUnit(v[2], "℃");
            result["变压器温度"] = TextFieldParser.NumberWithUnit(v[3], "℃");
            result["最高温度"] = TextFieldParser.NumberWithUnit(v[4], "℃");
            result["风扇1转速"] = TextFieldParser.NumberWithUnit(v[5], "%");
            result["风扇2转速"] = TextFieldParser.NumberWithUnit(v[6], "%");
            result["风扇1状态"] = TextFieldParser.Map(TextFieldParser.BitAt(v[7], 0), OnOffMap);
            result["风扇2状态"] = TextFieldParser.Map(TextFieldParser.BitAt(v[7], 1), OnOffMap);
            result["PV2温度"] = TextFieldParser.NumberWithUnit(v[8], "℃");
            result["DC整流温度"] = TextFieldParser.NumberWithUnit(TextFieldParser.Substring(v[9], 0, 3), "℃");
        }
        catch (Exception)
        {
            // 忽略解析异常
        }

        return result;
    }
}
