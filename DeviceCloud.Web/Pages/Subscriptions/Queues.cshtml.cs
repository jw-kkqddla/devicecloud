using Microsoft.AspNetCore.Mvc.RazorPages;
using DeviceCloud.Application.DTOs.Subscriptions;
using DeviceCloud.Application.Interfaces.Subscriptions;

namespace DeviceCloud.Web.Pages.Subscriptions;

public class QueuesModel : PageModel
{
    private readonly ISubscriptionAppService _subscriptionService;

    public QueuesModel(ISubscriptionAppService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    public List<QueueListDto> Queues { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var result = await _subscriptionService.GetQueuesAsync();

        if (result.Success)
        {
            Queues = result.Data ?? new List<QueueListDto>();
        }
        else
        {
            ErrorMessage = result.Message;
        }
    }
}
