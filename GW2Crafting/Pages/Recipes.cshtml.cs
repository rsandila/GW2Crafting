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
    public class Gw2ResolvedRecipe
    {
        public Gw2ResolvedRecipe(Gw2Recipe recipe, Gw2Database _database, Gw2TokenCache _tokenCache)
        {
            Original = recipe;
            OutputItem = _database.GetItem(recipe.OutputItemId);
            if (recipe.Ingredients == null)
            {
                throw new InvalidDataException("recipe.Ingredients can not be null");
            }
            Ingredients = recipe.Ingredients.Select(w => _database.GetMaterialItem(w.Id, w.Count, 0, String.Empty));
            var outputItemPrice = Listings.GetListingsFor(_tokenCache, new[] { Original.OutputItemId });
            var ingredientPrices = Listings.GetListingsFor(_tokenCache, recipe.Ingredients.Select(w => w.Id));
            ListingSellPrice = outputItemPrice.GetSellingUnitPrice(0);
            ListingBuyPrice = outputItemPrice.GetBuyingUnitPrice(0);
            ListingIngredientsPrice = 0;
            if (ingredientPrices.Count() != recipe.Ingredients.Count())
            {
                ListingIngredientsPrice = 1000000;
            }
            foreach (var item in ingredientPrices)
            {
                var ingredientCount = recipe.Ingredients?.FirstOrDefault(w => w.Id == item.Id)?.Count ?? 1000000;
                ListingIngredientsPrice += item.GetSellingUnitPrice(1000000, ingredientCount);
            }
        }
        public Gw2Recipe Original { get; set; }
        public Gw2Item? OutputItem { get; set; }
        public IEnumerable<MaterialItem?> Ingredients { get; set; }
        public int ListingSellPrice { get; set; }
        public int ListingBuyPrice { get; set; }
        public int ListingIngredientsPrice { get; set; }
    }
    public class RecipesModel : PageModel
    {
        private readonly ILogger<RecipesModel> _logger;
        private readonly Gw2TokenCache _tokenCache;
        private readonly Gw2Database _database;
        [BindProperty]
        public IEnumerable<Gw2ResolvedRecipe> Recipes { get; set; }
        [BindProperty]
        public string? CharacterName { get; set; }
        public RecipesModel(ILogger<RecipesModel> logger, Gw2TokenCache tokenCache, Gw2Database db)
        {
            _logger = logger;
            _tokenCache = tokenCache;
            _database = db;
            Recipes = new List<Gw2ResolvedRecipe>();
        }
        public async Task<IActionResult> OnGetAsync(string? sortOrder)
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
            var selectedCharacter = await _tokenCache.Get<string>(id, CacheTypeId.SelectedCharacter);
            CharacterName = selectedCharacter;
            if (selectedCharacter == null)
            {
                SessionId.ResetSession(HttpContext);
                return RedirectToPage("Index");
            }
            var localRecipes = await _tokenCache.GetResolvedRecipies(_database, id, selectedCharacter);
            if (string.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "sell";
            }
            Recipes = sortOrder.ToLowerInvariant() switch
            {
                "buy" => localRecipes.OrderByDescending(w => w.ListingBuyPrice - w.ListingIngredientsPrice),
                "sell" => localRecipes.OrderByDescending(w => w.ListingSellPrice - w.ListingIngredientsPrice),
                _ => localRecipes,
            };
            /*
var characters = await _tokenCache.Get<IApiV2ObjectList<Character>>(id, CacheTypeId.Characters);
if (string.IsNullOrWhiteSpace(selectedCharacter) || characters == null)
{
  SessionId.ResetSession(HttpContext);
  return RedirectToPage("Index");
}
var character = characters.FirstOrDefault(w => w.Name == selectedCharacter);
if (character == null || character.Recipes == null)
{
  SessionId.ResetSession(HttpContext);
  return RedirectToPage("Index");
}
CharacterName = character.Name;
var resolvedRecipies = character.Recipes.Select(w => _database.GetRecipe(w));
await Listings.CacheListingsFor(_tokenCache, id, resolvedRecipies.Select(w => w?.OutputItemId ?? 0));
await Listings.CacheListingsFor(_tokenCache, id, resolvedRecipies.SelectMany(w => w?.Ingredients?.Select(q => q.Id) ?? Array.Empty<int>()));
foreach (var item in resolvedRecipies)
{
  if (item != null)
  {
      Recipes.Add(new Gw2ResolvedRecipe(item, _database, _tokenCache));
  }
}
*/
            return Page();
        }
    }
}
