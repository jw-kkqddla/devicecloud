namespace DeviceCloud.Domain.Services.Protocols.Text.ModuleParsers;

/// <summary>
/// HBMS2（BMS 容量与最高最低电压）模块解析器。协议 5.12。
/// 响应格式：(AAAA.A BBBB.B C DDDD EEEE FFFF GGGG OOOOOOOOOOOOOOOOOOOOOO
/// </summary>
public class Hbms2Parser : IModuleParser
{
    public string Code => "HBMS2";

    private static readonly IReadOnlyDictionary<string, string> DisplayModeMap = new Dictionary<string, string>
    {
        ["0"] = "不显示BMS细节", ["1"] = "显示16个电芯数据", ["2"] = "全部显示",
    };

    public IReadOnlyDictionary<string, string> Parse(string rawValue)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(rawValue) || TextFieldParser.IsCrcError(rawValue))
            return result;

        var v = TextFieldParser.SplitFields(rawValue);

        try
        {
            result["剩余容量"] = TextFieldParser.NumberWithUnit(v[0].TrimStart('('), "A");
            result["标称容量"] = TextFieldParser.NumberWithUnit(v[1], "A");
            result["显示模式"] = TextFieldParser.Map(TextFieldParser.BitAt(v[2], 0), DisplayModeMap);
            result["最高电压"] = TextFieldParser.NumberWithUnit(v[3], "MV");
            result["最高电压电芯位置"] = TextFieldParser.Number(v[4]);
            result["最低电压"] = TextFieldParser.NumberWithUnit(v[5], "MV");
            result["最低电压电芯位置"] = TextFieldParser.Number(v[6]);
        }
        catch (Exception)
        {
            // 忽略解析异常
        }

        return result;
    }
}
