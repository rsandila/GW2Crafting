using GW2Crafting.Caching;
using GW2Crafting.Common;
using GW2Crafting.Models;
using Gw2Sharp.WebApi.V2;
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
        }

        _tokenCache.Add(id, CacheTypeId.AccessToken, AccessToken);
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
    }
}
