namespace DeviceCloud.Domain.Services.Protocols.Text.ModuleParsers;

/// <summary>
/// HGRID（并网）模块解析器。协议 5.5。
/// 响应格式：(NNN.N MM.M AAA BBB CC DD EFFFFFOOOOOOOOOOOOOOOOO
/// </summary>
public class HgridParser : IModuleParser
{
    public string Code => "HGRID";

    private static readonly IReadOnlyDictionary<string, string> FlowMap = new Dictionary<string, string>
    {
        ["+"] = "输入至逆变器", ["-"] = "逆变器输入电网",
    };

    public IReadOnlyDictionary<string, string> Parse(string rawValue)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(rawValue) || TextFieldParser.IsCrcError(rawValue))
            return result;

        var v = TextFieldParser.SplitFields(rawValue);

        try
        {
            result["市电电压"] = TextFieldParser.NumberWithUnit(v[0].TrimStart('('), "V");
            result["市电频率"] = TextFieldParser.NumberWithUnit(v[1], "Hz");
            result["市电丢失电压高点"] = TextFieldParser.NumberWithUnit(v[2], "V");
            result["市电丢失电压低点"] = TextFieldParser.NumberWithUnit(v[3], "V");
            result["市电丢失频率高点"] = TextFieldParser.NumberWithUnit(v[4], "Hz");
            result["市电丢失频率低点"] = TextFieldParser.NumberWithUnit(v[5], "Hz");
            result["市电电流流向"] = TextFieldParser.Map(TextFieldParser.BitAt(v[6], 0), FlowMap);
            result["市电功率"] = TextFieldParser.NumberWithUnit(TextFieldParser.Substring(v[6], 1, 5), "W");
        }
        catch (Exception)
        {
            // 忽略解析异常
        }

        return result;
    }
}
