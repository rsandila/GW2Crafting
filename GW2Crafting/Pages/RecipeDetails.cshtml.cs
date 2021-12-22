using GW2Crafting.Caching;
using GW2Crafting.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GW2Crafting.Pages
{
    public class RecipeDetailsModel : PageModel
    {
        private readonly ILogger<RecipeDetailsModel> _logger;
        private readonly Gw2TokenCache _tokenCache;
        private readonly Gw2Database _database;
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }
        [BindProperty]
        public Gw2ResolvedRecipe? Recipe { get; private set; } 
        public RecipeDetailsModel(ILogger<RecipeDetailsModel> logger, Gw2TokenCache tokenCache, Gw2Database db)
        {
            _logger = logger;
            _tokenCache = tokenCache;
            _database = db;
        }
        public IActionResult OnGet()
        {
            var id = SessionId.GetSessionId(HttpContext);
            if (id == Guid.Empty)
            {
                return RedirectToPage("Index");
            }
            if (Id == 0)
            {
                SessionId.ResetSession(HttpContext);
                return RedirectToPage("Index");
            }
            var recipe = _database.GetRecipe(Id);
            if (recipe == null)
            {
                SessionId.ResetSession(HttpContext);
                return RedirectToPage("Index");
            }
            Recipe = new Gw2ResolvedRecipe(recipe, _database, _tokenCache);
            return Page();
        }
    }
}
