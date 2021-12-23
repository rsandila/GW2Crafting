
using GW2Crafting.Common;
using GW2Crafting.Pages;
using Gw2Sharp;
using Gw2Sharp.WebApi.Exceptions;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Extensions.Caching.Memory;

namespace GW2Crafting.Caching
{
    public enum CacheTypeId
    {
        Connection,
        Characters,
        AccessToken,
        ListType,
        SelectedCharacter,
        Client,
        MaterialCategories,
        Materials
    }
    public class Gw2TokenCache
    {
        private readonly IMemoryCache cache;

        public Gw2TokenCache(ILoggerFactory loggerFactory)
        {
            cache = new MemoryCache(new MemoryCacheOptions(), loggerFactory);
        }

        public void Add(Guid id, CacheTypeId type, object value)
        {
            cache.Set(CalculateId(id, type), value, CalculateExpiration);
        }

        public async Task<T?> Get<T>(Guid id, CacheTypeId type) where T : class
        {
            if (cache.TryGetValue(CalculateId(id, type), out var value))
            {
                return value as T;
            }
            try
            {
                switch (type)
                {
                    case CacheTypeId.Client:
                        {
                            var connection = await Get<Connection>(id, CacheTypeId.Connection);
                            if (connection == null)
                            {
                                return default;
                            }
                            var client = new Gw2Client(connection) as T;
                            return client;
                        }

                    case CacheTypeId.Connection:
                        {
                            string? accessToken = await Get<string>(id, CacheTypeId.AccessToken);
                            if (string.IsNullOrWhiteSpace(accessToken))
                            {
                                return default;
                            }
                            var connection = new Connection(accessToken);
                            if (connection == null)
                            {
                                return default;
                            }
                            cache.Set(CalculateId(id, type), connection, CalculateExpiration);
                            return connection as T;
                        }
                    case CacheTypeId.Characters:
                        {
                            var client = await Get<Gw2Client>(id, CacheTypeId.Client);
                            if (client == null)
                            {
                                return default;
                            }
                            var webApiClient = client.WebApi.V2;

                            var characterList = await webApiClient.Characters.AllAsync();
                            if (characterList != null)
                            {
                                cache.Set(CalculateId(id, type), characterList, CalculateExpiration);
                            }
                            return characterList as T;
                        }
                    case CacheTypeId.MaterialCategories:
                        {
                            var client = await Get<Gw2Client>(id, CacheTypeId.Client);
                            if (client == null)
                            {
                                return default;
                            }
                            var webApiClient = client.WebApi.V2;
                            var materials = await client.WebApi.V2.Materials.AllAsync();
                            if (materials != null)
                            {
                                cache.Set(CalculateId(id, type), materials, CalculateExpiration);
                            }
                            return materials as T;
                        }
                    case CacheTypeId.Materials:
                        {
                            var client = await Get<Gw2Client>(id, CacheTypeId.Client);
                            if (client == null)
                            {
                                return default;
                            }
                            var material = await client.WebApi.V2.Account.Materials.GetAsync();
                            if (material == null)
                            {
                                return default;
                            }
                            if (material != null)
                            {
                                cache.Set(CalculateId(id, CacheTypeId.Materials), material, CalculateExpiration);
                            }
                            return material as T;
                        }
                    default:
                        return default;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return default;
            }
            
        }
        internal async Task<IEnumerable<Gw2ResolvedRecipe>> GetResolvedRecipies(Gw2Database _database, Guid id, string characterName)
        {
            var key = $"{id}_{characterName}";
            if (cache.TryGetValue(key, out IEnumerable<Gw2ResolvedRecipe> recipies) && recipies.Any())
            {
                return recipies;
            }
            var client = await Get<Gw2Client>(id, CacheTypeId.Client);
            if (client == null)
            {
                return Array.Empty<Gw2ResolvedRecipe>();
            }
            var characters = await Get<IApiV2ObjectList<Character>>(id, CacheTypeId.Characters);
            if (characters == null)
            {
                return Array.Empty<Gw2ResolvedRecipe>();
            }
            var character = characters.FirstOrDefault(w => w.Name == characterName);
            if (character == null || character.Recipes == null)
            {
                return Array.Empty<Gw2ResolvedRecipe>();
            }
            var resolvedRecipies = character.Recipes.Select(w => _database.GetRecipe(w));
            await Listings.CacheListingsFor(this, id, resolvedRecipies.Select(w => w?.OutputItemId ?? 0));
            await Listings.CacheListingsFor(this, id, resolvedRecipies.SelectMany(w => w?.Ingredients?.Select(q => q.Id) ?? Array.Empty<int>()));
            var localRecipies = new List<Gw2ResolvedRecipe>();
            foreach (var item in resolvedRecipies)
            {
                if (item != null)
                {
                    localRecipies.Add(new Gw2ResolvedRecipe(item, _database, this));
                }
            }
            cache.Set(key, localRecipies);
            return localRecipies;
        }
        internal ICollection<CommerceListings> GetListingsFor(IEnumerable<int> listingIds)
        {
            var commerceListings = cache.Get<Dictionary<int, CommerceListings>>("CommerceListings");
            if (commerceListings == null)
            {
                return Array.Empty<CommerceListings>();
            }
            return commerceListings.Where(w => listingIds.Contains(w.Key)).Select(w => w.Value).ToList();
        }


        internal async Task CacheCommerceListings(IEnumerable<int> listingIds, Guid id)
        {
            var commerceListings = cache.Get<Dictionary<int, CommerceListings>>("CommerceListings");
            if (commerceListings == null)
            {
                commerceListings = new Dictionary<int, CommerceListings>();
                cache.Set("CommerceListings", commerceListings, CalculateExpiration);
            }
            var missing = listingIds.Except(commerceListings.Keys).ToList();
            if (missing.Any())
            {
                var client = await Get<Gw2Client>(id, CacheTypeId.Client);
                if (client != null)
                {
                    try
                    {
                        var listings = await client.WebApi.V2.Commerce.Listings.ManyAsync(missing);
                        if (listings != null)
                        {
                            foreach (var item in listings)
                            {
                                commerceListings.Add(item.Id, item);
                            }
                        }
                    }
                    catch (NotFoundException)
                    {
                        // just eat it
                    }
                }
            }
        }

        private static string CalculateId(Guid id, CacheTypeId type) => $"{id}_{type}";
        private static DateTime CalculateExpiration => DateTime.Now.AddMinutes(30);
    }
}
