using GW2Crafting.Caching;
using GW2Crafting.Caching.Models;
using GW2Crafting.Common;
using Gw2Sharp;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GW2Crafting.Pages
{
    public class MaterialItem: Gw2Item
    {
        public string? CategoryName { get; set; }
        public long Count { get; set; }
        public long UnitSellPrice { get; set; }
    }
    public class BankModel : PageModel
    {
        private readonly ILogger<BankModel> _logger;
        private readonly Gw2TokenCache _tokenCache;
        private readonly Gw2Database _database;
        [BindProperty]
        public List<MaterialItem> Items { get; }

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
            var materials = await _tokenCache.Get<IApiV2ObjectList<Gw2Sharp.WebApi.V2.Models.MaterialCategory>>(id, CacheTypeId.MaterialCategories);
            if (materials == null)
            {
                SessionId.ResetSession(HttpContext);
                return RedirectToPage("Index");
            }
            var categoryDict = materials.ToDictionary(x => x.Id, x => x);
            var material = await _tokenCache.Get<IApiV2ObjectList<Gw2Sharp.WebApi.V2.Models.AccountMaterial>>(id, CacheTypeId.Materials);
            if (material == null)
            {
                SessionId.ResetSession(HttpContext);
                return RedirectToPage("Index");
            }
            var listings = Listings.GetListingsFor(_tokenCache, id, material.Select(x => x.Id));
            if (!listings.Any())
            {
                SessionId.ResetSession(HttpContext);
                return RedirectToPage("Index");
            }
            var listingDict = listings.ToDictionary(x => x.Id, x => x.Buys.OrderByDescending(w => w.UnitPrice).FirstOrDefault());
            foreach (var item in material)
            {
                var resolvedItem = _database.GetItem(item.Id);
                if (resolvedItem == null)
                {
                    continue;
                }
                string categoryName = string.Empty;
                if (categoryDict.TryGetValue(item.Category, out var category))
                {
                    categoryName = category.Name;
                }
                var unitSellPrice = 0;
                if (listingDict.TryGetValue(item.Id, out var listing) && listing != null)
                {
                    unitSellPrice = listing.UnitPrice;
                }
                Items.Add(new MaterialItem
                {
                    Id = item.Id,
                    CategoryName = categoryName,
                    Count = item.Count,
                    Description = resolvedItem.Description,
                    Flags = resolvedItem.Flags,
                    Icon = resolvedItem.Icon,
                    Level = resolvedItem.Level,
                    Name = resolvedItem.Name,
                    Rarity = resolvedItem.Rarity,
                    Type = resolvedItem.Type,
                    VendorValue = resolvedItem.VendorValue,
                    UnitSellPrice = unitSellPrice
                });
            }
            return Page();
        }
    }
}
