using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DeviceCloud.Application.DTOs.Devices;
using DeviceCloud.Application.Interfaces.Devices;

namespace DeviceCloud.Web.Pages.Devices;

public class IndexModel : PageModel
{
    private readonly IDeviceAppService _deviceService;
    private readonly IConfiguration _configuration;

    public IndexModel(IDeviceAppService deviceService, IConfiguration configuration)
    {
        _deviceService = deviceService;
        _configuration = configuration;
    }

    [BindProperty(SupportsGet = true)]
    public string ProductKey { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? DeviceKey { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? DeviceName { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNum { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 20;

    public List<DeviceListDto> Devices { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        // 页面载入时自动使用默认产品Key调用API，无需人工输入
        if (string.IsNullOrWhiteSpace(ProductKey))
        {
            ProductKey = _configuration["Quectel:DefaultProductKey"] ?? "p11zgw";
        }

        var result = await _deviceService.GetDevicesAsync(ProductKey, DeviceKey, DeviceName, PageNum, PageSize);

        if (result.Success)
        {
            Devices = result.Data ?? new List<DeviceListDto>();
        }
        else
        {
            ErrorMessage = result.Message;
        }
    }

    public async Task<JsonResult> OnPostCreateAsync([FromBody] DeviceCreateRequest request)
    {
        var result = await _deviceService.CreateDeviceAsync(
            request.ProductKey,
            request.DeviceKey,
            request.DeviceName,
            request.Sn,
            request.AuthMode,
            request.Psk,
            request.FingerPrint);

        return new JsonResult(new { success = result.Success, data = result.Data, message = result.Message });
    }

    public async Task<JsonResult> OnPostDeleteAsync([FromBody] DeviceDeleteRequest request)
    {
        var result = await _deviceService.DeleteDeviceAsync(request.ProductKey, request.DeviceKey);
        return new JsonResult(new { success = result.Success, message = result.Message });
    }

    public async Task<JsonResult> OnPostUpdateAsync([FromBody] DeviceUpdateRequest request)
    {
        var result = await _deviceService.UpdateDeviceAsync(
            request.ProductKey,
            request.DeviceKey,
            request.DeviceName,
            request.Sn);

        return new JsonResult(new { success = result.Success, message = result.Message });
    }
}
