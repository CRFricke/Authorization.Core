using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

#pragma warning disable CA1515 // Consider making public types internal

namespace Authorization.Core.UI.Test.Web.Areas.Admin.Pages;

[Authorize()]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
