using GW2Crafting.Caching;
using GW2Crafting.Common;
using Gw2Sharp;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GW2Crafting.Pages
{
    public class RecipeSearchModel : PageModel
    {
        private readonly ILogger<RecipeSearchModel> _logger;
        private readonly Gw2TokenCache _tokenCache;
        private readonly Gw2Database _database;

        [BindProperty(SupportsGet = true)]
        public int ItemId { get; set; }
        [BindProperty]
        public string ItemName { get; set; } = string.Empty;
        [BindProperty]
        public ICollection<Gw2ResolvedRecipe> Recipes { get; set; } = new List<Gw2ResolvedRecipe>();

        public RecipeSearchModel(ILogger<RecipeSearchModel> logger, Gw2TokenCache tokenCache, Gw2Database db)
        {
            _logger = logger;
            _tokenCache = tokenCache;
            _database = db;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var id = SessionId.GetSessionId(HttpContext);
            if (id == Guid.Empty)
            {
                _logger.LogInformation("Redirecting to home page due to lack of session Id");
                return RedirectToPage("Index");
            }
            var client = await _tokenCache.Get<Gw2Client>(id, CacheTypeId.Client);
            if (client == null)
            {
                _logger.LogWarning("Unable to get client. Resetting session.");
                SessionId.ResetSession(HttpContext);
                return RedirectToPage("Index");
            }
            var selectedCharacter = await _tokenCache.Get<string>(id, CacheTypeId.SelectedCharacter);
            var characters = await _tokenCache.Get<IApiV2ObjectList<Character>>(id, CacheTypeId.Characters);
            if (string.IsNullOrWhiteSpace(selectedCharacter) || characters == null)
            {
                _logger.LogWarning("Unable to get selected character, resetting session");
                SessionId.ResetSession(HttpContext);
                return RedirectToPage("Index");
            }
            var character = characters.FirstOrDefault(w => w.Name == selectedCharacter);
            if (character == null)
            {
                _logger.LogWarning($"Character {selectedCharacter} not found, resetting session");
                SessionId.ResetSession(HttpContext);
                return RedirectToPage("Index");
            }
            var inputClient = client.WebApi.V2.Recipes.Search.Input(ItemId);
            var data = await inputClient.GetAsync();
            if (data == null)
            {
                _logger.LogInformation($"Unable to find recipies for {ItemId}");
                return RedirectToPage("Material");
            }
            Recipes.Clear();
            foreach (var item in data)
            {
                var recipe = _database.GetRecipe(item);
                if (recipe == null)
                {
                    continue;
                }
                if (recipe.CraftingDisciplines.Any(w => character.Crafting.Any(q => q.Discipline == w)))
                {
                    Recipes.Add(new Gw2ResolvedRecipe(recipe, _database, _tokenCache));
                }
            }
            return Page();
        }
    }
}
