using GW2Crafting.Caching;
using GW2Crafting.Caching.Models;
using GW2Crafting.Common;
using Gw2Sharp;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GW2Crafting.Pages
{
    public class WalletItem: Gw2Currency
    {
        public WalletItem(Gw2Currency original, int value) : base(original)
        {
            Value = value;
        }
        public int Value { get; private set; }
    }
    public class WalletModel : PageModel
    {
        private readonly ILogger<WalletModel> _logger;
        private readonly Gw2TokenCache _tokenCache;
        private readonly Gw2Database _database;
        [BindProperty]
        public List<WalletItem> Items { get; private set; }
        public WalletModel(ILogger<WalletModel> logger, Gw2TokenCache tokenCache, Gw2Database db)
        {
            _logger = logger;
            _tokenCache = tokenCache;
            _database = db;
            Items = new List<WalletItem>();
        }
        public async Task<IActionResult> OnGet()
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
            var wallet = await client.WebApi.V2.Account.Wallet.GetAsync();
            if (wallet == null)
            {
                SessionId.ResetSession(HttpContext);
                return RedirectToPage("Index");
            }
            Items.Clear();
            foreach (var item in wallet)
            {
                var resolvedItem = _database.GetCurrencyItem(item.Id);
                if (resolvedItem != null)
                {
                    Items.Add(new WalletItem(resolvedItem, item.Value));
                }
            }
            return Page();
        }
    }
}
