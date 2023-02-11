using UnityEngine.Purchasing;

namespace MokomoGamesLib.Runtime.Extensions
{
    public static class ProductTypeExtensions
    {
        public static ProductType FromString(string fromString)
        {
            return fromString switch
            {
                "NonConsumable" => ProductType.NonConsumable,
                "Consumable" => ProductType.Consumable,
                "Subscription" => ProductType.Subscription,
                _ => ProductType.Consumable
            };
        }
    }
}