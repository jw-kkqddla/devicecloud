namespace DeviceCloud.Domain.Services.Protocols.Text.ModuleParsers;

/// <summary>
/// HBMS1（BMS 信息）模块解析器。协议 5.11。
/// 响应格式：(AA b7b6b5b4b3b2b1b0c7c6c5c4c3c2c1c0 BBB.B CCC.C DDD.D EEE FFFF.F GGGG.G HHHHH OOOOOO
/// </summary>
public class Hbms1Parser : IModuleParser
{
    public string Code => "HBMS1";

    public IReadOnlyDictionary<string, string> Parse(string rawValue)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(rawValue) || TextFieldParser.IsCrcError(rawValue))
            return result;

        var v = TextFieldParser.SplitFields(rawValue);

        try
        {
            result["协议类型"] = TextFieldParser.Number(TextFieldParser.Substring(v[0], 1, 2));

            // Values[1]：b7b6b5b4b3b2b1b0c7c6c5c4c3c2c1c0（16 位标志）
            result["BMS通信正常"] = TextFieldParser.YesNo(Bit(v, 1, 0));
            result["BMS低电报警标志"] = TextFieldParser.YesNo(Bit(v, 1, 1));
            result["BMS低电故障标志"] = TextFieldParser.YesNo(Bit(v, 1, 2));
            result["BMS允许充电标志"] = TextFieldParser.YesNo(Bit(v, 1, 3));
            result["BMS允许放电标志"] = TextFieldParser.YesNo(Bit(v, 1, 4));
            result["BMS充电过流标志"] = TextFieldParser.YesNo(Bit(v, 1, 5));
            result["BMS放电过流标志"] = TextFieldParser.YesNo(Bit(v, 1, 6));
            result["BMS温度过低标志"] = TextFieldParser.YesNo(Bit(v, 1, 7));
            result["BMS温度过高标志"] = TextFieldParser.YesNo(Bit(v, 1, 8));

            result["BMS放电电压限制"] = TextFieldParser.NumberWithUnit(v[2], "V");
            result["BMS充电电压限制"] = TextFieldParser.NumberWithUnit(v[3], "V");
            result["BMS充电电流限制"] = TextFieldParser.NumberWithUnit(v[4], "A");
            result["BMS当前SOC"] = TextFieldParser.NumberWithUnit(v[5], "%");
            result["BMS充电电流"] = TextFieldParser.NumberWithUnit(v[6], "A");
            result["BMS放电电流"] = TextFieldParser.NumberWithUnit(v[7], "A");
            result["BMS平均温度"] = TextFieldParser.NumberWithUnit(v[8], "°C");
        }
        catch (Exception)
        {
            // 忽略解析异常
        }

        return result;
    }

    private static string Bit(string[] fields, int fieldIndex, int bitIndex)
        => fieldIndex < fields.Length ? TextFieldParser.BitAt(fields[fieldIndex], bitIndex) : string.Empty;
}
