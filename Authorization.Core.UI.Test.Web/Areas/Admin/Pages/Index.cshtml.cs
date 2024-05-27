using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Authorization.Core.UI.Test.Web.Areas.Admin.Pages
{
    [Authorize()]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
