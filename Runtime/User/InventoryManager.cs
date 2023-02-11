using System;
using Cysharp.Threading.Tasks;
using MokomoGamesLib.Runtime.Store;
using MokomoGamesLib.Runtime.Store.PlayFab;
using UnityEngine;

namespace MokomoGamesLib.Runtime.User
{
    public class InventoryManager : MonoBehaviour
    {
        private UserInventory _userInventory;
        public event Action OnLoaded;

        public bool HasItem(ItemId itemId)
        {
            return _userInventory.HasItem(itemId);
        }

        public async UniTask<UserInventory> LoadInventoryItems()
        {
            _userInventory = await StoreRepository.LoadInventoryItems();
            OnLoaded?.Invoke();
            return _userInventory;
        }

        public UniTask AddItem(string itemId)
        {
            return StoreRepository.AddItemAsync(itemId);
        }
    }
}