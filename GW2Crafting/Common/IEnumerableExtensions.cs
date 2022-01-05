using Gw2Sharp.WebApi.V2.Models;

namespace GW2Crafting.Common
{
    public static class IEnumerableExtensions
    {
        public static string EnumToString<T>(this IEnumerable<T>? itemFlags) where T: Enum
        {
            if (itemFlags == null || !itemFlags.Any())
            {
                return string.Empty;
            }
            return string.Join(",", itemFlags);
        }

    }
}
