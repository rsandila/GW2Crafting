using Gw2Sharp.WebApi.V2.Models;
using LiteDB;

namespace GW2Crafting.Caching.Models
{
    public class Gw2Recipe
    {
        public Gw2Recipe() { }
        internal Gw2Recipe(Recipe original)
        {
            Id = original.Id;
            Type = original.Type;
            MinRating = original.MinRating;
            OutputItemId = original.OutputItemId;
            OutputItemCount = original.OutputItemCount;
            RecipeFlags = original.Flags.Select(w => w.Value).ToList();
            Ingredients = original.Ingredients.Select(w => new Ingredient { Id = w.ItemId, Count = w.Count });
            CraftingDisciplines = original.Disciplines.Select(w => w.Value).ToList();
            if (original.GuildIngredients != null)
            {
                GuildIngredients = original.GuildIngredients.Select(w => new Ingredient { Id = w.UpgradeId, Count = w.Count });
            }
            OutputUpgradedId = original.OutputUpgradeId;
        }
        public int Id { get; set; }
        public RecipeType? Type { get; set; }
        public int MinRating { get; set; }
        public int OutputItemId { get; set; }  
        public int OutputItemCount { get; set; }
        public IEnumerable<RecipeFlag>? RecipeFlags { get; set; }
        public IEnumerable<Ingredient>? Ingredients { get; set; }
        public IEnumerable<CraftingDisciplineType>? CraftingDisciplines { get; set; }
        public IEnumerable<Ingredient>? GuildIngredients { get; set; }   
        public int OutputUpgradedId { get; set; }
    }
}
