using Cysharp.Threading.Tasks;
using MokomoGamesLib.Runtime.Store;
using UnityEngine;

namespace MokomoGamesLib.Runtime.User
{
    public class InventoryManager : MonoBehaviour
    {
        private UserInventory _userInventory;

        public bool HasItem(ItemId itemId)
        {
            return _userInventory.HasItem(itemId);
        }

        public async UniTask<UserInventory> LoadInventoryItems()
        {
            _userInventory = await PlayFabStoreRepository.LoadInventoryItems();
            return _userInventory;
        }

        public UniTask AddItem(string itemId)
        {
            return PlayFabStoreRepository.AddItemAsync(itemId);
        }
    }
}