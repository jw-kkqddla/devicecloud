namespace DeviceCloud.Domain.Services.Protocols.Text.ModuleParsers;

/// <summary>
/// HSTS2 模块解析器。协议 5.3。
/// 响应格式：(NN MABCDEFG HIJKL MPQRSTUVWXYOOOOOOOOOO
/// </summary>
public class Hsts2Parser : IModuleParser
{
    public string Code => "HSTS2";

    private static readonly IReadOnlyDictionary<string, string> ModeMap = new Dictionary<string, string>
    {
        ["P"] = "初始上电模式", ["S"] = "待机模式", ["L"] = "市电模式", ["B"] = "电池模式",
        ["F"] = "故障模式", ["D"] = "关机模式", ["X"] = "测试模式",
    };

    private static readonly IReadOnlyDictionary<string, string> OnOffMap = new Dictionary<string, string>
    {
        ["0"] = "关闭", ["1"] = "开启",
    };

    public IReadOnlyDictionary<string, string> Parse(string rawValue)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(rawValue) || TextFieldParser.IsCrcError(rawValue))
            return result;

        var v = TextFieldParser.SplitFields(rawValue);

        try
        {
            result["故障代码"] = TextFieldParser.Substring(v[0], 1, 2);

            // Values[1]：MABCDEFG
            result["模式"] = TextFieldParser.Map(Bit(v, 1, 0), ModeMap);
            result["PV"] = Bit(v, 1, 1) == "1" ? "有输入" : "无";
            result["市电"] = Bit(v, 1, 2) == "1" ? "有输入" : "无";
            result["逆变输出"] = Bit(v, 1, 3) == "1" ? "有输出" : "无";
            result["并网标志"] = TextFieldParser.YesNo(Bit(v, 1, 4));
            result["能量供给负载"] = TextFieldParser.YesNo(Bit(v, 1, 5));
            result["电池充电标志"] = TextFieldParser.YesNo(Bit(v, 1, 6));
            result["电池放电标志"] = TextFieldParser.YesNo(Bit(v, 1, 7));

            // Values[2]：HIJKL
            result["逆变器工作状态"] = TextFieldParser.Map(Bit(v, 2, 0), OnOffMap);
            result["PV电压状态"] = Bit(v, 2, 1) == "1" ? "正常" : "异常";
            result["逆变桥状态"] = Bit(v, 2, 2);
            result["MPPT状态"] = Bit(v, 2, 3);
            result["锁相环状态"] = Bit(v, 2, 4);
        }
        catch (Exception)
        {
            // 字段越界等异常，返回已解析的部分
        }

        return result;
    }

    private static string Bit(string[] fields, int fieldIndex, int bitIndex)
        => fieldIndex < fields.Length ? TextFieldParser.BitAt(fields[fieldIndex], bitIndex) : string.Empty;
}
