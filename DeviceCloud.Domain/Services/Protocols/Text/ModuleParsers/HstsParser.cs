using DeviceCloud.Domain.Services.Protocols.Common;

namespace DeviceCloud.Domain.Services.Protocols.Text.ModuleParsers;

/// <summary>
/// HSTS（状态）模块解析器。协议 5.2。
/// 响应格式：(NN MABCDEFGHIJKL MPQRSTUVWXYOOOOOOOOOO
/// </summary>
public class HstsParser : IModuleParser
{
    public string Code => "HSTS";

    private static readonly IReadOnlyDictionary<string, string> ModeMap = new Dictionary<string, string>
    {
        ["P"] = "初始上电模式", ["S"] = "待机模式", ["L"] = "市电模式", ["B"] = "电池模式",
        ["F"] = "故障模式", ["D"] = "关机模式", ["X"] = "测试模式",
    };

    private static readonly IReadOnlyDictionary<string, string> GridTieMap = new Dictionary<string, string>
    {
        ["0"] = "离网", ["1"] = "等待并网", ["2"] = "正在并网",
    };

    private static readonly IReadOnlyDictionary<string, string> RoleMap = new Dictionary<string, string>
    {
        ["0"] = "切换中", ["1"] = "从机", ["2"] = "主机",
    };

    private static readonly IReadOnlyDictionary<string, string> LampMap = new Dictionary<string, string>
    {
        ["0"] = "关闭", ["1"] = "常亮", ["2"] = "闪烁",
    };

    private static readonly IReadOnlyDictionary<string, string> OnOffMap = new Dictionary<string, string>
    {
        ["0"] = "关闭", ["1"] = "开启",
    };

    private static readonly IReadOnlyDictionary<string, string> HasMap = new Dictionary<string, string>
    {
        ["0"] = "无", ["1"] = "有",
    };

    public IReadOnlyDictionary<string, string> Parse(string rawValue)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(rawValue) || TextFieldParser.IsCrcError(rawValue))
            return result;

        var v = TextFieldParser.SplitFields(rawValue);

        try
        {
            // 故障代码：Values[0] 的 Substring(1, 2)
            result["故障代码"] = TextFieldParser.Substring(v[0], 1, 2);

            // Values[1]：M + ABCDEFGHIJKL（模式 + 12 位状态）
            result["模式"] = TextFieldParser.Map(Bit(v, 1, 0), ModeMap);
            result["AC状态下PV馈能到负载"] = TextFieldParser.Map(Bit(v, 1, 1), HasMap);
            result["机器是否有输出"] = TextFieldParser.Map(Bit(v, 1, 2), HasMap);
            result["电池低电报警"] = TextFieldParser.YesNo(Bit(v, 1, 3));
            result["电池未接"] = TextFieldParser.YesNo(Bit(v, 1, 4));
            result["输出过载"] = TextFieldParser.YesNo(Bit(v, 1, 5));
            result["机器过温"] = TextFieldParser.YesNo(Bit(v, 1, 6));
            result["EEPROM数据异常"] = TextFieldParser.YesNo(Bit(v, 1, 7));
            result["EEPROM读写异常"] = TextFieldParser.YesNo(Bit(v, 1, 8));
            result["PV功率过低异常"] = TextFieldParser.YesNo(Bit(v, 1, 9));
            result["输入电压过高"] = TextFieldParser.YesNo(Bit(v, 1, 10));
            result["电池电压过高"] = TextFieldParser.YesNo(Bit(v, 1, 11));
            result["风扇转速异常"] = TextFieldParser.YesNo(Bit(v, 1, 12));

            // Values[2]：MPQRSTUVWXY + 其他标志
            result["并机系统机器总数"] = TextFieldParser.Number(Bit(v, 2, 0));
            result["并网标志"] = TextFieldParser.Map(Bit(v, 2, 1), GridTieMap);
            result["并机系统角色"] = TextFieldParser.Map(Bit(v, 2, 2), RoleMap);
            result["主输出继电器状态"] = TextFieldParser.Map(Bit(v, 2, 3), OnOffMap);
            result["第二输出当前状态"] = TextFieldParser.Map(Bit(v, 2, 4), OnOffMap);
            result["BMS通讯异常状态"] = TextFieldParser.YesNo(Bit(v, 2, 5));
            result["温度传感器异常"] = TextFieldParser.YesNo(Bit(v, 2, 6));
            result["市电灯状态"] = TextFieldParser.Map(Bit(v, 2, 7), LampMap);
            result["逆变灯状态"] = TextFieldParser.Map(Bit(v, 2, 8), LampMap);
            result["充电灯状态"] = TextFieldParser.Map(Bit(v, 2, 9), LampMap);
            result["报警灯状态"] = TextFieldParser.Map(Bit(v, 2, 10), LampMap);
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
