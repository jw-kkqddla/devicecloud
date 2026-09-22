namespace DeviceCloud.Domain.Services.Protocols.Text.ModuleParsers;

/// <summary>
/// HPVB（第二路 PV）模块解析器。协议 5.9（双 PV 机型）。
/// 响应格式与 HPV 同构，字段为 PV2 电压/电流/功率，用于与 HPV 的 PV 功率相加得到发电功率。
/// </summary>
public class HpvbParser : IModuleParser
{
    public string Code => "HPVB";

    public IReadOnlyDictionary<string, string> Parse(string rawValue)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(rawValue) || TextFieldParser.IsCrcError(rawValue))
            return result;

        var v = TextFieldParser.SplitFields(rawValue);

        try
        {
            result["PV2电压"] = TextFieldParser.NumberWithUnit(v[0].TrimStart('('), "V");
            result["PV2电流"] = TextFieldParser.NumberWithUnit(v[1], "A");
            result["PV2功率"] = TextFieldParser.NumberWithUnit(v[2], "W");
        }
        catch (Exception)
        {
            // 忽略解析异常
        }

        return result;
    }
}
