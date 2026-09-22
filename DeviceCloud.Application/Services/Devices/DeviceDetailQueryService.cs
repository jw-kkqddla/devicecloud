using System.Text.Json;
using DeviceCloud.Application.DTOs.Devices;
using DeviceCloud.Application.Interfaces;
using DeviceCloud.Application.Interfaces.Devices;
using DeviceCloud.Domain.Services;
using DeviceCloud.Domain.Services.Devices;
using Microsoft.Extensions.Logging;

namespace DeviceCloud.Application.Services.Devices;

/// <summary>
/// 设备详情页查询服务实现。
/// 从 DetailModel 抽取：物模型解析、最新值加载（readData 优先 + 历史回退 + 缓存）、
/// 字段分组、机型识别、图表属性映射、历史曲线与月发电量查询。
/// </summary>
public class DeviceDetailQueryService : IDeviceDetailQueryService
{
    private readonly IDeviceAppService _deviceAppService;
    private readonly IDeviceService _deviceService;
    private readonly IProtocolParsingService _protocolParsingService;
    private readonly ICommandEncodingService _commandEncodingService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DeviceDetailQueryService> _logger;

    public DeviceDetailQueryService(
        IDeviceAppService deviceAppService,
        IDeviceService deviceService,
        IProtocolParsingService protocolParsingService,
        ICommandEncodingService commandEncodingService,
        ICacheService cacheService,
        ILogger<DeviceDetailQueryService> logger)
    {
        _deviceAppService = deviceAppService;
        _deviceService = deviceService;
        _protocolParsingService = protocolParsingService;
        _commandEncodingService = commandEncodingService;
        _cacheService = cacheService;
        _logger = logger;
    }

    // 物模型解析
    public List<TslPropertyViewModel> ParseTslProperties(string tslData)
    {
        var properties = new List<TslPropertyViewModel>();

        if (string.IsNullOrWhiteSpace(tslData))
            return properties;

        using var doc = JsonDocument.Parse(tslData);
        var root = doc.RootElement;

        JsonElement propsElement;
        bool found = false;

        if (root.TryGetProperty("properties", out propsElement))
            found = true;
        else if (root.TryGetProperty("data", out var dataEl) &&
                 dataEl.ValueKind == JsonValueKind.Object &&
                 dataEl.TryGetProperty("properties", out propsElement))
            found = true;
        else if (root.TryGetProperty("profile", out var profileEl) &&
                 profileEl.ValueKind == JsonValueKind.Object &&
                 profileEl.TryGetProperty("properties", out propsElement))
            found = true;

        if (!found)
            return properties;

        if (propsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var prop in propsElement.EnumerateArray())
            {
                if (prop.ValueKind != JsonValueKind.Object)
                    continue;

                var tslProp = ParseTslProperty(prop);
                if (tslProp != null)
                    properties.Add(tslProp);
            }
        }
        else if (propsElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var item in propsElement.EnumerateObject())
            {
                var prop = item.Value;
                if (prop.ValueKind != JsonValueKind.Object)
                    continue;

                var tslProp = ParseTslProperty(prop, item.Name);
                if (tslProp != null)
                    properties.Add(tslProp);
            }
        }

        return properties.OrderBy(p => p.Name).ToList();
    }

    private TslPropertyViewModel? ParseTslProperty(JsonElement prop, string? defaultIdentifier = null)
    {
        string? identifier = defaultIdentifier;
        string? name = null;
        string dataType = string.Empty;
        int? numericId = null;

        if (prop.TryGetProperty("identifier", out var identifierEl) && identifierEl.ValueKind == JsonValueKind.String)
            identifier = identifierEl.GetString();
        else if (prop.TryGetProperty("code", out var codeEl) && codeEl.ValueKind == JsonValueKind.String)
            identifier = codeEl.GetString();
        else if (prop.TryGetProperty("id", out var idNum) && idNum.ValueKind != JsonValueKind.Null)
            identifier = idNum.ToString();

        if (prop.TryGetProperty("name", out var nameEl) && nameEl.ValueKind == JsonValueKind.String)
            name = nameEl.GetString();

        if (prop.TryGetProperty("dataType", out var typeEl) && typeEl.ValueKind == JsonValueKind.String)
            dataType = typeEl.GetString() ?? string.Empty;
        else if (prop.TryGetProperty("type", out var type2El) && type2El.ValueKind == JsonValueKind.String)
            dataType = type2El.GetString() ?? string.Empty;

        if (prop.TryGetProperty("id", out var idNumEl) && idNumEl.ValueKind == JsonValueKind.Number)
        {
            if (idNumEl.TryGetInt32(out var intId))
                numericId = intId;
        }

        if (string.IsNullOrEmpty(identifier))
            return null;

        return new TslPropertyViewModel
        {
            Identifier = identifier,
            Name = string.IsNullOrEmpty(name) ? identifier : name,
            DataType = dataType.ToUpperInvariant(),
            PropId = numericId
        };
    }

    // ============================================================
    // 最新值加载
    // ============================================================

    public async Task<DeviceDataSnapshot> LoadSnapshotAsync(string productKey, string deviceKey, List<TslPropertyViewModel> tslProperties)
    {
        var snapshot = new DeviceDataSnapshot();

        try
        {
            var (readOk, readValues, readTime) = await TryFillFromReadDataAsync(productKey, deviceKey, tslProperties);

            Dictionary<int, string> values;
            long? updateTime;
            if (readOk)
            {
                values = readValues;
                updateTime = readTime;
            }
            else
            {
                (values, updateTime) = await FillFromHistoryAsync(productKey, deviceKey, tslProperties);
            }

            snapshot.Values = values;
            snapshot.UpdateTime = updateTime;

            if (values.Count > 0)
            {
                var (machineType, machineTypeGroup) = ResolveMachineType(tslProperties, values);
                snapshot.MachineType = machineType;
                snapshot.MachineTypeGroup = machineTypeGroup;
                snapshot.FieldGroups = BuildFieldGroups(tslProperties, values, machineType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取设备最新数据失败");
        }

        return snapshot;
    }

    /// <summary>
    /// 优先用 readData 读设备当前值（轻量），成功返回 true；失败/无数据返回 false，由调用方回退历史数据
    /// </summary>
    private async Task<(bool Success, Dictionary<int, string> Values, long? UpdateTime)> TryFillFromReadDataAsync(
        string productKey, string deviceKey, List<TslPropertyViewModel> tslProperties)
    {
        try
        {
            var identifiers = tslProperties
                .Where(t => !string.IsNullOrWhiteSpace(t.Identifier))
                .Select(t => t.Identifier)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (identifiers.Count == 0)
                return (false, new Dictionary<int, string>(), null);

            var cacheKey = $"dc:cache:device:readdata:{productKey}:{deviceKey}";

            var cache = await _cacheService.GetAsync<LatestValuesCache>(cacheKey);
            if (cache == null)
            {
                var dataJson = JsonSerializer.Serialize(identifiers);
                var result = await _deviceAppService.ReadDeviceDataAsync(
                    productKey, new List<string> { deviceKey }, dataJson);

                if (!result.Success || result.Data == null || result.Data.Count == 0)
                    return (false, new Dictionary<int, string>(), null);

                var props = result.Data.Count > 0 ? result.Data[0].Properties : null;
                if (props == null || props.Count == 0)
                    return (false, new Dictionary<int, string>(), null);

                // key 可能是 identifier 或数字 id，统一映射成 PropId -> value
                var idMap = new Dictionary<int, string>();
                long? maxTime = null;
                foreach (var prop in props)
                {
                    if (string.IsNullOrWhiteSpace(prop.Key))
                        continue;

                    var val = NormalizeValue(prop.Value);
                    if (string.IsNullOrWhiteSpace(val))
                        continue;

                    var tsl = tslProperties.FirstOrDefault(t =>
                        string.Equals(t.Identifier, prop.Key, StringComparison.OrdinalIgnoreCase));
                    if (tsl?.PropId.HasValue == true)
                    {
                        idMap[tsl.PropId.Value] = val;
                    }
                    else if (int.TryParse(prop.Key, out var idNum))
                    {
                        idMap[idNum] = val;
                    }

                    if (prop.Time.HasValue && (maxTime == null || prop.Time.Value > maxTime.Value))
                        maxTime = prop.Time.Value;
                }

                if (idMap.Count == 0)
                    return (false, new Dictionary<int, string>(), null);

                // 缓存解析后的结果（idMap + 时间），命中后无需重复映射
                cache = new LatestValuesCache { Values = idMap, UpdateTime = maxTime };
                await _cacheService.SetAsync(cacheKey, cache, TimeSpan.FromSeconds(10));
            }

            return (true, cache.Values, cache.UpdateTime);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "readData 读取设备当前值失败，回退历史数据");
            return (false, new Dictionary<int, string>(), null);
        }
    }

    /// <summary>
    /// 回退：拉历史数据，合并区间内所有上行（up）记录
    /// </summary>
    private async Task<(Dictionary<int, string> Values, long? UpdateTime)> FillFromHistoryAsync(
        string productKey, string deviceKey, List<TslPropertyViewModel> tslProperties)
    {
        var empty = (new Dictionary<int, string>(), (long?)null);

        var cacheKey = $"dc:cache:device:history:{productKey}:{deviceKey}";

        var cache = await _cacheService.GetAsync<LatestValuesCache>(cacheKey);
        if (cache == null)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var end = now;
            var start = Math.Max(0, now - 168 * 3600 * 1000);

            var historyJson = await _deviceService.GetDeviceHistoryDataAsync(productKey, deviceKey, start, end);

            if (string.IsNullOrWhiteSpace(historyJson))
            {
                _logger.LogWarning("获取历史数据返回空");
                return empty;
            }

            using var hdoc = JsonDocument.Parse(historyJson);
            var hroot = hdoc.RootElement;

            if (!hroot.TryGetProperty("data", out var hdata) ||
                hdata.ValueKind != JsonValueKind.Object ||
                !hdata.TryGetProperty("records", out var hrecords) ||
                hrecords.ValueKind != JsonValueKind.Array ||
                hrecords.GetArrayLength() == 0)
            {
                return empty;
            }

            // 合并区间内所有上行（up）记录，按 id 各自取最新的非空值
            var (latest, latestUpTime) = MergeLatestUpValues(hrecords, tslProperties);

            // 缓存解析后的结果（idMap + 时间），命中后无需重复 Parse + Merge
            cache = new LatestValuesCache { Values = latest, UpdateTime = latestUpTime };
            await _cacheService.SetAsync(cacheKey, cache, TimeSpan.FromSeconds(10));
        }

        return (cache.Values, cache.UpdateTime);
    }

    private static string? NormalizeValue(object? value)
    {
        if (value == null)
            return null;

        if (value is JsonElement je)
        {
            return je.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.String => je.GetString(),
                _ => je.GetRawText()
            };
        }

        return value.ToString();
    }

    /// <summary>
    /// 合并区间内所有上行（up）记录，按物模型属性 id 各自取最新的非空值
    /// </summary>
    /// <returns>latest：id -> 最新值；latestUpTime：up 记录中最新的 createTime</returns>
    private (Dictionary<int, string> Latest, long? LatestUpTime) MergeLatestUpValues(
        JsonElement hrecords, List<TslPropertyViewModel> tslProperties)
    {
        var latest = new Dictionary<int, string>();
        var pending = new HashSet<int>(
            tslProperties.Where(t => t.PropId.HasValue).Select(t => t.PropId!.Value));

        // 收集所有 up 记录
        var upRecords = new List<(long CreateTime, JsonElement Record)>();
        foreach (var rec in hrecords.EnumerateArray())
        {
            if (!rec.TryGetProperty("msgType", out var mt) ||
                mt.ValueKind != JsonValueKind.String ||
                !string.Equals(mt.GetString(), "up", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            long createTime = 0;
            if (rec.TryGetProperty("createTime", out var ct) &&
                ct.ValueKind == JsonValueKind.Number &&
                ct.TryGetInt64(out var t))
            {
                createTime = t;
            }

            upRecords.Add((createTime, rec));
        }

        long? latestUpTime = null;

        // 按 createTime 倒序（不依赖接口返回顺序），逐 id 取首次遇到的非空值
        foreach (var (createTime, rec) in upRecords.OrderByDescending(x => x.CreateTime))
        {
            if (!latestUpTime.HasValue || createTime > latestUpTime.Value)
            {
                latestUpTime = createTime;
            }

            if (!rec.TryGetProperty("dmData", out var dm))
                continue;

            var idMap = ParseDmDataSafely(dm);
            foreach (var (id, val) in idMap)
            {
                if (string.IsNullOrWhiteSpace(val))
                    continue;

                if (pending.Remove(id))
                    latest[id] = val;
            }

            // 所有属性都凑齐了，提前退出
            if (pending.Count == 0)
                break;
        }
        return (latest, latestUpTime);
    }

    /// <summary>
    /// 解析 dmData，支持字符串和数组两种格式
    /// </summary>
    private Dictionary<int, string> ParseDmDataSafely(JsonElement dm)
    {
        var idMap = new Dictionary<int, string>();

        try
        {
            if (dm.ValueKind == JsonValueKind.String)
            {
                var txt = dm.GetString();
                if (string.IsNullOrEmpty(txt))
                    return idMap;

                using var dmDoc = JsonDocument.Parse(txt);
                var dataArray = dmDoc.RootElement;

                if (dataArray.ValueKind != JsonValueKind.Array)
                    return idMap;

                ParseDmArray(dataArray, idMap);
            }
            else if (dm.ValueKind == JsonValueKind.Array)
            {
                ParseDmArray(dm, idMap);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "解析 dmData 失败");
        }

        _logger.LogDebug("dmData 解析完成，共 {Count} 个属性值", idMap.Count);
        return idMap;
    }

    /// <summary>
    /// 解析 dmData 数组
    /// </summary>
    private void ParseDmArray(JsonElement dataArray, Dictionary<int, string> idMap)
    {
        if (dataArray.ValueKind != JsonValueKind.Array)
            return;

        int idx = 0;
        foreach (var item in dataArray.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Object)
            {
                if (item.TryGetProperty("id", out var idEl) &&
                    idEl.ValueKind == JsonValueKind.Number &&
                    idEl.TryGetInt32(out var idNum))
                {
                    var val = ExtractValue(item);
                    if (val != null)
                        idMap[idNum] = val;
                }
                else
                {
                    var val = ExtractValue(item);
                    if (val != null)
                        idMap[idx + 1] = val;
                }
            }
            else
            {
                var val = item.ValueKind == JsonValueKind.String ?
                    item.GetString() ?? string.Empty :
                    item.ToString();
                idMap[idx + 1] = val;
            }
            idx++;
        }
    }

    /// <summary>
    /// 从 JSON 对象中提取 value 或 val 字段
    /// </summary>
    private string? ExtractValue(JsonElement item)
    {
        if (item.TryGetProperty("value", out var v) && v.ValueKind != JsonValueKind.Null)
        {
            return v.ValueKind == JsonValueKind.String ? v.GetString() : v.ToString();
        }

        if (item.TryGetProperty("val", out var v2) && v2.ValueKind != JsonValueKind.Null)
        {
            return v2.ValueKind == JsonValueKind.String ? v2.GetString() : v2.ToString();
        }

        return null;
    }

    // ============================================================
    // 字段分组与机型识别
    // ============================================================

    /// <summary>
    /// 显示名 -> 工程字段名 映射（仅记录与解析器输出不一致的字段；同名字段无需映射）
    /// </summary>
    private static readonly Dictionary<string, string> FieldRenameMap = new(StringComparer.Ordinal)
    {
        ["设备类型"] = "MachineType",
        ["状态码"] = "故障代码",
        ["并机角色"] = "并机系统角色",
        ["并网总数"] = "并机系统机器总数",
        ["系统时间（小时分钟）"] = "系统时间",
        ["系统时间（年月日）"] = "系统日期",
        ["过载重启功能"] = "过载重启",
        ["电池电压1"] = "电池1电压",
        ["电池电压2"] = "电池2电压",
        ["电池电压3"] = "电池3电压",
        ["电池电压4"] = "电池4电压",
        ["电池电压5"] = "电池5电压",
        ["电池电压6"] = "电池6电压",
        ["电池电压7"] = "电池7电压",
        ["电池电压8"] = "电池8电压",
        ["电池电压9"] = "电池9电压",
        ["电池电压10"] = "电池10电压",
        ["电池电压11"] = "电池11电压",
        ["电池电压12"] = "电池12电压",
        ["电池电压13"] = "电池13电压",
        ["电池电压14"] = "电池14电压",
        ["电池电压15"] = "电池15电压",
        ["电池电压16"] = "电池16电压",
    };

    /// <summary>
    /// 每个分组的字段显示顺序（显示名）。清单里的缺失字段（解析器无输出）也会按此顺序空占位显示。
    /// </summary>
    private static readonly Dictionary<string, string[]> FieldOrder = new(StringComparer.Ordinal)
    {
        ["电池状态"] = new[]
        {
            "电池电压", "电池容量", "电池充电电流", "电池放电电流",
            "电池状态", "电池类型", "电池节数",
        },
        ["BMS状态"] = new[]
        {
            "剩余容量", "标称容量", "显示模式", "最高电压",
            "最低电压", "最高电压电芯位置", "最低电压电芯位置", "电池电压1",
            "电池电压2", "电池电压3", "电池电压4", "电池电压5",
            "电池电压6", "电池电压7", "电池电压8", "电池电压9",
            "电池电压10", "电池电压11", "电池电压12", "电池电压13",
            "电池电压14", "电池电压15", "电池电压16",
        },
        ["电网状态"] = new[]
        {
            "市电电压", "市电频率", "市电电流流向", "市电功率",
        },
        ["负载状态"] = new[]
        {
            "输出电压", "输出频率", "输出视在功率", "输出有功功率",
            "输出负载百分比", "输出直流分量",
        },
        ["光伏板状态"] = new[]
        {
            "发电功率", "PV电压", "PV电流", "PV功率",
            "日发电量", "月发电量", "年发电量", "总发电量",
            "PV温度", "太阳能充电开关",
        },
        ["更多"] = new[]
        {
            "设备类型", "输出模式", "模式", "状态码",
            "BUS电压", "软件版本", "AC充电开关", "AC状态下PV馈能到负载",
            "BMS低电SOC", "BMS低电后自动开机SOC", "BMS低电报警标志", "BMS低电故障标志",
            "BMS允许充电标志", "BMS允许放电标志", "BMS充电电压限制", "BMS充电电流",
            "BMS充电电流限制", "BMS充电过流标志", "BMS平均温度", "BMS当前SOC",
            "BMS放电电压限制", "BMS放电电流", "BMS放电过流标志", "BMS温度过低标志",
            "BMS温度过高标志", "BMS返回市电模式SOC", "BMS返回电池模式SOC", "BMS通信控制功能",
            "BMS通信正常", "CT功能开关", "ECO", "EEPROM数据异常",
            "EEPROM读写异常", "LCD背光", "MPPT恒温模式", "PV功率过低异常",
            "PV并网协议", "PV馈能优先级", "主输出继电器状态", "低电锁机电压",
            "充电优先顺序", "充电总开关", "充电灯状态", "升压温度",
            "双输出模式", "变压器温度", "均衡时间", "均衡超时",
            "均衡间隔", "工作模式", "市电丢失电压低点", "市电丢失电压高点",
            "市电丢失频率低点", "市电丢失频率高点", "市电充电关闭时间", "市电充电开启时间",
            "市电最大充电电流", "市电灯状态", "市电输入范围", "并机模式",
            "并机模式关闭SOC", "并机模式关闭电压", "并机角色", "并网功能",
            "并网总数", "并网标志", "并网电流", "强充电压",
            "报警灯状态", "最大总充电电流", "最高温度", "机器是否有输出",
            "机器过温", "浮充电压", "温度传感器异常", "电感电流",
            "电池低电报警", "电池均衡模式", "电池均衡电压", "电池未接",
            "电池电压过高", "电池过压关机电压", "第二延时时间", "第二输出关闭时间",
            "第二输出开启时间", "第二输出放电时间", "第二输出电池容量", "第二输出电池电压",
            "系统时间（小时分钟）", "系统时间（年月日）", "系统运行时间", "自动返回第一页功能",
            "蜂鸣器功能", "输入源提示功能", "输入电压过高", "输出设定电压",
            "输出设定频率", "输出过载", "过温重启功能", "过载转旁路功能",
            "过载重启功能", "返回市电模式电压", "返回电池模式电压", "逆变温度",
            "逆变灯状态", "锂电激活功能开关", "锂电激活过程", "风扇1状态",
            "风扇1转速", "风扇2状态", "风扇2转速", "风扇转速异常",
        },
    };

    /// <summary>
    /// 业务分组展示顺序
    /// </summary>
    private static readonly string[] GroupOrder =
    {
        "电池状态", "BMS状态", "电网状态", "负载状态", "光伏板状态", "更多"
    };

    /// <summary>
    /// 从合并结果识别机型（MachineType 属性），用于控制面板自动填充机型
    /// </summary>
    private (string? MachineType, string? MachineTypeGroup) ResolveMachineType(
        List<TslPropertyViewModel> tslProperties, Dictionary<int, string> idMap)
    {
        var machineTsl = tslProperties.FirstOrDefault(t =>
            string.Equals(t.Identifier, "MachineType", StringComparison.OrdinalIgnoreCase));
        if (machineTsl?.PropId.HasValue == true &&
            idMap.TryGetValue(machineTsl.PropId.Value, out var machineRaw) &&
            !string.IsNullOrWhiteSpace(machineRaw))
        {
            return (
                _commandEncodingService.ResolveMachineModel(machineRaw),
                _commandEncodingService.ResolveMachineTypeGroup(machineRaw));
        }

        return (null, null);
    }

    /// <summary>
    /// 把合并后的最新值按固定字段清单（FieldOrder）归组展示：
    /// 1. 先把所有模块的协议解析字段打平成「工程字段名 -> 值」；
    /// 2. 再按 FieldOrder 的顺序输出，字段名经 FieldRenameMap 对齐为显示名，取不到值则空占位；
    /// 3. 解析器有、但清单未列出的字段追加到「更多」末尾。
    /// </summary>
    public List<PropertyFieldGroup> BuildFieldGroups(
        List<TslPropertyViewModel> tslProperties, Dictionary<int, string> idMap, string? machineType)
    {
        // 1. 打平所有模块的解析结果
        var fieldValues = new Dictionary<string, string>(StringComparer.Ordinal);
        var machineRaw = string.Empty;

        foreach (var tsl in tslProperties)
        {
            if (!tsl.PropId.HasValue || !idMap.TryGetValue(tsl.PropId.Value, out var raw))
                continue;

            // 设备类型：MachineType 属性单独记录，显示解析后的型号
            if (string.Equals(tsl.Identifier, "MachineType", StringComparison.OrdinalIgnoreCase))
            {
                machineRaw = raw;
                continue;
            }

            if (_protocolParsingService.Supports(tsl.Identifier))
            {
                var fields = _protocolParsingService.Parse(tsl.Identifier, raw);
                if (fields != null && fields.Count > 0)
                {
                    foreach (var kv in fields)
                        fieldValues[kv.Key] = kv.Value;
                }
                else if (!string.IsNullOrWhiteSpace(raw))
                {
                    // 解析失败时回退为显示原始值，确保有数据就能展示
                    fieldValues[tsl.Name] = raw;
                }
            }
            else
            {
                fieldValues[tsl.Name] = raw;
            }
        }

        // 设备类型值：优先解析后的型号，否则原始值
        if (!string.IsNullOrWhiteSpace(machineType))
            fieldValues["MachineType"] = machineType;
        else if (!string.IsNullOrWhiteSpace(machineRaw))
            fieldValues["MachineType"] = machineRaw;

        // 2. 按 FieldOrder 顺序组装（缺失字段空占位）
        var groups = new Dictionary<string, List<PropertyFieldItem>>();
        foreach (var g in GroupOrder)
            groups[g] = new List<PropertyFieldItem>();

        var consumed = new HashSet<string>(StringComparer.Ordinal);

        foreach (var g in GroupOrder)
        {
            if (!FieldOrder.TryGetValue(g, out var displayNames))
                continue;

            foreach (var displayName in displayNames)
            {
                var fieldKey = FieldRenameMap.TryGetValue(displayName, out var renamed)
                    ? renamed
                    : displayName;

                var value = fieldValues.TryGetValue(fieldKey, out var v) ? v : string.Empty;
                consumed.Add(fieldKey);
                groups[g].Add(new PropertyFieldItem { Label = displayName, Value = value });
            }
        }

        // 3. 解析器有、但清单未列出的字段，追加到「更多」末尾
        foreach (var kv in fieldValues)
        {
            if (!consumed.Contains(kv.Key))
                groups["更多"].Add(new PropertyFieldItem { Label = kv.Key, Value = kv.Value });
        }

        return GroupOrder
            .Where(g => groups[g].Count > 0)
            .Select(g => new PropertyFieldGroup { GroupName = g, Fields = groups[g] })
            .ToList();
    }

    // ============================================================
    // 图表属性映射
    // ============================================================

    /// <summary>
    /// 图表属性名 -> (模块 code, 解析字段名)
    /// </summary>
    private static readonly Dictionary<string, (string Module, string Field, string Unit)> ChartPropertyMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["市电电压"] = ("HGRID","市电电压","V"),
        ["市电频率"] = ("HGRID","市电频率","Hz"),
        ["市电功率"] = ("HGRID","市电功率","W"),
        ["输出电压"] = ("HOP","输出电压","V"),
        ["输出频率"] = ("HOP","输出频率","Hz"),
        ["输出视在功率"] = ("HOP","输出视在功率","W"),
        ["输出有功功率"] = ("HOP", "输出有功功率", "W"),
        ["输出负载百分比"] = ("HOP", "输出负载百分比", "%"),
        ["电池电压"] = ("HBAT", "电池电压", "V"),
        ["电池容量"] = ("HBAT", "电池容量", "%"),
        ["电池充电电流"] = ("HBAT", "电池充电电流", "A"),
        ["电池放电电流"] = ("HBAT", "电池放电电流", "A"),
        ["BUS电压"] = ("HBAT", "BUS电压", "V"),
        ["PV电压"] = ("HPV", "PV电压", "V"),
        ["PV电流"] = ("HPV", "PV电流", "A"),
        ["PV功率"] = ("HPV", "PV功率", "W"),
        ["BMS低电SOC"] = ("HEP1", "BMS低电SOC", "%"),
        ["BMS低电后自动开机SOC"] = ("HEP1", "BMS低电后自动开机SOC", "%"),
        ["BMS充电电压限制"] = ("HBMS1", "BMS充电电压限制", "V"),
        ["BMS充电电流"] = ("HBMS1", "BMS充电电流", "A"),
        ["BMS充电电流限制"] = ("HBMS1", "BMS充电电流限制", "A"),
        ["BMS平均温度"] = ("HBMS1", "BMS平均温度", "°C"),
        ["BMS当前SOC"] = ("HBMS1", "BMS当前SOC", "%"),
        ["BMS放电电压限制"] = ("HBMS1", "BMS放电电压限制", "V"),
        ["BMS放电电流"] = ("HBMS1", "BMS放电电流", "A"),
        ["BMS返回市电模式SOC"] = ("HEP1", "BMS返回市电模式SOC", "%"),
        ["BMS返回电池模式SOC"] = ("HEP1", "BMS返回电池模式SOC", "%"),
        ["PV温度"] = ("HTEMP", "PV温度", "°C"),
        ["低电锁机电压"] = ("HEP1", "低电锁机电压", "V"),
        ["升压温度"] = ("HTEMP", "升压温度", "°C"),
        ["变压器温度"] = ("HTEMP", "变压器温度", "°C"),
        ["市电丢失电压低点"] = ("HGRID", "市电丢失电压低点", "V"),
        ["市电丢失电压高点"] = ("HGRID", "市电丢失电压高点", "V"),
        ["市电丢失频率低点"] = ("HGRID", "市电丢失频率低点", "Hz"),
        ["市电丢失频率高点"] = ("HGRID", "市电丢失频率高点", "Hz"),
        ["市电最大充电电流"] = ("HEP1", "市电最大充电电流", "A"),
        ["并网电流"] = ("HEP1", "并网电流", "A"),
        ["强充电压"] = ("HEP1", "强充电压", "V"),
        ["最大总充电电流"] = ("HEP1", "最大总充电电流", "A"),
        ["浮充电压"] = ("HEP1", "浮充电压", "V"),
        ["电感电流"] = ("HOP", "电感电流", "A"),
        ["输出设定电压"] = ("HEP1", "输出设定电压", "V"),
        ["逆变温度"] = ("HTEMP", "逆变温度", "°C"),
        ["风扇1转速"] = ("HTEMP", "风扇1转速", "%"),
        ["风扇2转速"] = ("HTEMP", "风扇2转速", "%"),
        ["日发电量"] = ("HGEN","日发电量", "KW.h")
    };

    private static readonly IReadOnlyList<ChartPropertyGroup> ChartPropertyGroupCache = BuildChartPropertyGroups();

    public IReadOnlyList<ChartPropertyGroup> ChartPropertyGroups => ChartPropertyGroupCache;

    /// <summary>
    /// 按模块顺序将 ChartPropertyMap 分组，保证下拉框顺序稳定。
    /// </summary>
    private static IReadOnlyList<ChartPropertyGroup> BuildChartPropertyGroups()
    {
        var order = new (string Module, string Label)[]
        {
            ("HGRID", "市电"),
            ("HOP", "输出"),
            ("HBAT", "电池"),
            ("HPV", "光伏"),
            ("HEP1", "系统设置"),
            ("HBMS1", "BMS"),
            ("HTEMP", "温度"),
            ("HGEN", "发电量"),
        };

        var groups = new List<ChartPropertyGroup>();
        foreach (var (module, label) in order)
        {
            var items = ChartPropertyMap
                .Where(kv => string.Equals(kv.Value.Module, module, StringComparison.OrdinalIgnoreCase))
                .Select(kv => new ChartPropertyOption { Name = kv.Key, Unit = kv.Value.Unit })
                .ToList();

            if (items.Count > 0)
                groups.Add(new ChartPropertyGroup { Module = module, Label = label, Items = items });
        }

        return groups;
    }

    // ============================================================
    // 历史曲线与月发电量
    // ============================================================

    public async Task<HistorySeries> GetHistoryAsync(string productKey, string deviceKey, string property, string range)
    {
        if (string.IsNullOrWhiteSpace(productKey) || string.IsNullOrWhiteSpace(deviceKey))
            return HistorySeries.Fail("参数缺失");

        if (string.IsNullOrWhiteSpace(property) || !ChartPropertyMap.TryGetValue(property, out var target))
            return HistorySeries.Fail($"不支持的图表属性: {property}");

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // 24h 对齐「当日 00:00~24:00」整点横坐标：从当天 0 点起取数
        long start;
        if (range == "24h" || string.IsNullOrWhiteSpace(range))
        {
            start = new DateTimeOffset(DateTime.Today).ToUnixTimeMilliseconds();
        }
        else
        {
            var hours = range switch
            {
                "1h" => 1,
                "6h" => 6,
                "7d" => 168,
                _ => 24
            };
            start = now - (long)hours * 3600 * 1000;
        }

        try
        {
            var historyJson = await _deviceService.GetDevicePropertyHistoryAsync(productKey, deviceKey, target.Module, start, now, pageSize: 1000);
            if (string.IsNullOrWhiteSpace(historyJson))
                return HistorySeries.Fail("获取历史数据为空");

            using var hdoc = JsonDocument.Parse(historyJson);
            var hroot = hdoc.RootElement;

            if (!hroot.TryGetProperty("data", out var hdata) ||
                hdata.ValueKind != JsonValueKind.Object ||
                !hdata.TryGetProperty("records", out var hrecords) ||
                hrecords.ValueKind != JsonValueKind.Array ||
                hrecords.GetArrayLength() == 0)
            {
                return new HistorySeries { Success = true, PropertyName = target.Field, Unit = target.Unit, Message = "暂无历史数据" };
            }

            var points = new List<(long Time, double Value)>();

            foreach (var rec in hrecords.EnumerateArray())
            {
                if (!rec.TryGetProperty("time", out var tEl) ||
                tEl.ValueKind != JsonValueKind.Number || !tEl.TryGetInt64(out var time))
                    continue;

                if (!rec.TryGetProperty("value", out var valEl) || valEl.ValueKind != JsonValueKind.Object)
                    continue;

                string? raw = null;
                foreach (var propEl in valEl.EnumerateObject())
                {
                    if (propEl.Value.ValueKind == JsonValueKind.String &&
                        string.Equals(propEl.Name, target.Module, StringComparison.OrdinalIgnoreCase))
                    {
                        raw = propEl.Value.GetString();
                        break;
                    }
                }

                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                var fields = _protocolParsingService.Parse(target.Module, raw);
                if (fields == null || fields.Count == 0 || !fields.TryGetValue(target.Field, out var fieldVal))
                    continue;

                if (!TryParseNumber(fieldVal, out var num))
                    continue;

                points.Add((time, num));
            }

            var ordered = points
                .OrderBy(p => p.Time)
                .Select(p => new HistoryPoint { Time = p.Time, Value = p.Value })
                .ToList();

            return new HistorySeries { Success = true, Points = ordered, PropertyName = target.Field, Unit = target.Unit };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取图表数据失败: {Property}", property);
            return HistorySeries.Fail($"获取图表数据失败: {ex.Message}");
        }
    }

    public async Task<DailyEnergySeries> GetDailyEnergyAsync(string productKey, string deviceKey, string month, List<TslPropertyViewModel> tslProperties)
    {
        if (string.IsNullOrWhiteSpace(productKey) || string.IsNullOrWhiteSpace(deviceKey))
            return DailyEnergySeries.Fail("参数缺失");

        // month 形如 "2026-09"，缺省取当月
        if (!DateTime.TryParse(month + "-01", out var monthStart))
            monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        var monthEnd = monthStart.AddMonths(1);
        var start = new DateTimeOffset(monthStart).ToUnixTimeMilliseconds();
        var end = new DateTimeOffset(monthEnd).ToUnixTimeMilliseconds();

        try
        {
            var historyJson = await _deviceService.GetDeviceHistoryDataAsync(productKey, deviceKey, start, end);
            if (string.IsNullOrWhiteSpace(historyJson))
                return EmptyDailyResult(monthStart);

            using var hdoc = JsonDocument.Parse(historyJson);
            var hroot = hdoc.RootElement;

            if (!hroot.TryGetProperty("data", out var hdata) || hdata.ValueKind != JsonValueKind.Object || !hdata.TryGetProperty("records", out var hrecords) || hrecords.ValueKind != JsonValueKind.Array)
            {
                return EmptyDailyResult(monthStart);
            }

            var moduleTsl = tslProperties.FirstOrDefault(t =>
                string.Equals(t.Identifier, "HGEN", StringComparison.OrdinalIgnoreCase));
            if (moduleTsl == null || !moduleTsl.PropId.HasValue)
                return DailyEnergySeries.Fail("未找到模块 HGEN 的物模型属性");

            var moduleId = moduleTsl.PropId.Value;

            // 每天保留 createTime 最大的一条「日发电量」
            var dailyLast = new Dictionary<DateTime, (long Time, double Value)>();

            foreach (var rec in hrecords.EnumerateArray())
            {
                if (!rec.TryGetProperty("msgType", out var mt) || mt.ValueKind != JsonValueKind.String || !string.Equals(mt.GetString(), "up", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!rec.TryGetProperty("createTime", out var ct) || ct.ValueKind != JsonValueKind.Number || !ct.TryGetInt64(out var createTime))
                    continue;

                if (!rec.TryGetProperty("dmData", out var dm))
                    continue;

                var idMap = ParseDmDataSafely(dm);
                if (!idMap.TryGetValue(moduleId, out var raw) || string.IsNullOrWhiteSpace(raw))
                    continue;

                var fields = _protocolParsingService.Parse("HGEN", raw);
                if (fields == null || !fields.TryGetValue("日发电量", out var fieldVal))
                    continue;

                if (!TryParseNumber(fieldVal, out var num))
                    continue;

                var day = DateTimeOffset.FromUnixTimeMilliseconds(createTime).LocalDateTime.Date;
                if (!dailyLast.TryGetValue(day, out var existing) || createTime > existing.Time)
                    dailyLast[day] = (createTime, num);
            }

            var labels = new List<string>();
            var values = new List<double>();
            for (var d = monthStart; d < monthEnd; d = d.AddDays(1))
            {
                labels.Add(d.Day.ToString("D2"));
                values.Add(dailyLast.TryGetValue(d, out var v) ? Math.Round(v.Value, 2) : 0);
            }

            return new DailyEnergySeries { Success = true, Labels = labels, Values = values, Unit = "kWh" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取日发电量失败: {Month}", month);
            return DailyEnergySeries.Fail(ex.Message);
        }
    }

    /// <summary>
    /// 整月无数据时返回全 0，避免前端画空图。
    /// </summary>
    private static DailyEnergySeries EmptyDailyResult(DateTime monthStart)
    {
        var labels = new List<string>();
        var values = new List<double>();
        var monthEnd = monthStart.AddMonths(1);
        for (var d = monthStart; d < monthEnd; d = d.AddDays(1))
        {
            labels.Add(d.Day.ToString("D2"));
            values.Add(0);
        }
        return new DailyEnergySeries { Success = true, Labels = labels, Values = values, Unit = "kWh" };
    }

    /// <summary>
    /// 解析「数值+单位」字符串为数值
    /// </summary>
    private static bool TryParseNumber(string text, out double number)
    {
        number = 0;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var s = text.Trim();
        var end = 0;
        while (end < s.Length && (char.IsDigit(s[end]) || s[end] == '.' || s[end] == '-' || s[end] == '+'))
            end++;

        if (end == 0)
            return false;

        return double.TryParse(
            s.Substring(0, end),
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out number);
    }
    // ============================================================
    // 数据明细（每一条上报记录）
    // ============================================================

    /// <summary>
    /// 数据明细列定义：与「数据总览」字段完全一致，复用 FieldOrder / FieldRenameMap。
    /// 返回 (表头显示名, 工程字段名) 的有序列表。
    /// </summary>
    private static List<(string Label, string FieldKey)> GetDataDetailColumnDefs()
    {
        var defs = new List<(string, string)>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var groupName in GroupOrder)
        {
            if (!FieldOrder.TryGetValue(groupName, out var displayNames))
                continue;

            foreach (var displayName in displayNames)
            {
                var fieldKey = FieldRenameMap.TryGetValue(displayName, out var renamed)
                    ? renamed
                    : displayName;

                if (seen.Add(fieldKey))
                    defs.Add((displayName, fieldKey));
            }
        }

        return defs;
    }

    /// <summary>
    /// 获取数据明细：时间区间内每一条上行（up）记录解析为一行，按时间倒序。
    /// 列与「数据总览」字段保持一致（固定清单 + 解析器额外输出字段追加到末尾）。
    /// </summary>
    public async Task<DataDetailResult> GetDataDetailAsync(string productKey, string deviceKey, long start, long end, List<TslPropertyViewModel> tslProperties)
    {
        Console.WriteLine($"DataDetail request {productKey}/{deviceKey} {start}-{end}");
        if (string.IsNullOrWhiteSpace(productKey) || string.IsNullOrWhiteSpace(deviceKey))
            return DataDetailResult.Fail("参数缺失");

        try
        {
            var historyJson = await _deviceService.GetDeviceHistoryDataAsync(productKey, deviceKey, start, end, limit: 2000);
            Console.WriteLine($"DataDetail history length={historyJson?.Length}");
            Console.WriteLine(historyJson.Substring(0, Math.Min(1000, historyJson.Length)));
            if (string.IsNullOrWhiteSpace(historyJson))
                return DataDetailResult.Fail("获取历史数据为空");

            using var doc = JsonDocument.Parse(historyJson);
            var root = doc.RootElement;

            var colDefs = GetDataDetailColumnDefs();
            var knownFieldKeys = new HashSet<string>(colDefs.Select(c => c.FieldKey), StringComparer.Ordinal);

            if (!root.TryGetProperty("data", out var data) ||
                data.ValueKind != JsonValueKind.Object ||
                !data.TryGetProperty("records", out var records) ||
                records.ValueKind != JsonValueKind.Array)
            {
                return new DataDetailResult
                {
                    Success = true,
                    Columns = colDefs.Select(c => new DataDetailColumn { Label = c.Label }).ToList(),
                    Rows = new List<DataDetailRow>()
                };
            }
            Console.WriteLine($"DataDetail records={records.GetArrayLength()}");

            // 物模型数字 id -> 模块标识符
            var idToIdentifier = new Dictionary<int, string>();
            foreach (var tsl in tslProperties)
            {
                if (tsl.PropId.HasValue && !string.IsNullOrWhiteSpace(tsl.Identifier))
                    idToIdentifier[tsl.PropId.Value] = tsl.Identifier;
            }

            // 第一遍：解析每条 up 记录，打平字段
            var parsed = new List<(long Time, Dictionary<string, string> Fields)>();
            var extraFields = new List<string>();
            var extraSeen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var rec in records.EnumerateArray())
            {
                // 只记录设备上报（up），过滤平台下行（down）
                if (!rec.TryGetProperty("msgType", out var mt) ||
                    mt.ValueKind != JsonValueKind.String ||
                    !string.Equals(mt.GetString(), "up", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                long time = 0;
                if (rec.TryGetProperty("createTime", out var ct) &&
                    ct.ValueKind == JsonValueKind.Number &&
                    ct.TryGetInt64(out var t))
                {
                    time = t;
                }

                if (!rec.TryGetProperty("dmData", out var dm))
                    continue;

                var idMap = ParseDmDataSafely(dm);
                if (idMap.Count == 0)
                    continue;

                // 打平该条记录的所有解析字段（与 BuildFieldGroups 一致）
                var fields = new Dictionary<string, string>(StringComparer.Ordinal);
                var machineRaw = string.Empty;

                foreach (var (id, raw) in idMap)
                {
                    if (!idToIdentifier.TryGetValue(id, out var identifier))
                        continue;

                    if (string.Equals(identifier, "MachineType", StringComparison.OrdinalIgnoreCase))
                    {
                        machineRaw = raw;
                        continue;
                    }

                    if (_protocolParsingService.Supports(identifier))
                    {
                        var parsedFields = _protocolParsingService.Parse(identifier, raw);
                        if (parsedFields != null)
                        {
                            foreach (var kv in parsedFields)
                                fields[kv.Key] = kv.Value;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(raw))
                    {
                        fields[identifier] = raw;
                    }
                }

                if (!string.IsNullOrWhiteSpace(machineRaw))
                {
                    var machineType = _commandEncodingService.ResolveMachineModel(machineRaw);
                    fields["MachineType"] = string.IsNullOrWhiteSpace(machineType) ? machineRaw : machineType;
                }

                // 收集固定清单之外、解析器额外输出的字段（追加到「更多」末尾）
                foreach (var key in fields.Keys)
                {
                    if (!knownFieldKeys.Contains(key) && extraSeen.Add(key))
                        extraFields.Add(key);
                }

                parsed.Add((time, fields));
            }

            // 列 = 固定清单（与数据总览一致）+ 额外字段
            var columns = colDefs
                .Select(c => new DataDetailColumn { Label = c.Label })
                .ToList();
            columns.AddRange(extraFields.Select(k => new DataDetailColumn { Label = k }));

            // 第二遍：按完整列组装每行 cells
            var rows = new List<DataDetailRow>(parsed.Count);
            foreach (var (time, fields) in parsed)
            {
                var row = new DataDetailRow { Time = time };
                foreach (var def in colDefs)
                {
                    row.Cells.Add(fields.TryGetValue(def.FieldKey, out var v) ? v : string.Empty);
                }
                foreach (var extra in extraFields)
                {
                    row.Cells.Add(fields.TryGetValue(extra, out var v) ? v : string.Empty);
                }
                rows.Add(row);
            }

            rows.Sort((a, b) => b.Time.CompareTo(a.Time));

            return new DataDetailResult { Success = true, Columns = columns, Rows = rows };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取数据明细失败");
            return DataDetailResult.Fail($"获取数据明细失败: {ex.Message}");
        }
    }
}
