namespace DeviceCloud.Domain.Services.Protocols.Text.ModuleParsers;

/// <summary>
/// HBMS3（16 节电芯电压）模块解析器。协议 5.13。
/// 响应格式：(AAAA BBBB CCCC DDDD EEEE FFFF GGGG HHHH IIII JJJJ KKKK LLLL MMMM NNNN PPPP QQQQ OOOOOOOO
/// </summary>
public class Hbms3Parser : IModuleParser
{
    public string Code => "HBMS3";

    public IReadOnlyDictionary<string, string> Parse(string rawValue)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(rawValue) || TextFieldParser.IsCrcError(rawValue))
            return result;

        var v = TextFieldParser.SplitFields(rawValue);

        try
        {
            result["电池1电压"] = TextFieldParser.NumberWithUnit(v[0].TrimStart('('), "MV");
            result["电池2电压"] = TextFieldParser.NumberWithUnit(v[1], "MV");
            result["电池3电压"] = TextFieldParser.NumberWithUnit(v[2], "MV");
            result["电池4电压"] = TextFieldParser.NumberWithUnit(v[3], "MV");
            result["电池5电压"] = TextFieldParser.NumberWithUnit(v[4], "MV");
            result["电池6电压"] = TextFieldParser.NumberWithUnit(v[5], "MV");
            result["电池7电压"] = TextFieldParser.NumberWithUnit(v[6], "MV");
            result["电池8电压"] = TextFieldParser.NumberWithUnit(v[7], "MV");
            result["电池9电压"] = TextFieldParser.NumberWithUnit(v[8], "MV");
            result["电池10电压"] = TextFieldParser.NumberWithUnit(v[9], "MV");
            result["电池11电压"] = TextFieldParser.NumberWithUnit(v[10], "MV");
            result["电池12电压"] = TextFieldParser.NumberWithUnit(v[11], "MV");
            result["电池13电压"] = TextFieldParser.NumberWithUnit(v[12], "MV");
            result["电池14电压"] = TextFieldParser.NumberWithUnit(v[13], "MV");
            result["电池15电压"] = TextFieldParser.NumberWithUnit(v[14], "MV");
            result["电池16电压"] = TextFieldParser.NumberWithUnit(v[15], "MV");
        }
        catch (Exception)
        {
            // 忽略解析异常
        }

        return result;
    }
}
