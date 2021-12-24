using CRFricke.Authorization.Core.Attributes;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Authorization.Core.UI.Test.Web.Admin.Pages.Calendar
{
    [RequiresClaims(AppClaims.Calendar.List)]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
