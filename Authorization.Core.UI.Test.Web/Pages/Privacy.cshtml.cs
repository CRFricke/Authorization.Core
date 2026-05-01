using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Authorization.Core.UI.Test.Web.Pages;

#pragma warning disable CA1515 // Consider making public types internal

public class PrivacyModel : PageModel
{
    private readonly ILogger<PrivacyModel> _logger;

    public PrivacyModel(ILogger<PrivacyModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }
}