using GW2Crafting.Caching;
using GW2Crafting.Common;
using GW2Crafting.Models;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GW2Crafting.Pages
{
    public class CharacterInformation
    {
        public string? Name { get; set; }
        public int Level { get; set; }
        public string? Profession { get; set; }
    }
    public class CharacterSelectionModel : PageModel
    {
        [BindProperty]
        public IEnumerable<CharacterInformation>? Characters { get; set; }
        [BindProperty]
        public string SelectedCharacter { get; set; } = string.Empty;
        private readonly ILogger<CharacterSelectionModel> _logger;
        private readonly Gw2TokenCache _tokenCache;
        public CharacterSelectionModel(ILogger<CharacterSelectionModel> logger, Gw2TokenCache tokenCache)
        {
            _logger = logger;
            _tokenCache = tokenCache;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var id = SessionId.GetSessionId(HttpContext);
            if (id != Guid.Empty)
            {
                var characters = await _tokenCache.Get<IApiV2ObjectList<Character>>(id, CacheTypeId.Characters);
                if (characters == null)
                {
                    SessionId.ResetSession(HttpContext);
                    return RedirectToPage("Index");
                }
                Characters = characters.Select(w => new CharacterInformation { Name = w.Name, Level = w.Level, Profession = w.Profession }).ToList();
                return Page();
            }
            else
            {
                SessionId.ResetSession(HttpContext);
                return RedirectToPage("Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var id = SessionId.GetSessionId(HttpContext);
            if (id != Guid.Empty)
            {
                var listType = await _tokenCache.Get<MainListSelection>(id, CacheTypeId.ListType);
                if (listType == null)
                {
                    SessionId.ResetSession(HttpContext);
                    return RedirectToPage("Index");
                }
                _tokenCache.Add(id, CacheTypeId.SelectedCharacter, SelectedCharacter);
                switch (listType.ListType)
                {
                    case MainListType.Inventory:
                        return RedirectToPage("Inventory"); // Other choices does not need a character selected at the moment
                    case MainListType.Recipes:
                        return RedirectToPage("Recipes");
                }
            }
            SessionId.ResetSession(HttpContext);
            return RedirectToPage("Index");
        }
    }
}
