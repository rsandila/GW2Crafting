using GW2Crafting.Caching;
using GW2Crafting.Common;
using Gw2Sharp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GW2Crafting.Pages
{
    public class BankModel : PageModel
    {
        private readonly ILogger<BankModel> _logger;
        private readonly Gw2TokenCache _tokenCache;
        private readonly Gw2Database _database;
        [BindProperty]
        public IEnumerable<MaterialItem> Items { get; set; }
        public BankModel(ILogger<BankModel> logger, Gw2TokenCache tokenCache, Gw2Database db)
        {
            _logger = logger;
            _tokenCache = tokenCache;
            _database = db;
            Items = new List<MaterialItem>();
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var id = SessionId.GetSessionId(HttpContext);
            if (id == Guid.Empty)
            {
                return RedirectToPage("Index");
            }
            var client = await _tokenCache.Get<Gw2Client>(id, CacheTypeId.Client);
            if (client == null)
            {
                SessionId.ResetSession(HttpContext);
                return RedirectToPage("Index");
            }
            var bankBlob = await client.WebApi.V2.Account.Bank.GetAsync();
            if (bankBlob == null)
            {
                SessionId.ResetSession(HttpContext);
                return RedirectToPage("Index");
            }
            // TODO
        }
    }
}
