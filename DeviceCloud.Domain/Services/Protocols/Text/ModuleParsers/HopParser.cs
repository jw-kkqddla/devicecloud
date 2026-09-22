namespace DeviceCloud.Domain.Services.Protocols.Text.ModuleParsers;

/// <summary>
/// HOP（输出功率）模块解析器。协议 5.6。
/// 响应格式：(NNN.N MM.M AAAAA BBBBB CCC DDD EEEEE FFF.F OOOOOO
/// </summary>
public class HopParser : IModuleParser
{
    public string Code => "HOP";

    public IReadOnlyDictionary<string, string> Parse(string rawValue)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(rawValue) || TextFieldParser.IsCrcError(rawValue))
            return result;

        var v = TextFieldParser.SplitFields(rawValue);

        try
        {
            result["输出电压"] = TextFieldParser.NumberWithUnit(v[0].TrimStart('('), "V");
            result["输出频率"] = TextFieldParser.NumberWithUnit(v[1], "Hz");
            result["输出视在功率"] = TextFieldParser.NumberWithUnit(v[2], "VA");
            result["输出有功功率"] = TextFieldParser.NumberWithUnit(v[3], "W");
            result["输出负载百分比"] = TextFieldParser.NumberWithUnit(v[4], "%");
            result["输出直流分量"] = TextFieldParser.Number(v[5]);
            result["内部数据"] = TextFieldParser.Number(v[6]);
            result["电感电流"] = TextFieldParser.NumberWithUnit(v[7], "A");
        }
        catch (Exception)
        {
            // 忽略解析异常
        }

        return result;
    }
}
