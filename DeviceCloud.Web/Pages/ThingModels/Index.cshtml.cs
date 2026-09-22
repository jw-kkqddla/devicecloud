using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DeviceCloud.Application.Common;
using DeviceCloud.Application.DTOs.Products;
using DeviceCloud.Application.Interfaces.Products;

namespace DeviceCloud.Web.Pages.ThingModels;

public class IndexModel : PageModel
{
    private readonly IProductAppService _productService;

    public IndexModel(IProductAppService productService)
    {
        _productService = productService;
    }

    [BindProperty(SupportsGet = true)]
    public string? ProductKey { get; set; }

    public ApiResponse<ProductTslDto>? TslResult { get; set; }

    public void OnGet()
    {
        // 页面加载时不执行查询
    }

    public async Task<IActionResult> OnPostGetTslAsync(string productKey)
    {
        ProductKey = productKey;

        if (!string.IsNullOrWhiteSpace(productKey))
        {
            TslResult = await _productService.GetProductTslAsync(productKey);
        }

        return Page();
    }
}
