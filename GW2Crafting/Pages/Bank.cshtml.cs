using GW2Crafting.Caching;
using GW2Crafting.Common;
using Gw2Sharp;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GW2Crafting.Pages
{
    public class BankItem : MaterialItem
    {
        internal BankItem(MaterialItem item, long unitBuyPrice, int charges, string? binding, string? boundTo) : base(item)
        {
            Charges = charges;
            Binding = binding;
            BoundTo = boundTo;
            BuyUnitPrice = unitBuyPrice;
        }

        public int Charges { get; set; }
        public string? Binding { get; set; }
        public string? BoundTo { get; set; }
        public long BuyUnitPrice { get; set; }
    }
    public class BankModel : PageModel
    {
        private readonly ILogger<BankModel> _logger;
        private readonly Gw2TokenCache _tokenCache;
        private readonly Gw2Database _database;
        [BindProperty]
        public List<BankItem> Items { get; set; }
        public BankModel(ILogger<BankModel> logger, Gw2TokenCache tokenCache, Gw2Database db)
        {
            _logger = logger;
            _tokenCache = tokenCache;
            _database = db;
            Items = new List<BankItem>();
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
            await Listings.CacheListingsFor(_tokenCache, id, bankBlob.Select(w => w.Id));
            // TODO - get buy price
            foreach (var item in bankBlob)
            {
                var resolvedItem = _database.GetMaterialItem(item.Id, item.Count, item.Count, string.Empty);
                if (resolvedItem != null)
                {
                    Items.Add(new BankItem(resolvedItem, 0 /* TODO unit Buy price */, item.Charges.GetValueOrDefault(0), item.Binding ?? string.Empty, item.BoundTo));
                }
            }
            return Page();
        }
    }
}
