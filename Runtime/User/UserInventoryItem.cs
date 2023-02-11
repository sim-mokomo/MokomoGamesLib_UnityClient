using MokomoGamesLib.Runtime.Extensions;
using UnityEngine.Purchasing;

namespace MokomoGamesLib.Runtime.User
{
    public class UserInventoryItem
    {
        public UserInventoryItem(ItemId itemId, string itemType)
        {
            ItemId = itemId;
            ItemType = ProductTypeExtensions.FromString(itemType);
        }

        public ItemId ItemId { get; }
        public ProductType ItemType { get; }
    }
}