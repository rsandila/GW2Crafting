using GW2Crafting.Caching.Models;
using GW2Crafting.Pages;
using Gw2Sharp;
using Gw2Sharp.WebApi.V2.Models;
using LiteDB;
using System.Linq;

namespace GW2Crafting.Caching
{
    public class Gw2Database : IDisposable
    {
        internal static async Task IntializeAsync()
        {
            using var db = new LiteDatabase("Gw2Crafting.db");
            var colItems = db.GetCollection<Gw2Item>("Items");

            var connection = new Connection();
            using var client = new Gw2Client(connection);
            var webApiClient = client.WebApi.V2;

            var idList = await webApiClient.Items.IdsAsync();
            if (idList == null)
            {
                return;
            }
            var existingIds = colItems.Query().Select(w => w.Id).ToList();
            var newIds = idList.ToList().Except(existingIds).ToList();
            if (newIds.Any())
            {
                var newItems = await webApiClient.Items.ManyAsync(newIds);
                if (newItems.Any())
                {
                    colItems.InsertBulk(newItems.Select(w => new Gw2Item(w)));
                }
            }
            var recipies = await client.WebApi.V2.Recipes.IdsAsync();
            if (recipies == null)
            {
                return;
            }
            var colRecipes = db.GetCollection<Gw2Recipe>("Recipies");
            var existingRecipeIds = colRecipes.Query().Select(w => w.Id).ToList();
            var newRecipies = recipies.ToList().Except(existingRecipeIds).ToList();
            if (newRecipies.Any())
            {
                var newRecipyContents = await webApiClient.Recipes.ManyAsync(newRecipies);
                if (newRecipyContents.Any())
                {
                    colRecipes.InsertBulk(newRecipyContents.Select(w => new Gw2Recipe(w)));
                }
            }
        }

        private readonly LiteDatabase _db;
        private readonly ILiteCollection<Gw2Item> _items;
        private readonly ILiteCollection<Gw2Recipe> _recipes;
        private bool disposedValue;

        public Gw2Database()
        {
            _db = new LiteDatabase("Gw2Crafting.db");
            _items = _db.GetCollection<Gw2Item>("Items");
            _recipes = _db.GetCollection<Gw2Recipe>("Recipies");
            _items.EnsureIndex(x => x.Id);
            _recipes.EnsureIndex(x => x.Id);
        }

        public Gw2Item? GetItem(int id)
        {
            return _items.Query().Where(w => w.Id == id).FirstOrDefault();
        }
        public MaterialItem? GetMaterialItem(int id, int quantity, int unitSellPrice, string categoryName)
        {
            var item = GetItem(id);
            if (item == null)
            {
                return null;
            }
            return new MaterialItem(item, categoryName, quantity, unitSellPrice);
        }
        public Gw2Recipe? GetRecipe(int id)
        {
            return _recipes.Query().Where(w => w.Id == id).FirstOrDefault();
        }
        public IEnumerable<Gw2Recipe> GetAutomaticRecipesForDiscipline(CraftingDisciplineType disciplineType)
        {
            return _recipes.Query().Where(w => w.CraftingDisciplines != null && w.CraftingDisciplines.Contains(disciplineType)).ToList();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _db?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
