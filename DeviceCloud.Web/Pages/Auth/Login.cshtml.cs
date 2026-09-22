using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DeviceCloud.Application.Common;
using DeviceCloud.Application.DTOs;
using DeviceCloud.Application.Interfaces;

namespace DeviceCloud.Web.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly IQuectelAppService _appService;

    public LoginModel(IQuectelAppService appService)
    {
        _appService = appService;
    }

    public ApiResponse<TokenData>? TokenInfo { get; set; }

    public async Task OnGetAsync()
    {
        TokenInfo = await _appService.GetCurrentTokenAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        TokenInfo = await _appService.LoginAsync();
        return Page();
    }
}
