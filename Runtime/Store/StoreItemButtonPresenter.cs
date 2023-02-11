using MokomoGamesLib.Runtime.User;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Store
{
    public class StoreItemButtonPresenter : MonoBehaviour
    {
        [SerializeField] private StoreItemButtonView storeItemButtonView;
        [SerializeField] private float showAnimDuration;
        [SerializeField] private string productID;
        private StoreManager _storeManger;
        private InventoryManager _inventoryManager;
        
        private void Constructor(StoreManager storeManger,InventoryManager inventoryManager)
        {
            _storeManger = storeManger;
            _inventoryManager = inventoryManager;
            storeItemButtonView.OnClickedPurchaseButton += () =>
            {
                _storeManger.Purchase(productID);
            };
        
            var noAdsItemId = new ItemId("no_ads");
            _inventoryManager.OnLoaded += () => { storeItemButtonView.Show(!_inventoryManager.HasItem(noAdsItemId), showAnimDuration); };
        }
    }
}

