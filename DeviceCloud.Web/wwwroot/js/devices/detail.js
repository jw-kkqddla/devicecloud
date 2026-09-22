// ============================================================
// 图表相关
// ============================================================
let historyChart = null;

// ============================================================
// 月发电量柱状图
// ============================================================
let monthlyEnergyChart = null;

function initMonthlyEnergyChart() {
    const canvas = document.getElementById('monthlyEnergyChart');
    if (!canvas) return;

    monthlyEnergyChart = new Chart(canvas, {
        type: 'bar',
        data: {
            labels: [],
            datasets: [{
                label: '日发电量',
                data: [],
                backgroundColor: '#3b82f6',
                borderRadius: 4,
                maxBarThickness: 32
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: ctx => `${ctx.parsed.y} kWh`
                    }
                }
            },
            scales: {
                x: {
                    grid: { display: false },
                    ticks: { font: { size: 11 } },
                    title: { display: true, text: '日', font: { size: 11 } }
                },
                y: {
                    beginAtZero: true,
                    grid: { color: false },
                    ticks: { font: { size: 11 } },
                    title: { display: true, text: 'kWh', font: { size: 11 } }
                }
            }
        }
    });
}

async function refreshMonthlyEnergy() {
    const picker = document.getElementById('energyMonthPicker');
    if (!picker || !monthlyEnergyChart) return;

    // 默认当月
    if (!picker.value) {
        const now = new Date();
        picker.value = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;
    }

    const month = picker.value;

    try {
        const url = '?handler=DailyEnergy'
            + '&month=' + encodeURIComponent(month)
            + '&ProductKey=' + encodeURIComponent('@Model.ProductKey')
            + '&DeviceKey=' + encodeURIComponent('@Model.DeviceKey');

        const resp = await fetch(url);
        const data = await resp.json();

        if (!data.success) {
            console.warn('月发电量获取失败:', data.message);
            return;
        }

        monthlyEnergyChart.data.labels = data.labels || [];
        monthlyEnergyChart.data.datasets[0].data = data.values || [];
        monthlyEnergyChart.update();
    } catch (e) {
        console.error('月发电量请求失败:', e);
    }
}

        function initChart() {
            const ctx = document.getElementById('historyChart');
            if (!ctx) return;

            historyChart = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: [],
                    datasets: [{
                        label: '数据值',
                        data: [],
                        borderColor: '#7c3aed',
                        backgroundColor: '#7c3aed',
                        borderWidth: 2,
                        fill: false,
                        tension: 0,
                        pointRadius: 0,
                        pointHoverRadius: 4,
                        spanGaps: true
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: false,
                            position: 'bottom',
                            labels: {
                                usePointStyle: true,
                                pointStyle: 'circle',
                                boxWidth: 8,
                                boxHeight: 8,
                                padding: 14,
                                color: '#6b7280',
                                font: { size: 12 }
                            }
                        },
                        tooltip: { mode: 'index', intersect: false }
                    },
                    scales: {
                        x: {
                            type: 'time',
                            time: {
                                unit: 'hour',
                                stepSize: 1,
                                tooltipFormat: 'yyyy-MM-dd HH:mm:ss',
                                displayFormats: { minute: 'HH:mm', hour: 'HH', day: 'MM-dd' }
                            },
                            grid: {
                                display: false,       
                                drawBorder: false
                            },
                            border: {
                                display: true,        
                                color: '#d1d5db',     
                                width: 1
                            },
                            title: {
                                display: true,       
                                text: '',            
                                color: '#6b7280',
                                font: { size: 11 },
                                padding: { top: 6 }
                            }
                        },
                        y: {
                            display: true,
                            beginAtZero: false,
                            grid: {
                                display: false,        
                                color: '#f3f4f6',     
                                drawBorder: false
                            },
                            border: {
                                display: true,        
                                color: '#d1d5db',
                                width: 1
                            },
                            ticks: {
                                font: { size: 11 },
                                precision: 0,
                                color: '#6b7280',
                                padding: 8
                            },
                            title: {
                                display: true,
                                text: '',
                                position: 'left',
                                rotation: -90,
                                font: { size: 11 },
                                color: '#6b7280'
                            }
                        }
                    }
                }
            });
        }

        // ============================================================
        // 横坐标按时间范围固定刻度：24h 为当日 00~24 整点（00,01,...,23,00）
        // ============================================================
        function configureChartScale(range) {
            if (!historyChart) return;
            const x = historyChart.options.scales.x;
            const t = x.time;
            const now = Date.now();

            if (range === '7d') {
                x.min = now - 7 * 24 * 3600 * 1000;
                x.max = now;
                t.unit = 'day';
                t.stepSize = 1;
                t.displayFormats = { day: 'MM-dd' };
                x.ticks.autoSkip = true;
            } else if (range === '6h') {
                x.min = now - 6 * 3600 * 1000;
                x.max = now;
                t.unit = 'hour';
                t.stepSize = 1;
                t.displayFormats = { hour: 'HH:mm' };
                x.ticks.autoSkip = false;
            } else if (range === '1h') {
                x.min = now - 3600 * 1000;
                x.max = now;
                t.unit = 'minute';
                t.stepSize = 10;
                t.displayFormats = { minute: 'HH:mm' };
                x.ticks.autoSkip = false;
            } else {
                // 24h：当日 00:00 ~ 次日 00:00
                const d = new Date();
                d.setHours(0, 0, 0, 0);
                x.min = d.getTime();
                x.max = d.getTime() + 24 * 3600 * 1000;
                t.unit = 'hour';
                t.stepSize = 1;
                t.displayFormats = { hour: 'HH' };
                x.ticks.autoSkip = false;
            }
        }

        async function refreshChartData() {
            const property = document.getElementById('chartPropertySelect') ? document.getElementById('chartPropertySelect').value : '';
            const range = document.getElementById('chartTimeRange') ? document.getElementById('chartTimeRange').value : '24h';
            const canvas = document.getElementById('historyChart');
            const placeholder = document.getElementById('chartPlaceholder');

            if (!property) {
                if (canvas) canvas.style.display = 'none';
                if (placeholder) placeholder.style.display = 'flex';
                return;
            }

            if (canvas) canvas.style.display = 'block';
            if (placeholder) placeholder.style.display = 'none';

            configureChartScale(range);

            try {
                const url = '?handler=HistoryData&ProductKey=' + encodeURIComponent(window.__deviceDetail.productKey) + '&DeviceKey=' + encodeURIComponent(window.__deviceDetail.deviceKey) + '&property=' + encodeURIComponent(property) + '&range=' + encodeURIComponent(range);
                const response = await fetch(url);
                const result = await response.json();

                if (result.success) {
                    updateChart({
                        points: result.points || [],
                        propertyName: result.propertyName || property,
                        unit: result.unit || ''
                    });
                } else {
                    updateChart({ points: [], propertyName: property });
                    console.warn(result.message || '获取图表数据失败');
                }
            } catch (error) {
                console.error('获取图表数据失败:', error);
                updateChart({ labels: [], values: [], propertyName: property });
            }
        }

        // 尝试解析历史数据返回的任意结构，寻找时间/值对
        function parseHistoryResponse(obj, identifier) {
            if (!obj) return null;

            // 如果直接是数组
            if (Array.isArray(obj)) {
                return extractFromArray(obj, identifier);
            }

            // 如果是对象并包含常见容器
            const candidates = ['records', 'list', 'data', 'rows', 'result', 'series'];
            for (const key of candidates) {
                if (obj[key]) {
                    if (Array.isArray(obj[key])) return extractFromArray(obj[key], identifier);
                    if (obj[key].records && Array.isArray(obj[key].records)) return extractFromArray(obj[key].records, identifier);
                }
            }

            // 深度遍历查找第一个可用数组
            const visited = new Set();
            let found = null;
            function traverse(o) {
                if (!o || typeof o !== 'object' || visited.has(o)) return;
                visited.add(o);
                for (const k in o) {
                    if (!Object.prototype.hasOwnProperty.call(o, k)) continue;
                    const v = o[k];
                    if (Array.isArray(v) && v.length > 0) {
                        const res = extractFromArray(v, identifier);
                        if (res) { found = res; return; }
                    } else if (typeof v === 'object') {
                        traverse(v);
                        if (found) return;
                    }
                }
            }
            traverse(obj);
            return found;
        }

        function extractFromArray(arr, identifier) {
            if (!Array.isArray(arr) || arr.length === 0) return null;

            // 找到第一个包含时间和值的元素
            const candidates = arr.filter(el => el && typeof el === 'object');
            if (candidates.length === 0) return null;

            // 常见字段名映射
            const valueKeys = ['value', 'val', 'v', 'data', identifier];
            const timeKeys = ['time', 'ts', 'timestamp', 'timeLocal', 't'];

            const labels = [];
            const values = [];

            for (const item of candidates) {
                let val = null;
                for (const vk of valueKeys) {
                    if (vk && item[vk] !== undefined) { val = item[vk]; break; }
                }
                // 有时值会在 item.data.value
                if (val === null && item.data && (item.data.value !== undefined || item.data.val !== undefined)) {
                    val = item.data.value ?? item.data.val;
                }

                let ts = null;
                for (const tk of timeKeys) {
                    if (tk && item[tk] !== undefined) { ts = item[tk]; break; }
                }

                // 如果没有时间字段但有时间戳在 nested
                if (ts === null && item.timeLocal) ts = item.timeLocal;

                // 解析时间
                let label = '-';
                if (ts !== null) {
                    if (typeof ts === 'number' || (!isNaN(ts) && String(ts).length >= 10)) {
                        const tnum = Number(ts);
                        const date = (String(ts).length > 12) ? new Date(tnum) : new Date(tnum);
                        label = date.toLocaleString();
                    } else {
                        const d = new Date(ts);
                        if (!isNaN(d)) label = d.toLocaleString();
                        else label = String(ts);
                    }
                }

                if (val === null || val === undefined) continue;

                labels.push(label);
                values.push(typeof val === 'object' ? JSON.stringify(val) : val);
            }

            if (labels.length === 0) return null;
            return { labels, values };
        }

        function updateChart(chartData) {
            if (!historyChart || !chartData) return;
            historyChart.data.labels = [];
            historyChart.data.datasets[0].data = (chartData.points || []).map(p => ({
                x: p.time,
                y: p.value
            }));
            historyChart.data.datasets[0].label = chartData.propertyName || '数据值';
            // y 轴标题只显示单位（属性名在底部图例展示）
            historyChart.options.scales.y.title.text = chartData.unit || '';
            historyChart.options.scales.x.title.text = chartData.propertyName || '';
            historyChart.update();
        }

        document.getElementById('chartPropertySelect')?.addEventListener('change', function() {
            if (this.value) refreshChartData();
        });

        document.getElementById('chartTimeRange')?.addEventListener('change', function() {
            refreshChartData();
        });

        document.addEventListener('DOMContentLoaded', function() {
    initChart();
    refreshChartData();

    initMonthlyEnergyChart();
    refreshMonthlyEnergy();
});

        // ============================================================
        // 数据明细：记录每一条上报的数据
        // ============================================================

        function detailPad(n) {
            return String(n).padStart(2, '0');
        }

        function detailFmtDate(d) {
            return `${d.getFullYear()}-${detailPad(d.getMonth() + 1)}-${detailPad(d.getDate())}`;
        }

        function detailFmtTime(ms) {
            if (!ms) return '-';
            const d = new Date(ms);
            return `${detailFmtDate(d)} ${detailPad(d.getHours())}:${detailPad(d.getMinutes())}:${detailPad(d.getSeconds())}`;
        }

        function detailEsc(s) {
            return String(s ?? '').replace(/[&<>"']/g, c => ({
                '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
            }[c]));
        }

        async function loadDataDetail() {
            const loading = document.getElementById('detailLoading');
            const errorBox = document.getElementById('detailError');
            const wrap = document.getElementById('detailTableWrap');
            const meta = document.getElementById('detailMeta');
            if (!loading || !errorBox || !wrap) return;

            const startDate = document.getElementById('detailStartDate').value;
            const endDate = document.getElementById('detailEndDate').value;
            if (!startDate || !endDate) {
                alert('请选择日期区间');
                return;
            }

            errorBox.style.display = 'none';
            wrap.style.display = 'none';
            if (meta) meta.style.display = 'none';
            loading.style.display = 'block';

            try {
                const response = await fetch(`?handler=DataDetail&productKey=${encodeURIComponent(window.__deviceDetail.productKey)}&deviceKey=${encodeURIComponent(window.__deviceDetail.deviceKey)}&startDate=${encodeURIComponent(startDate)}&endDate=${encodeURIComponent(endDate)}`);
                const result = await response.json();

                if (!result.success) {
                    errorBox.textContent = result.message || '加载数据明细失败';
                    errorBox.style.display = 'block';
                    return;
                }

                const columns = result.columns || [];
                const rows = result.rows || [];

                // 表头
                const head = document.getElementById('detailHead');
                let headHtml = '<tr><th style="min-width: 56px;">序号</th><th style="min-width: 160px;">数据更新时间</th>';
                columns.forEach(col => {
                    const label = col.unit ? `${col.label} (${col.unit})` : col.label;
                    headHtml += `<th style="min-width: 96px;">${detailEsc(label)}</th>`;
                });
                headHtml += '</tr>';
                head.innerHTML = headHtml;

                // 行（后端已按时间倒序：最新在上）
                const body = document.getElementById('detailBody');
                if (rows.length === 0) {
                    body.innerHTML = `<tr><td colspan="${columns.length + 2}" class="text-center text-muted py-4">该时间区间内暂无上报数据</td></tr>`;
                } else {
                    let bodyHtml = '';
                    rows.forEach((row, idx) => {
                        bodyHtml += `<tr><td>${idx + 1}</td><td>${detailFmtTime(row.time)}</td>`;
                        (row.cells || []).forEach(v => {
                            bodyHtml += `<td>${v === '' || v === null || v === undefined ? '-' : detailEsc(v)}</td>`;
                        });
                        bodyHtml += '</tr>';
                    });
                    body.innerHTML = bodyHtml;
                }

                if (meta) {
                    meta.textContent = `共 ${rows.length} 条上报记录（${startDate} ~ ${endDate}，按时间倒序）`;
                    meta.style.display = 'block';
                }
                wrap.style.display = 'block';
            } catch (error) {
                errorBox.textContent = '请求失败: ' + error.message;
                errorBox.style.display = 'block';
            } finally {
                loading.style.display = 'none';
            }
        }

        // 默认最近 3 天，页面加载后自动查询
        document.addEventListener('DOMContentLoaded', function() {
            if (!document.getElementById('detailBody')) return;
            const end = new Date();
            const start = new Date();
            start.setDate(end.getDate() - 2);
            document.getElementById('detailStartDate').value = detailFmtDate(start);
            document.getElementById('detailEndDate').value = detailFmtDate(end);
            loadDataDetail();
        });

        // 控制设备
        function debugDevice() {
            // 清空上次的结果
            document.getElementById('cmdResult').style.display = 'none';
            document.getElementById('cmdSuccess').style.display = 'none';
            document.getElementById('cmdError').style.display = 'none';
    
            // 清空指令输入框
            document.getElementById('cmdData').value = '';
    
            // 打开模态框
            new bootstrap.Modal(document.getElementById('commandModal')).show();
    
            // 聚焦到输入框
            setTimeout(() => {
                document.getElementById('cmdData').focus();
            }, 500);
        }

        // 快捷指令
        function setQuickCommand(cmd) {
            document.getElementById('cmdData').value = cmd;
            document.getElementById('cmdData').focus();
        }

        // 提交指令
        async function submitCommand() {
            const data = document.getElementById('cmdData').value.trim();
            if (!data) {
                alert('请输入要发送的指令');
                document.getElementById('cmdData').focus();
                return;
            }

            // 处理转义字符：将 \r 转为实际字符
            const processedData = data.replace(/\\r/g, '\r').replace(/\\n/g, '\n');

            const requestData = {
                productKey: document.getElementById('cmdProductKey').value,
                deviceKey: document.getElementById('cmdDeviceKey').value,
                data: processedData,
                encode: document.getElementById('cmdEncode').value,
                isCache: document.getElementById('cmdIsCache').checked,
                isCover: document.getElementById('cmdIsCover').checked,
                qos: document.getElementById('cmdQos').value ? parseInt(document.getElementById('cmdQos').value) : null,
                cacheTime: document.getElementById('cmdCacheTime').value ? parseInt(document.getElementById('cmdCacheTime').value) : null
            };

            const resultDiv = document.getElementById('cmdResult');
            const successDiv = document.getElementById('cmdSuccess');
            const errorDiv = document.getElementById('cmdError');

            resultDiv.style.display = 'block';
            successDiv.style.display = 'none';
            errorDiv.style.display = 'none';

            // 禁用发送按钮，防止重复提交
            const sendBtn = document.querySelector('#commandModal .btn-primary');
            sendBtn.disabled = true;
            sendBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status"></span> 发送中...';

            try {
                const response = await fetch('?handler=SendCommand', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                    },
                    body: JSON.stringify(requestData)
                });

                const result = await response.json();

                if (result.success) {
                    document.getElementById('cmdSuccessMsg').textContent = result.message || '指令发送成功';
                    successDiv.style.display = 'block';
                } else {
                    document.getElementById('cmdErrorMsg').textContent = result.message || '指令发送失败';
                    errorDiv.style.display = 'block';
                }
            } catch (error) {
                document.getElementById('cmdErrorMsg').textContent = '请求失败: ' + error.message;
                errorDiv.style.display = 'block';
            } finally {
                // 恢复发送按钮
                sendBtn.disabled = false;
                sendBtn.innerHTML = '<i class="bi bi-send"></i> 发送指令';
            }
        }

        // 按 Ctrl+Enter 发送指令
        document.addEventListener('DOMContentLoaded', function() {
            const cmdData = document.getElementById('cmdData');
            if (cmdData) {
                cmdData.addEventListener('keydown', function(e) {
                    if (e.ctrlKey && e.key === 'Enter') {
                        e.preventDefault();
                        submitCommand();
                    }
                });
            }
        });

        // 编辑设备
        function editDevice() {
            document.getElementById('editProductKey').value = window.__deviceDetail.productKey;
            document.getElementById('editDeviceKey').value = window.__deviceDetail.deviceKey;
            document.getElementById('editDeviceName').value = window.__deviceDetail.deviceName;
            document.getElementById('editSn').value = '@(Model.Device.Sn ?? "")';
            new bootstrap.Modal(document.getElementById('editModal')).show();
        }

        // 提交编辑
        async function submitEdit() {
            const productKey = document.getElementById('editProductKey').value;
            const deviceKey = document.getElementById('editDeviceKey').value;
            const deviceName = document.getElementById('editDeviceName').value;
            const sn = document.getElementById('editSn').value;

            const data = {
                productKey: productKey,
                deviceKey: deviceKey,
                deviceName: deviceName || '',
                sn: sn || ''
            };

            try {
                const response = await fetch('/Devices/Index?handler=Update', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                    },
                    body: JSON.stringify(data)
                });

                const result = await response.json();

                if (result.success) {
                    alert('修改成功');
                    document.getElementById('editModal').querySelector('.btn-close').click();
                    location.reload();
                } else {
                    alert(result.message || '修改失败');
                }
            } catch (error) {
                alert('请求失败: ' + error.message);
            }
        }
