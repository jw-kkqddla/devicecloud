// ===== 控制（设置参数下发）=====
        const settings = [
        { code: 'OutputVoltage', label: '输出电压', type: 'select', options: [{v:'220',l:'220V'},{v:'230',l:'230V'},{v:'240',l:'240V'}], compact: true },
        { code: 'TotalChargeCurrent', label: '总充电电流', type: 'number', unit: 'A' },
        { code: 'AcChargeCurrent', label: '市电充电电流', type: 'number', unit: 'A', compact: true },
        { code: 'StrongChargeVoltage', label: '强充电压', type: 'number', unit: 'V', compact: true },
        { code: 'FloatChargeVoltage', label: '浮充电压', type: 'number', unit: 'V', compact: true },
        { code: 'LowPowerLockVoltage', label: '低电锁机电压', type: 'number', unit: 'V', compact: true },
        { code: 'BmsReturnToAcSoc', label: '返回市电模式SOC(BMS)', type: 'number', unit: '%' },
        { code: 'BmsLowPowerSoc', label: '低电锁机SOC(BMS)', type: 'number', unit: '%' },
        { code: 'BmsReturnToBatterySoc', label: '返回电池模式SOC(BMS)', type: 'number', unit: '%' },
        { code: 'BmsAutoStartSoc', label: '恢复重启SOC(BMS)', type: 'number', unit: '%' },
        { code: 'BattLowAlarmVolt', label: '电池低电告警电压', type: 'number', unit: 'V', compact: true },
        { code: 'ReturnMainsBatteryVoltage', label: '返回市电模式电压', type: 'number', unit: 'V' },
        { code: 'ReturnBatteryModeVoltage', label: '返回电池模式电压', type: 'number', unit: 'V' },
        { code: 'BatteryBalancingVoltage', label: '电池均衡电压', type: 'number', unit: 'V' },
        { code: 'BatteryBalancingTime', label: '电池均衡时间值', type: 'number', unit: '分钟' },
        { code: 'BatteryBalancingTimeout', label: '电池均衡超时值', type: 'number', unit: '分钟' },
        { code: 'BatteryBalancingInterval', label: '电池均衡间隔时间', type: 'number', unit: '天' },
        { code: 'SecondOutputDischargeTime', label: '第二输出的放电时间', type: 'number', unit: 'Min' },
        { code: 'SecondOutputDelayTime', label: '第二输出的延时时间', type: 'number', unit: 'Min' },
        { code: 'SecondOutputRestoreCapacity', label: '第二输出的电池容量', type: 'number', unit: '%' },
        { code: 'SecondOutputRestoreVoltage', label: '第二输出的电池电压', type: 'number', unit: 'V' },
        { code: 'ParallelModeShutdownVoltage', label: '第二输出关闭电压值', type: 'number', unit: 'V' },
        { code: 'ParallelModeShutdownSoc', label: '第二输出关闭电池SOC', type: 'number', unit: '%' },
        { code: 'WorkingMode', label: '工作模式', type: 'select', options: [{v:'UTI',l:'UTI'},{v:'SUB',l:'SUB'},{v:'SBU',l:'SBU'}] },
        { code: 'OverloadRestart', label: '过载重启', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'OverTemperatureRestart', label: '过温重启', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'LcdBacklight', label: 'LCD背光', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'OutputSettingFrequency', label: '系统频率', type: 'select', options: [{v:'50',l:'50Hz'},{v:'60',l:'60Hz'}], compact: true },
        { code: 'BatteryType', label: '电池类型', type: 'select', options: [{v:'AGM',l:'AGM'},{v:'FLD',l:'FLD'},{v:'USER',l:'USER'},{v:'LIA',l:'LIA'},{v:'PYL',l:'PYL'},{v:'TQF',l:'TQF'},{v:'GRO',l:'GRO'},{v:'LIB',l:'LIB'},{v:'LIC',l:'LIC'}], compact: true },
        { code: 'ChargingPriority', label: '充电模式', type: 'select', options: [{v:'CSO',l:'CSO'},{v:'SNU',l:'SNU'},{v:'OSO',l:'OSO'}] },
        { code: 'BmsCommunicationControl', label: 'BMS开关', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'BuzzerStatus', label: '蜂鸣器状态', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'StatePromptTone', label: '输入源提示', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'AutoReturnHome', label: '自动返回首页', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'AcInputRange', label: '市电输入范围', type: 'select', options: [{v:'APL',l:'APL(宽范围)'},{v:'UPS',l:'UPS(窄范围)'}], compact: true },
        { code: 'BatteryBalancingMmode', label: '电池均衡模式', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'DualOutputMode', label: '双输出模式', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'GridConnectedFunction', label: '并网功能', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'GridCurrent', label: '并网电流', type: 'number', unit: 'A' },
        { code: 'PvGridProtocol', label: '并网协议', type: 'select', options: [{v:'India',l:'India'},{v:'Germen',l:'Germen'},{v:'SouthAmerica',l:'SouthAmerica'},{v:'Pakistan',l:'Pakistan'}] },
        { code: 'PvFeedPriority', label: 'PV馈能优先级', type: 'select', options: [{v:'BLU',l:'BLU'},{v:'LBU',l:'LBU'}] },
        { code: 'OutputMode', label: '输出模式', type: 'select', options: [{v:'SIG',l:'SIG'},{v:'PAL',l:'PAL'},{v:'3P1',l:'3P1'},{v:'3P2',l:'3P2'},{v:'3P3',l:'3P3'}] }
        ]
        
        
        function renderSettingGroup(tbody, title, settings) {
            const titleTr = document.createElement('tr');
            titleTr.innerHTML = '<td colspan="3" class="table-secondary fw-bold">' + title + '</td>';
            tbody.appendChild(titleTr);
            settings.forEach(s => {
                const tr = document.createElement('tr');
                tr.dataset.code = s.code;
                let valueCell;
                if (s.type === 'select') {
                    valueCell = '<select class="form-select form-select-sm setting-value">' +
                        s.options.map(o => '<option value="' + o.v + '">' + o.l + '</option>').join('') + '</select>';
                } else if (s.type === 'button') {
                    valueCell = '';
                } else if (s.type === 'text') {
                    valueCell = '<input type="text" class="form-control form-control-sm setting-value" placeholder="' + (s.placeholder || '') + '" />';
                } else {
                    valueCell = '<input type="number" step="0.1" class="form-control form-control-sm setting-value" placeholder="' + (s.unit || '') + '" />';
                }
                tr.innerHTML =
                    '<td>' + s.label + '<br><small class="text-muted">' + s.code + '</small></td>' +
                    '<td>' + valueCell + '</td>' +
                    '<td><button type="button" class="btn btn-sm btn-outline-primary" onclick="sendSettingRow(this, \'' + s.code + '\')">下发</button></td>';
                tbody.appendChild(tr);
            });
        }

        function isBmsModel(model) {
            return model && model.startsWith('BMS');
        }

        // 设置项数组统一为 settings
        const allSettingItems = [...new Map(settings.map(s => [s.code, s])).values()];

        // 精简机型（CYJ/LB6/CG000001）支持的设置项
        const compactSettings = allSettingItems.filter(s => s.compact);

        // 每个机型独立一份设置项数组（展开拷贝，后续可单独增删某个机型）
        const HPVINV02Settings = [
        ...allSettingItems,
        { code: 'SystemTime', label: '系统时间', type: 'text', placeholder: 'YYMMDDhhmmss' }];
        const HPVINV04Settings = HPVINV02Settings.filter(s =>!['WorkingMode','OverloadRestart','OverTemperatureRestart','LcdBacklight','OutputSettingFrequency'].includes(s.code));
        const HPVINV06Settings = [
        { code: 'BatteryType', label: '电池类型', type: 'select', options: [{v:'AGM',l:'AGM'},{v:'FLD',l:'FLD'},{v:'USER',l:'USER'},{v:'LIA',l:'LIA'},{v:'PYL',l:'PYL'},{v:'TQF',l:'TQF'},{v:'GRO',l:'GRO'},{v:'LIB',l:'LIB'},{v:'LIC',l:'LIC'}], compact: true },
        { code: 'LowPowerLockVoltage', label: '低电锁机电压', type: 'number', unit: 'V', compact: true },
        { code: 'WorkingMode', label: '工作模式', type: 'select', options: [{v:'UTI',l:'UTI'},{v:'SUB',l:'SUB'},{v:'SBU',l:'SBU'}] },
        { code: 'StrongChargeVoltage', label: '强充电压', type: 'number', unit: 'V', compact: true },
        { code: 'FloatChargeVoltage', label: '浮充电压', type: 'number', unit: 'V', compact: true },
        {code: 'TotalChargeCurrent', label: '总充电电流', type: 'number', unit: 'A' },
        {code: 'AcChargeCurrent', label: '市电充电电流', type: 'number', unit: 'A', compact: true },
        { code: 'ReturnMainsBatteryVoltage', label: '返回市电模式电压', type: 'number', unit: 'V' },
        { code: 'ReturnBatteryModeVoltage', label: '返回电池模式电压', type: 'number', unit: 'V' },
        { code: 'BuzzerStatus', label: '蜂鸣器状态', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'OverloadRestart', label: '过载重启', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'OverTemperatureRestart', label: '过温重启', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'LcdBacklight', label: 'LCD背光', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        {code: 'OutputVoltage', label: '输出电压', type: 'select', options: [{v:'220',l:'220V'},{v:'230',l:'230V'},{v:'240',l:'240V'}], compact: true },
        { code: 'BmsCommunicationControl', label: 'BMS开关', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'BatteryBalancingInterval', label: '电池均衡间隔时间', type: 'number', unit: '天' },
        { code: 'OutputMode', label: '输出模式', type: 'select', options: [{v:'SIG',l:'SIG'},{v:'PAL',l:'PAL'},{v:'3P1',l:'3P1'},{v:'3P2',l:'3P2'},{v:'3P3',l:'3P3'}] },
        { code: 'SecondOutputDischargeTime', label: '第二输出放电时间', type: 'number', unit: 'Min' },
        { code: 'SecondOutputDelayTime', label: '第二输出的延时时间', type: 'number', unit: 'Min' },
        { code: 'SecondOutputRestoreCapacity', label: '第二输出的电池容量', type: 'number', unit: '%' },
        { code: 'SecondOutputRestoreVoltage', label: '第二输出的电池电压', type: 'number', unit: 'V' },
        { code: 'AcInputRange', label: '市电输入范围', type: 'select', options: [{v:'APL',l:'APL(宽范围)'},{v:'UPS',l:'UPS(窄范围)'}], compact: true },
        { code: 'ChargingPriority', label: '充电模式', type: 'select', options: [{v:'CSO',l:'CSO'},{v:'SNU',l:'SNU'},{v:'OSO',l:'OSO'}] },
        { code: 'BmsLowPowerSoc', label: '低电锁机SOC(BMS)', type: 'number', unit: '%' },
        { code: 'BmsReturnToAcSoc', label: '返回市电模式SOC(BMS)', type: 'number', unit: '%' },
        { code: 'BmsReturnToBatterySoc', label: '返回电池模式SOC(BMS)', type: 'number', unit: '%' },
        { code: 'BmsAutoStartSoc', label: '恢复重启SOC(BMS)', type: 'number', unit: '%' },
        { code: 'OverloadByPass', label: '市电过载转旁路', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'OutputSettingFrequency', label: '系统频率', type: 'select', options: [{v:'50',l:'50Hz'},{v:'60',l:'60Hz'}], compact: true },
        { code: 'GridCurrent', label: '并网电流', type: 'number', unit: 'A' },
        { code: 'GridConnectedFunction', label: '并网功能', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'PvGridProtocol', label: '并网协议', type: 'select', options: [{v:'India',l:'India'},{v:'Germen',l:'Germen'},{v:'SouthAmerica',l:'SouthAmerica'},{v:'Pakistan',l:'Pakistan'}] },
        { code: 'PvFeedPriority', label: 'PV馈能优先级', type: 'select', options: [{v:'BLU',l:'BLU'},{v:'LBU',l:'LBU'}] },
        { code: 'AcChargeTime', label: 'AC充电时间', type: 'text', placeholder: '如 10-23' },
        { code: 'DualOutputMode', label: '双输出模式', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'ParallelModeShutdownVoltage', label: '第二输出关闭电压值', type: 'number', unit: 'V' },
        { code: 'ParallelModeShutdownSoc', label: '第二输出关闭电池SOC', type: 'number', unit: '%' },
        { code: 'BatteryBalancingMmode', label: '电池均衡模式', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'BatteryBalancingVoltage', label: '电池均衡电压', type: 'number', unit: 'V' },
        { code: 'BatteryBalancingTime', label: '电池均衡时间值', type: 'number', unit: '分钟' },
        { code: 'BatteryBalancingTimeout', label: '电池均衡超时值', type: 'number', unit: '分钟' },
        { code: 'SystemTime', label: '系统时间', type: 'text', placeholder: 'YYMMDDhhmmss' }
        ];
        const HPVINV07Settings = [{ code: 'GridCurrent', label: '并网电流', type: 'number', unit: 'A' },
        { code: 'OutputVoltage', label: '输出电压', type: 'select', options: [{v:'220',l:'220V'},{v:'230',l:'230V'},{v:'240',l:'240V'}], compact: true },
        { code: 'ZeroAdjPwr', label: '调零功率', type: 'number', unit: 'W' },
        { code: 'OutputMode', label: '输出模式', type: 'select', options: [{v:'SIG',l:'SIG'},{v:'PAL',l:'PAL'},{v:'3P1',l:'3P1'},{v:'3P2',l:'3P2'},{v:'3P3',l:'3P3'}] },
        { code: 'OutputSettingFrequency', label: '系统频率', type: 'select', options: [{v:'50',l:'50Hz'},{v:'60',l:'60Hz'}], compact: true },
        { code: 'PvGridProtocol', label: '并网协议', type: 'select', options: [{v:'India',l:'India'},{v:'Germen',l:'Germen'},{v:'SouthAmerica',l:'SouthAmerica'},{v:'Pakistan',l:'Pakistan'}] },
        { code: 'AcInputRange', label: '市电输入范围', type: 'select', options: [{v:'APL',l:'APL(宽范围)'},{v:'UPS',l:'UPS(窄范围)'}], compact: true },
        { code: 'CTEnableOperation', label: 'CT功能开关', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'GridConnectedFunction', label: '并网功能', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'OverloadRestart', label: '过载重启', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'OverTemperatureRestart', label: '过温重启', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'LcdBacklight', label: 'LCD背光', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'BuzzerStatus', label: '蜂鸣器状态', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'StatePromptTone', label: '输入源提示', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'AutoReturnHome', label: '自动返回首页', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        { code: 'SystemTime', label: '系统时间', type: 'text', placeholder: 'YYMMDDhhmmss' }
        ];
        const HPVINV08Settings = [
        ...HPVINV02Settings.filter(s => !['GridConnectedFunction','GridCurrent','PvFeedPriority','OutputMode'].includes(s.code)),
        { code: 'ZeroAdjPwr', label: '调零功率', type: 'number', unit: 'W' },
        { code: 'GridPower', label: '并网功率', type: 'number', unit: 'W' },
        { code: 'DisChargeCurrentLimit', label: '电池放电电流限制', type: 'number', unit: 'A' },
        { code: 'SystemTime', label: '系统时间', type: 'text', placeholder: 'YYMMDDhhmmss' }
        ];
        const HPVINV09Settings = HPVINV08Settings;
        const HPVINV10Settings = HPVINV08Settings;
        const LPVINV02Settings = [
        { code: 'OutputSettingFrequency', label: '输出频率', type: 'select', options: [{v:'50',l:'50Hz'},{v:'60',l:'60Hz'}], compact: true },
        ...allSettingItems.filter(s => !['SecondOutputDischargeTime','SecondOutputDelayTime','SecondOutputRestoreCapacity','SecondOutputRestoreVoltage','DualOutputMode','GridConnectedFunction','GridCurrent','PvGridProtocol','WorkingMode'].includes(s.code)),
        { code: 'FaultSave', label: '故障记录', type: 'select', options: [{v:'on',l:'开启'},{v:'off',l:'关闭'}] },
        ];
        const UPSCYX01Settings = [...compactSettings];
        const LB6Settings = [...compactSettings];
        const CG000001Settings = [...compactSettings];


        // 机型 → 设置项映射
        const settingsMap = {
            HPVINV02: HPVINV02Settings, HPVINV04: HPVINV04Settings, HPVINV06: HPVINV06Settings,
            HPVINV07: HPVINV07Settings, HPVINV08: HPVINV08Settings, HPVINV09: HPVINV09Settings,
            HPVINV10: HPVINV10Settings, LPVINV02: LPVINV02Settings,
            UPSCYX01: UPSCYX01Settings, LB6: LB6Settings, CG000001: CG000001Settings,
            BMS01: [], BMS02: [], BMS03: [],
        };

        function renderSettingGroups(model) {
            const tbody = document.querySelector('#heep1SettingsTable tbody');
            if (!tbody) return;
            tbody.innerHTML = '';
            const items = settingsMap[model] || [];
            if (items.length === 0) return;
            renderSettingGroup(tbody, '参数设定', items);
        }

        document.addEventListener('DOMContentLoaded', function() {
            const sel = document.getElementById('controlMachineType');
            if (!sel) return;
            renderSettingGroups(sel.value);
            onMachineTypeChange();
        });

        function onMachineTypeChange() {
            const mt = document.getElementById('controlMachineType').value;
            renderSettingGroups(mt);
            const table = document.getElementById('heep1SettingsTable');
            const bmsHint = document.getElementById('bmsNotSupported');
            const batchBtn = document.querySelector('#controlDeviceModal .modal-footer .btn-primary');
            const isBms = isBmsModel(mt);
            if (table) table.style.display = isBms ? 'none' : '';
            if (bmsHint) bmsHint.style.display = isBms ? 'block' : 'none';
            if (batchBtn) batchBtn.disabled = isBms;
        }

        function controlDevice() {
            const modal = new bootstrap.Modal(document.getElementById('controlDeviceModal'));
            modal.show();
        }

        async function sendHeep1Setting(code, value) {
            const machineType = document.getElementById('controlMachineType').value;
            const resp = await fetch('?handler=SendSetting', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: JSON.stringify({
                    productKey: window.__deviceDetail.productKey,
                    deviceKey: window.__deviceDetail.deviceKey,
                    setting: code,
                    value: String(value),
                    machineType: machineType
                })
            });
            return await resp.json();
        }

        async function sendSettingRow(btn, code) {
            const row = btn.closest('tr');
            const input = row.querySelector('.setting-value');
            const value = input ? input.value.trim() : '';
            if (!value && input) { alert('请输入值'); return; }
            const origText = btn.textContent;
            btn.disabled = true;
            btn.textContent = '下发中...';
            try {
                const data = await sendHeep1Setting(code, value);
                if (data.success) {
                    btn.textContent = '✓ 已下发';
                    setTimeout(() => { btn.textContent = origText; btn.disabled = false; }, 1500);
                } else {
                    alert('下发失败: ' + data.message);
                    btn.textContent = origText;
                    btn.disabled = false;
                }
            } catch (e) {
                alert('错误: ' + e.message);
                btn.textContent = origText;
                btn.disabled = false;
            }
        }

        async function sendAllSettings() {
            const rows = document.querySelectorAll('#heep1SettingsTable tbody tr');
            let success = 0, failed = 0;
            for (const row of rows) {
                const input = row.querySelector('.setting-value');
                const value = input.value.trim();
                if (!value) continue;
                const code = row.dataset.code;
                try {
                    const data = await sendHeep1Setting(code, value);
                    if (data.success) success++;
                    else failed++;
                } catch (e) {
                    failed++;
                }
            }
            alert('批量下发完成：成功 ' + success + '，失败 ' + failed);
        }
