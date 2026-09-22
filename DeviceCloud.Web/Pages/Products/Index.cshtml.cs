using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DeviceCloud.Application.DTOs.Products;
using DeviceCloud.Application.Interfaces.Products;

namespace DeviceCloud.Web.Pages.Products;

public class IndexModel : PageModel
{
    private readonly IProductAppService _productService;

    public IndexModel(IProductAppService productService)
    {
        _productService = productService;
    }

    [BindProperty(SupportsGet = true)]
    public string? ProductName { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ProductKey { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNum { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 20;

    public List<ProductListDto> Products { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var result = await _productService.GetProductsAsync(ProductName, ProductKey, null, PageNum, PageSize);

        if (result.Success)
        {
            Products = result.Data ?? new List<ProductListDto>();
        }
        else
        {
            ErrorMessage = result.Message;
        }
    }

    public async Task<JsonResult> OnGetDetailAsync(string productKey)
    {
        if (string.IsNullOrWhiteSpace(productKey))
        {
            return new JsonResult(new { success = false, message = "产品Key不能为空" });
        }

        var result = await _productService.GetProductDetailAsync(productKey);

        if (result.Success && result.Data != null)
        {
            var detail = result.Data;
            return new JsonResult(new
            {
                success = true,
                data = new
                {
                    productKey = detail.ProductKey,
                    productName = detail.ProductName,
                    accessType = detail.AccessType,
                    accessTypeDisplay = detail.AccessTypeDisplay,
                    netWay = detail.NetWay ?? "-",
                    dataFmt = detail.DataFmt,
                    dataFmtDisplay = detail.DataFmtDisplay,
                    logoPath = detail.LogoPath ?? "",
                    createTime = detail.CreateTimeLocal.ToString("yyyy-MM-dd HH:mm:ss"),
                    updateTime = detail.UpdateTimeLocal?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-",
                    connectPlatform = detail.ConnectPlatform
                }
            });
        }

        return new JsonResult(new { success = false, message = result.Message });
    }
}
