using DeviceCloud.Domain.Services.Protocols.Common;

namespace DeviceCloud.Domain.Services.Protocols.Text;

/// <summary>
/// HEEP1/HEEP2/特殊设置指令统一编码器。
/// 从 WpfApp1 HEEP1_ViewModel / HEEP2_ViewModel / Special_Command 的 getSelectedToCommad + SendSettingCommand 移植。
/// 编码结果为纯文本指令（如 "POP01"），CRC + 回车由 TqfCrc16.BuildFrame 追加。
/// </summary>
public static class HeepCommandEncoder
{
    private static readonly IReadOnlyList<SettingDefinition> Settings = BuildSettings();
    private static readonly Dictionary<string, SettingDefinition> ByCode =
        Settings.ToDictionary(s => s.Code, StringComparer.OrdinalIgnoreCase);

    // 工作模式（机型相关）
    private static readonly Dictionary<string, string> WorkingModeAC = new()
    { ["UTI"] = "00", ["SUB"] = "01", ["SBU"] = "02" };
    private static readonly Dictionary<string, string> WorkingModeBD = new()
    { ["SUB"] = "00", ["SBU"] = "01" };

    // 充电模式（机型相关）
    private static readonly Dictionary<string, string> ChargingPriorityAC = new()
    { ["CUT"] = "00", ["CSO"] = "01", ["SNU"] = "02", ["OSO"] = "03" };
    private static readonly Dictionary<string, string> ChargingPriorityBD = new()
    { ["CSO"] = "00", ["SNU"] = "01", ["OSO"] = "02", ["SOR"] = "03" };

    // 精简机型（CYJ/LB6，仅基础电压/电流/开关机）
    private static readonly IReadOnlySet<string> CompactModels =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "UPSCYX01", "LB6", "CG000001"
        };

    // 精简机型（CYJ/LB6）支持的设置项
    private static readonly IReadOnlySet<string> CompactSupportedCodes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "PowerOn", "PowerOff", "OutputVoltage", "OutputSettingFrequency",
            "StrongChargeVoltage", "FloatChargeVoltage", "LowPowerLockVoltage",
            "BattLowAlarmVolt", "AcInputRange", "AcChargeCurrent", "BatteryType"
        };

    public static bool Supports(string code) => ByCode.ContainsKey(code);

    /// <summary>
    /// 判断机型是否支持某设置项。空机型视为未指定（允许所有文本设置）。
    /// BMS 型号不支持文本设置；精简机型（CYJ/LB6）仅支持基础设置项。
    /// </summary>
    public static bool IsSupported(string code, string? model)
    {
        if (string.IsNullOrEmpty(model)) return true;
        if (MachineTypeResolver.IsBmsModel(model)) return false;
        if (CompactModels.Contains(model)) return CompactSupportedCodes.Contains(code);
        return true;
    }

    public static string GetModule(string code)
        => ByCode.TryGetValue(code, out var d) ? d.Module : string.Empty;

    public static IReadOnlyCollection<SettingDefinition> All => Settings;

    public static string Encode(string code, string value, string machineType = "")
    {
        if (!ByCode.TryGetValue(code, out var def))
            throw new ArgumentException($"未知设置项: {code}");

        if (!IsSupported(code, machineType))
            throw new ArgumentException($"机型 {machineType} 不支持设置项 {code}");

        var symbol = ResolveSymbol(code, def, value, machineType);
        return def.Prefix + symbol;
    }

    private static string ResolveSymbol(string code, SettingDefinition def, string value, string machineType)
    {
        // 机型相关的枚举
        if (code == "WorkingMode")
        {
            var map = NormalizeGroup(machineType) == "B" ? WorkingModeBD : WorkingModeAC;
            return map.TryGetValue(value, out var s) ? s : value;
        }
        if (code == "ChargingPriority")
        {
            var map = NormalizeGroup(machineType) == "B" ? ChargingPriorityBD : ChargingPriorityAC;
            return map.TryGetValue(value, out var s) ? s : value;
        }

        // 普通枚举
        if (def.EnumMap != null)
            return def.EnumMap.TryGetValue(value, out var s) ? s : value;

        // 数值格式化
        return def.Format switch
        {
            ValueFormat.FormatToXxx => ProtocolTools.FormatToXxx(value),
            ValueFormat.Pad3 => ProtocolTools.PadToThreeDigits(value),
            ValueFormat.Pad4 => ProtocolTools.PadToFourDigits(value),
            ValueFormat.Pad5 => ProtocolTools.PadToFiveDigits(value),
            _ => value,
        };
    }

    /// <summary>
    /// 归一化机型分组：接受字母机型（A/B/C/D）或具体型号（HPVINVxx 等），
    /// 返回 "A"（A/C 组）或 "B"（B/D 组）；无法识别返回 null（按 A/C 兜底）。
    /// </summary>
    private static string? NormalizeGroup(string? machineType)
    {
        if (string.IsNullOrEmpty(machineType)) return null;
        if (machineType == "A" || machineType == "C") return "A";
        if (machineType == "B" || machineType == "D") return "B";
        return MachineTypeResolver.ResolveGroup(machineType);
    }

    private static IReadOnlyList<SettingDefinition> BuildSettings()
    {
        var list = new List<SettingDefinition>();

        void Add(string code, string module, string prefix, ValueFormat fmt = ValueFormat.None, Dictionary<string, string>? map = null, bool noValue = false)
            => list.Add(new SettingDefinition { Code = code, Module = module, Prefix = prefix, Format = fmt, EnumMap = map, NoValue = noValue });

        // ===== HEEP1 =====
        //工作模式
        Add("WorkingMode", "HEEP1", "POP");
        //总充电电流
        Add("TotalChargeCurrent", "HEEP1", "MNCHGC", ValueFormat.Pad3);
        //市电充电电流
        Add("AcChargeCurrent", "HEEP1", "MUCHGC", ValueFormat.Pad3);
        //市电输入范围
        Add("AcInputRange", "HEEP1", "PGR", map: new() { ["APL"] = "00", ["UPS"] = "01" });
        //并网协议
        Add("PvGridProtocol", "HEEP1", "^S???RS", map: new() { ["India"] = "00", ["Germen"] = "01", ["SouthAmerica"] = "02", ["Pakistan"] = "03" });
        //电池类型
        Add("BatteryType", "HEEP1", "PBT", map: new() { ["AGM"] = "00", ["FLD"] = "01", ["USER"] = "02", ["LIA"] = "03", ["PYL"] = "04", ["TQF"] = "05", ["GRO"] = "06", ["LIB"] = "07", ["LIC"] = "08" });
        //PV馈能优先级
        Add("PvFeedPriority", "HEEP1", "PVENGUSE", map: new() { ["BLU"] = "00", ["LBU"] = "01" });
        //过载重启
        Add("OverloadRestart", "HEEP1", "", map: new() { ["on"] = "PEu", ["off"] = "PDu" });
        //过温重启
        Add("OverTemperatureRestart", "HEEP1", "", map: new() { ["on"] = "PEv", ["off"] = "PDv" });
        //蜂鸣器状态
        Add("BuzzerStatus", "HEEP1", "", map: new() { ["on"] = "PEa", ["off"] = "PDa" });
        //LCD背光
        Add("LcdBacklight", "HEEP1", "", map: new() { ["on"] = "PEx", ["off"] = "PDx" });
        //过载转旁路
        Add("OverloadByPass", "HEEP1", "", map: new() { ["on"] = "PEb", ["off"] = "PDb" });
        //自动返回第一页功能
        Add("AutoReturnHome", "HEEP1", "", map: new() { ["on"] = "PEk", ["off"] = "PDk" });
        //输入源提示
        Add("StatePromptTone", "HEEP1", "", map: new() { ["on"] = "PEy", ["off"] = "PDy" });
        //故障信息保存
        Add("FaultSave", "HEEP1", "", map: new() { ["on"] = "PEz", ["off"] = "PDz" });
        //ECO模式
        Add("EcoMode", "HEEP1", "", map: new() { ["on"] = "PEj", ["off"] = "PDj" });
        //输出模式
        Add("OutputMode", "HEEP1", "POPM", map: new() { ["SIG"] = "00", ["PAL"] = "01", ["3P1"] = "02", ["3P2"] = "03", ["3P3"] = "04" });
        //BMS开关
        Add("BmsCommunicationControl", "HEEP1", "BMSC", map: new() { ["on"] = "01", ["off"] = "00" });
        //充电模式
        Add("ChargingPriority", "HEEP1", "PCP");
        //BMS锁机电池容量
        Add("BmsLowPowerSoc", "HEEP1", "BMSSDC", ValueFormat.Pad3);
        //AC充电电池容量
        Add("BmsReturnToAcSoc", "HEEP1", "BMSB2UC", ValueFormat.Pad3);
        //电池放电电池容量
        Add("BmsReturnToBatterySoc", "HEEP1", "BMSU2BC", ValueFormat.Pad3);
        //逆变开机电池容量
        Add("BmsAutoStartSoc", "HEEP1", "BMSSRC", ValueFormat.Pad3);
        //强充电压
        Add("StrongChargeVoltage", "HEEP1", "PCVV");
        //浮充电压
        Add("FloatChargeVoltage", "HEEP1", "PBFT");
        //低电锁机电压
        Add("LowPowerLockVoltage", "HEEP1", "PSDV");
        //电池过压报警电压
        Add("BattOverVoltAlarm", "HEEP1", "PBVO");
        //并网电流
        Add("GridCurrent", "HEEP1", "PGFC", ValueFormat.Pad3);
        //并网功率
        Add("GridPower", "HEEP1", "PGFP", ValueFormat.Pad5);
        //电池放电电流限制
        Add("DisChargeCurrentLimit", "HEEP1", "DISCC", ValueFormat.Pad3);
        //并网功能
        Add("GridConnectedFunction", "HEEP1", "GRIDFEED", map: new() { ["on"] = "01", ["off"] = "00" });
        //系统频率
        Add("OutputSettingFrequency", "HEEP1", "F", map: new() { ["50"] = "50", ["60"] = "60" });
        //输出电压
        Add("OutputVoltage", "HEEP1", "V", map: new() { ["220"] = "220", ["230"] = "230", ["240"] = "240" });
        //CRC开关
        Add("HostCrc", "HEEP1", "", map: new() { ["on"] = "HOSTCRCEN", ["off"] = "HOSTCRCDN" });
        //复位系统状态
        Add("FactoryReset", "HEEP1", "PF", noValue: true);
        //清除发电量
        Add("ClearEnergy", "HEEP1", "^S???CLE", noValue: true);
        //开机
        Add("PowerOn", "HEEP1", "SWON", noValue: true);
        //定时关机
        Add("PowerOff", "HEEP1", "SWOFF", map: new() { ["off"] = "00", ["shutdown"] = "01", ["10"] = "02", ["15"] = "03", ["20"] = "04", ["30"] = "05" });
        //系统时间
        Add("SystemTime", "HEEP1", "^S???DAT");
        //AC充电时间
        Add("AcChargeTime", "HEEP1", "^S???ACCT");
        //逆变输出时间
        Add("InvOutputTime", "HEEP1", "^S???ACLT");
        //CT功能开关
        Add("CTEnableOperation", "HEEP1", "EXTCT", map: new() { ["on"] = "01", ["off"] = "00" });
        // ===== HEEP2 =====
        //双输出模式
        Add("DualOutputMode", "HEEP2", "PDAULC", map: new() { ["on"] = "01", ["off"] = "00" });
        //并机模式关闭电压
        Add("ParallelModeShutdownVoltage", "HEEP2", "PDSDV", ValueFormat.FormatToXxx);
        //并机模式关闭SOC
        Add("ParallelModeShutdownSoc", "HEEP2", "PDSDS", ValueFormat.Pad3);
        //电池低电告警电压
        Add("BattLowAlarmVolt", "HEEP2", "PSLV", ValueFormat.FormatToXxx);
        //返回市电电池电压
        Add("ReturnMainsBatteryVoltage", "HEEP2", "PBCV", ValueFormat.FormatToXxx);
        //返回电池模式电压
        Add("ReturnBatteryModeVoltage", "HEEP2", "PBDV", ValueFormat.FormatToXxx);
        //电池均衡模式
        Add("BatteryBalancingMmode", "HEEP2", "PBEQE", map: new() { ["on"] = "1", ["off"] = "0" });
        //电池均衡电压
        Add("BatteryBalancingVoltage", "HEEP2", "PBEQV", ValueFormat.FormatToXxx);
        //电池均衡时间
        Add("BatteryBalancingTime", "HEEP2", "PBEQT", ValueFormat.Pad3);
        //电池均衡超时值
        Add("BatteryBalancingTimeout", "HEEP2", "PBEQOT", ValueFormat.Pad3);
        //电池均衡间隔时间(Day)
        Add("BatteryBalancingInterval", "HEEP2", "PBEQP", ValueFormat.Pad3);
        //第二输出放电时长(Min)
        Add("SecondOutputDischargeTime", "HEEP2", "PDDCGT", ValueFormat.Pad4);
        //第二输出的延时时间
        Add("SecondOutputDelayTime", "HEEP2", "PDDLYT", ValueFormat.Pad3);
        //第二输出的电池电压
        Add("SecondOutputRestoreVoltage", "HEEP2", "PDSRV", ValueFormat.FormatToXxx);
        //第二输出的电池容量
        Add("SecondOutputRestoreCapacity", "HEEP2", "PDSRS", ValueFormat.Pad3);
        //第二输出时间
        Add("SecondOutputTime", "HEEP2", "^S???DALT");
        //调零功率
        Add("ZeroAdjPwr", "HEP3", "EZCTP",ValueFormat.Pad3);

        return list;
    }
}
