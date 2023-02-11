using System;
using Cysharp.Threading.Tasks;
using MokomoGamesLib.Runtime.Store.Unity;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Store
{
    public class StoreManager : MonoBehaviour
    {
        public UnityIAPManager IAPService { get; private set; }
        
        public event Action OnRestored;
        
        public async UniTask InitializeAsync()
        {
            IAPService = new UnityIAPManager(PlayFab.StoreRepository.RequestCatalog, gameObject.GetCancellationTokenOnDestroy());
            IAPService.OnRestored += OnRestored;
            await IAPService.InitializeAsync();
        }
        
        public void Purchase(string productId)
        {
            IAPService.Purchase(productId);
        }
        
        public void Restore()
        {
            IAPService.Restore();
        }
    }
}