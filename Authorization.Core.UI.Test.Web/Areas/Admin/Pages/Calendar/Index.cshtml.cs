using CRFricke.Authorization.Core.Attributes;
using Microsoft.AspNetCore.Mvc.RazorPages;

#pragma warning disable CA1515 // Consider making public types internal
#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Authorization.Core.UI.Test.Web.Admin.Pages.Calendar;

[RequiresClaims(AppClaims.Calendar.List)]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
