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
        public CharacterInformation()
        {
            Name = String.Empty;
            Profession = String.Empty;
        }
        public string Name { get; set; }

        public int Level { get; set; }
        public string Profession { get; set; }
    }
    public class CharacterSelectionModel : PageModel
    {
        [BindProperty]
        public IEnumerable<CharacterInformation> Characters { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SelectedCharacter { get; set; } = string.Empty;
        private readonly ILogger<CharacterSelectionModel> _logger;
        private readonly Gw2TokenCache _tokenCache;
        public CharacterSelectionModel(ILogger<CharacterSelectionModel> logger, Gw2TokenCache tokenCache)
        {
            _logger = logger;
            _tokenCache = tokenCache;
            Characters = Array.Empty<CharacterInformation>();
        }
        public async Task<IActionResult> OnGetAsync(string selectedCharacter, string returnTo)
        {
            Characters = await GetCharacterInformation(HttpContext, _tokenCache);
            if (!Characters.Any())
            {
                _logger.LogError("No characters found in account");
                SessionId.ResetSession(HttpContext);
                return RedirectToPage("Index");
            }
            if (!string.IsNullOrWhiteSpace(selectedCharacter) && !Characters.Any(w => w.Name != null && w.Name.Equals(selectedCharacter, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogError($"Character {selectedCharacter} found in account");
                return RedirectToPage("Index");
            }
            if (!string.IsNullOrWhiteSpace(selectedCharacter))
            {
                SelectedCharacter = selectedCharacter;
                _tokenCache.Add(SessionId.GetSessionId(HttpContext), CacheTypeId.SelectedCharacter, SelectedCharacter);
            }
            else
            {
                SelectedCharacter = (await _tokenCache.Get<string>(SessionId.GetSessionId(HttpContext), CacheTypeId.SelectedCharacter)) ?? string.Empty;
            }
            if (string.IsNullOrWhiteSpace(returnTo) || new Uri(returnTo, UriKind.RelativeOrAbsolute).IsAbsoluteUri)
            {
                return Page();
            }
            return Redirect(returnTo);
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

        public static async Task<IEnumerable<CharacterInformation>> GetCharacterInformation(HttpContext context, Gw2TokenCache _tokenCache)
        {
            var id = SessionId.GetSessionId(context);
            if (id != Guid.Empty)
            {
                var characters = await _tokenCache.Get<IApiV2ObjectList<Character>>(id, CacheTypeId.Characters);
                if (characters == null)
                {
                    return Array.Empty<CharacterInformation>();
                }
                return characters.Select(w => new CharacterInformation { Name = w.Name, Level = w.Level, Profession = w.Profession }).ToList();
            }
            return Array.Empty<CharacterInformation>();
        }

        public static async Task UpdateSelectedCharacter(HttpContext context, Gw2TokenCache _tokenCache, string selectedCharacter)
        {
            var id = SessionId.GetSessionId(context);
            if (id != Guid.Empty)
            {
                var listType = await _tokenCache.Get<MainListSelection>(id, CacheTypeId.ListType);
                if (listType == null)
                {
                    return;
                }
                _tokenCache.Add(id, CacheTypeId.SelectedCharacter, selectedCharacter);
            }
        }
    }
}
