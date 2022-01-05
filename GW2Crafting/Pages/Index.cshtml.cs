using GW2Crafting.Caching;
using GW2Crafting.Common;
using GW2Crafting.Models;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace GW2Crafting.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly Gw2TokenCache _tokenCache;

    [BindProperty]
    public MainListSelection Selection { get; set; } = new MainListSelection { ListType = MainListType.Recipes };
    [BindProperty]
    public string AccessToken { get; set; } = "";

    public IndexModel(ILogger<IndexModel> logger, Gw2TokenCache tokenCache)
    {
        _logger = logger;
        _tokenCache = tokenCache;
    }

    public async Task<IActionResult> OnGetAsync()
    {

        var id = SessionId.GetSessionId(HttpContext);
        if (id != Guid.Empty)
        {
            AccessToken = await _tokenCache.Get<string>(id, CacheTypeId.AccessToken) ?? string.Empty;
            Selection = await _tokenCache.Get<MainListSelection>(id, CacheTypeId.ListType) ?? new MainListSelection { ListType = MainListType.Inventory };
        }
        return Page();
    }

    public IActionResult OnPost()
    {
        var id = SessionId.GetSessionId(HttpContext);
        if (id == Guid.Empty)
        {
            id = Guid.NewGuid();
            SessionId.SetSessionId(HttpContext, id);
            _tokenCache.Add(id, CacheTypeId.AccessToken, AccessToken);
        }
        return Page();
        /*else
        {
            _tokenCache.Add(id, CacheTypeId.ListType, Selection);
            return Selection.ListType switch
            {
                MainListType.Inventory => RedirectToPage("CharacterSelection"),
                MainListType.Recipes => RedirectToPage("CharacterSelection"),
                MainListType.Bank => RedirectToPage("Bank"),
                MainListType.Material => RedirectToPage("Material"),
                MainListType.Wallet => RedirectToPage("Wallet"),
                _ => Page(),
            };
        }*/
    }

    public static bool IsLoggedIn(HttpContext context)
    {
        var id = SessionId.GetSessionId(context);
        return id != Guid.Empty;
    }
    public static string GetAccountName(HttpContext context, Gw2TokenCache tokenCache)
    {
        var id = SessionId.GetSessionId(context);
        if (id != Guid.Empty)
        {
            var account = tokenCache.Get<Account>(id, CacheTypeId.Account).Result;
            if (string.IsNullOrWhiteSpace(account?.Name))
            {
                return String.Empty;
            }
            var selectedCharacter = tokenCache.Get<string>(id, CacheTypeId.SelectedCharacter).Result;
            if (string.IsNullOrWhiteSpace(selectedCharacter))
            {
                return account.Name;
            }
            return $"{account.Name} ({selectedCharacter})";
        }
        return String.Empty;
    }
    public static string SelectedCharacter(HttpContext context, Gw2TokenCache tokenCache)
    {
        var id = SessionId.GetSessionId(context);
        if (id != Guid.Empty)
        {
            var selectedCharacter = tokenCache.Get<string>(id, CacheTypeId.SelectedCharacter).Result;
            return string.IsNullOrWhiteSpace(selectedCharacter) ? String.Empty : selectedCharacter;
        }
        return String.Empty;
    }
}
