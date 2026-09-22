using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DeviceCloud.Application.DTOs.Devices;
using DeviceCloud.Application.Interfaces.Devices;
using DeviceCloud.Application.Interfaces.Products;

namespace DeviceCloud.Web.Pages.Devices
{
    public partial class DetailModel : PageModel
    {
        private readonly IDeviceAppService _deviceService;
        private readonly IProductAppService _productService;
        private readonly IDeviceDetailQueryService _queryService;
        private readonly IDeviceCommandService _commandService;
        private readonly ILogger<DetailModel> _logger;

        public DetailModel(
            IDeviceAppService deviceService,
            IProductAppService productService,
            IDeviceDetailQueryService queryService,
            IDeviceCommandService commandService,
            ILogger<DetailModel> logger)
        {
            _deviceService = deviceService;
            _productService = productService;
            _queryService = queryService;
            _commandService = commandService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string ProductKey { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string DeviceKey { get; set; } = string.Empty;

        public DeviceDetailViewModel Device { get; set; } = new();

        public List<TslPropertyViewModel> TslProperties { get; set; } = new();

        public List<PropertyFieldGroup> PropertyFieldGroups { get; set; } = new();

        public string? TslError { get; set; }

        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 识别出的机型分组（"A"=A/C 组、"B"=B/D 组），用于控制面板自动填充机型。
        /// </summary>
        public string? MachineTypeGroup { get; set; }

        /// <summary>
        /// 识别出的具体机型（如 "HPVINV04"），用于控制面板自动选中。
        /// </summary>
        public string? MachineType { get; set; }

        /// <summary>
        /// 图表属性分组（按模块），供前端下拉框按 optgroup 展示。
        /// </summary>
        public IReadOnlyList<ChartPropertyGroup> ChartPropertyGroups => _queryService.ChartPropertyGroups;

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrWhiteSpace(ProductKey) || string.IsNullOrWhiteSpace(DeviceKey))
            {
                return RedirectToPage("/Devices/Index");
            }

            var result = await _deviceService.GetDeviceDetailAsync(ProductKey, DeviceKey);

            if (!result.Success || result.Data == null)
            {
                ErrorMessage = result.Message;
                return Page();
            }

            Device = DeviceDetailViewModel.FromDto(result.Data);

            try
            {
                var tslResult = await _productService.GetProductTslAsync(ProductKey);
                if (tslResult.Success && tslResult.Data != null)
                {
                    TslProperties = _queryService.ParseTslProperties(tslResult.Data.TslData);
                }
                else
                {
                    TslError = tslResult.Message;
                }

                await FillLatestPropertyValuesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "物模型解析或历史数据获取失败");
                TslError = $"数据加载失败: {ex.Message}";
            }

            // 兜底：即使物模型/历史数据都为空，也按固定字段清单空占位显示全部字段
            if (PropertyFieldGroups == null || PropertyFieldGroups.Count == 0)
            {
                PropertyFieldGroups = _queryService.BuildFieldGroups(TslProperties, new Dictionary<int, string>(), MachineType);
            }

            return Page();
        }

        /// <summary>
        /// 填充属性的最新值（readData 优先 + 历史回退 + 缓存）
        /// </summary>
        private async Task FillLatestPropertyValuesAsync()
        {
            var snapshot = await _queryService.LoadSnapshotAsync(ProductKey, DeviceKey, TslProperties);

            if (snapshot.UpdateTime.HasValue)
            {
                Device.UpdateTime = snapshot.UpdateTime.Value;
            }

            MachineType = snapshot.MachineType;
            MachineTypeGroup = snapshot.MachineTypeGroup;
            PropertyFieldGroups = snapshot.FieldGroups;
        }

        /// <summary>
        /// 读取设备数据（调试用）
        /// </summary>
        public async Task<IActionResult> OnPostReadDataAsync([FromBody] ReadDeviceDataRequest request)
        {
            var result = await _commandService.ReadDataAsync(request);

            if (result.Success)
                return new JsonResult(new { success = true, data = result.Data, message = result.Message });

            if (result.ErrorCode != null)
                return new JsonResult(new { success = false, message = result.Message, code = result.ErrorCode });

            return new JsonResult(new { success = false, message = result.Message });
        }

        /// <summary>
        /// 下发设备指令（Text 格式）
        /// </summary>
        public async Task<IActionResult> OnPostSendCommandAsync([FromBody] SendCommandRequest request)
        {
            var result = await _commandService.SendCommandAsync(request);

            if (result.Success)
                return new JsonResult(new { success = true, message = result.Message, data = result.Data });

            if (result.Code.HasValue)
                return new JsonResult(new { success = false, message = result.Message, code = result.Code });

            return new JsonResult(new { success = false, message = result.Message });
        }

        /// <summary>
        /// 下发 HEEP1 设置指令（枚举/数值 → 指令 → 透传下发）
        /// </summary>
        public async Task<IActionResult> OnPostSendSettingAsync([FromBody] SendSettingRequest request)
        {
            var result = await _commandService.SendSettingAsync(request);

            if (result.Success)
                return new JsonResult(new { success = true, message = result.Message, hex = result.Hex });

            if (result.Code.HasValue)
                return new JsonResult(new { success = false, message = result.Message, code = result.Code });

            return new JsonResult(new { success = false, message = result.Message });
        }

        /// <summary>
        /// 获取图表曲线数据
        /// </summary>
        public async Task<IActionResult> OnGetHistoryDataAsync(string property, string range = "24h")
        {
            var series = await _queryService.GetHistoryAsync(ProductKey, DeviceKey, property, range);

            if (!series.Success)
                return new JsonResult(new { success = false, message = series.Message });

            return new JsonResult(new { success = true, points = series.Points, propertyName = series.PropertyName, unit = series.Unit });
        }

        /// <summary>
        /// 月发电量柱状图：按天取当天最后一条「日发电量」。
        /// </summary>
        public async Task<IActionResult> OnGetDailyEnergyAsync(string month)
        {
            var series = await _queryService.GetDailyEnergyAsync(ProductKey, DeviceKey, month, TslProperties);

            if (!series.Success)
                return new JsonResult(new { success = false, message = series.Message });

            return new JsonResult(new { success = true, labels = series.Labels, values = series.Values, unit = series.Unit });
        }
        /// <summary>
        /// 数据明细：时间区间内每一条上报记录（最新在上）。
        /// GET 处理器不经过 OnGetAsync，需显式接收 productKey/deviceKey 并独立加载物模型。
        /// </summary>
        public async Task<IActionResult> OnGetDataDetailAsync(string productKey, string deviceKey, string startDate, string endDate)
        {
            // 最近 3 天（含今天）
            var today = DateTime.Today;
            if (!DateTime.TryParse(startDate, out var s)) s = today.AddDays(-2);
            if (!DateTime.TryParse(endDate, out var e)) e = today;

            var start = new DateTimeOffset(s).ToUnixTimeMilliseconds();
            var now = DateTime.Now;

            var endTime = e.Date.AddDays(1).AddSeconds(-1);

            if (endTime > now)
            {
                endTime = now;
            }

            var end = new DateTimeOffset(endTime)
                .ToUnixTimeMilliseconds();

            // 独立加载物模型（GET 处理器不经过 OnGetAsync）
            var tslProperties = new List<TslPropertyViewModel>();
            try
            {
                var tslResult = await _productService.GetProductTslAsync(productKey);
                if (tslResult.Success && tslResult.Data != null)
                    tslProperties = _queryService.ParseTslProperties(tslResult.Data.TslData);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "数据明细加载物模型失败，按空清单处理");
            }

            var result = await _queryService.GetDataDetailAsync(productKey, deviceKey, start, end, tslProperties);
            return new JsonResult(result);
        }
    }
}
