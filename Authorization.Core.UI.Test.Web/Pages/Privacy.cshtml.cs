using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authorization.Core.UI.Test.Web.Pages
{
    public class PrivacyModel : PageModel
    {
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ILogger<PrivacyModel> _logger;
#pragma warning restore IDE0052 // Remove unread private members

        public PrivacyModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}
