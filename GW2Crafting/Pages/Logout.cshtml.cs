using GW2Crafting.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GW2Crafting.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            var id = SessionId.GetSessionId(HttpContext);
            if (id != Guid.Empty)
            {
                SessionId.ResetSession(HttpContext);
            }
            return RedirectToPage("Index");
        }
    }
}
