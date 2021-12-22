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
    public class InventoryModel : PageModel
    {
        [BindProperty]
        public List<Gw2Item> Inventory { get; set; }
        [BindProperty]
        public string? CharacterName { get; set; }

        private readonly ILogger<InventoryModel> _logger;
        private readonly Gw2TokenCache _tokenCache;
        private readonly Gw2Database _database;
        public InventoryModel(ILogger<InventoryModel> logger, Gw2TokenCache tokenCache, Gw2Database db)
        {
            _logger = logger;
            _tokenCache = tokenCache;
            _database = db;
            Inventory = new List<Gw2Item>();
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var id = SessionId.GetSessionId(HttpContext);
            if (id != Guid.Empty)
            {
                var selectedCharacter = await _tokenCache.Get<string>(id, CacheTypeId.SelectedCharacter);
                var characters = await _tokenCache.Get<IApiV2ObjectList<Character>>(id, CacheTypeId.Characters);
                if (string.IsNullOrWhiteSpace(selectedCharacter) || characters == null)
                {
                    SessionId.ResetSession(HttpContext);
                    return RedirectToPage("Index");
                }
                var character = characters.FirstOrDefault(w => w.Name == selectedCharacter);
                if (character == null)
                {
                    SessionId.ResetSession(HttpContext);
                    return RedirectToPage("Index");
                }
                CharacterName = selectedCharacter;
                Inventory.Clear();
                if (character.Bags != null)
                {
                    foreach (var bag in character.Bags.Where(w => w != null))
                    {
                        foreach (var item in bag.Inventory)
                        {
                            if (item != null)
                            {
                                var itemDetails = _database.GetItem(item.Id);
                                if (itemDetails != null)
                                {
                                    Inventory.Add(itemDetails);
                                }
                            }
                        }
                    }
                }
                return Page();
            }
            else
            {
                SessionId.ResetSession(HttpContext);
                return RedirectToPage("Index");
            }
        }
        public IActionResult OnPost()
        {
            return Page();
        }
    }
}
