using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using Protobuf;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

namespace MokomoGamesLib.Runtime.Store.Unity
{
    public class UnityIAPManager : IStoreListener
    {
        private readonly CancellationToken _cancellationToken;
        private readonly Func<UniTask<List<ProductDefinition>>> _requestCatalog;
        private IExtensionProvider _extensionProvider;

        private AsyncReactiveProperty
            <(IStoreController storeController, IExtensionProvider extensionProvider)> _onInitializedStore;

        private AsyncReactiveProperty<InitializationFailureReason> _onInitializedStoreFailed;
        private IStoreController _storeController;
        private bool IsInitialized => _storeController != null && _extensionProvider != null;
        public event Action OnRestored;
        public event Action OnInitializedEvent;

        public UnityIAPManager(Func<UniTask<List<ProductDefinition>>> requestCatalog, CancellationToken cancellationToken)
        {
            _requestCatalog = requestCatalog;
            _cancellationToken = cancellationToken;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private UniTask<(IStoreController storeController, IExtensionProvider extensionProvider)> OnInitializedStoreAsync
        {
            get
            {
                if (_onInitializedStore != null) return _onInitializedStore.WaitAsync();

                _onInitializedStore =
                    new AsyncReactiveProperty
                        <(IStoreController storeController, IExtensionProvider extensionProvider)>(default);
                _onInitializedStore.AddTo(_cancellationToken);
                return _onInitializedStore.WaitAsync();
            }
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private UniTask<InitializationFailureReason> OnInitializeStoreFailedAsync
        {
            get
            {
                if (_onInitializedStoreFailed != null) return _onInitializedStoreFailed.WaitAsync();

                _onInitializedStoreFailed = new AsyncReactiveProperty<InitializationFailureReason>(default);
                _onInitializedStoreFailed.AddTo(_cancellationToken);
                return _onInitializedStoreFailed.WaitAsync();
            }
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _onInitializedStore.Value = (controller, extensions);
            OnInitializedEvent?.Invoke();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            _onInitializedStoreFailed.Value = error;
        }

        // NOTE: Pending??????????????????????????????????????????IAP???????????????????????????????????????
        // Android????????????IAP??????????????????????????????????????????????????????????????????????????????
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEventArgs)
        {
            if (purchaseEventArgs.purchasedProduct == null)
            {
                Debug.LogError("??????????????????????????????????????????????????????");
                return PurchaseProcessingResult.Complete;
            }

            // NOTE: ?????????/????????????????????????false
            if (!purchaseEventArgs.purchasedProduct.hasReceipt)
            {
                Debug.LogError("????????????????????????????????????????????????????????????");
                return PurchaseProcessingResult.Complete;
            }

#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR_OSX
            // NOTE: ??????????????????????????????????????????????????????
            var isPermanentItem =
                purchaseEventArgs.purchasedProduct.definition.type == ProductType.Subscription ||
                purchaseEventArgs.purchasedProduct.definition.type == ProductType.NonConsumable;
            var purchasedPermanentItem = isPermanentItem && purchaseEventArgs.purchasedProduct.hasReceipt;
            if (purchasedPermanentItem)
            {
                try
                {
                    Debug.Log("??????????????????????????????????????????????????????");
                    // var validator = new CrossPlatformValidator(
                    //     GooglePlayTangle.Data(),
                    //     AppleTangle.Data(),
                    //     Application.identifier
                    // );
                    // var receipts = validator.Validate(purchaseEventArgs.purchasedProduct.receipt);
                    Debug.Log("??????????????????????????????????????????????????????");
                    // foreach (var purchaseReceipt in receipts)
                    // {
                    //     Debug.Log(purchaseReceipt.productID);
                    //     Debug.Log(purchaseReceipt.purchaseDate);
                    //     Debug.Log(purchaseReceipt.transactionID);
                    // }

                    // TODO: ??????????????????
                    _storeController.ConfirmPendingPurchase(purchaseEventArgs.purchasedProduct);
                }
                catch (IAPSecurityException exception)
                {
                    Debug.Log("????????????????????????????????????????????????????????????????????????");
                    return PurchaseProcessingResult.Complete;
                }
            }
#endif

            var receipt = Receipt.Parser.ParseJson(purchaseEventArgs.purchasedProduct.receipt);
#if UNITY_ANDROID
            // PlayFabStoreRepository.ValidateGooglePlayReceipt(
            //     purchaseEventArgs,
            //     receipt,
            //     result =>
            //     {
            //         _storeController.ConfirmPendingPurchase(purchaseEventArgs.purchasedProduct);
            //         OnPurchased?.Invoke();
            //         Debug.Log("Android????????????");
            //     },
            //     error =>
            //     {
            //         _storeController.ConfirmPendingPurchase(purchaseEventArgs.purchasedProduct);
            //         Debug.Log($"Android?????????????????? {error.GenerateErrorReport()}");
            //     }
            // );
#elif UNITY_IOS || UNITY_EDITOR_OSX
            PlayFabStoreRepository.ValidateAppStoreReceipt(
                purchaseEventArgs,
                receipt,
                result =>
                {
                    _storeController.ConfirmPendingPurchase(purchaseEventArgs.purchasedProduct);
                    OnPurchased?.Invoke();
                    Debug.Log("iOS??????????????????");
                },
                error =>
                {
                    _storeController.ConfirmPendingPurchase(purchaseEventArgs.purchasedProduct);
                    Debug.Log($"iOS?????????????????? {error.GenerateErrorReport()}");
                }
            );
#endif
            Debug.Log($"????????????????????????????????????... {purchaseEventArgs.purchasedProduct.transactionID}");
            return PurchaseProcessingResult.Pending;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            Debug.LogError($"????????????: {reason.ToString()}");
        }

        public event Action OnPurchased;

        public async UniTask InitializeAsync()
        {
            var task = UniTask.WhenAny(
                OnInitializedStoreAsync,
                OnInitializeStoreFailedAsync
            );

            // NOTE: ??????????????????
            var productDefineList = await _requestCatalog();
            var bundler = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            bundler.AddProducts(productDefineList);

            UnityPurchasing.Initialize(this, bundler);

            var (taskIndex,
                (storeController, extensionProvider),
                initializationFailureReason) = await task;

            // NOTE: OnInitialized
            if (taskIndex == 0)
            {
                _storeController = storeController;
                _extensionProvider = extensionProvider;
            } // NOTE: OnInitializeFailed
            else if (taskIndex == 1)
            {
                throw new FailedToInitializedStoreException(initializationFailureReason);
            }
        }

        public void Restore()
        {
            if (!IsInitialized)
            {
                return;
            }
            
            void RestoreTransactionEvent(bool success)
            {
                Debug.Log(success ? "??????????????????????????????????????????" : "??????????????????????????????????????????");
                OnRestored?.Invoke();
            }
#if UNITY_IOS || UNITY_EDITOR_OSX
            _extensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions(RestoreTransactionEvent);
#elif UNITY_ANDROID
            _extensionProvider.GetExtension<IGooglePlayStoreExtensions>().RestoreTransactions(RestoreTransactionEvent);
#endif
        }

        public void Purchase(string productId)
        {
            if (!IsInitialized)
            {
                return;
            }
            _storeController.InitiatePurchase(productId);
        }

        public class StoreException : Exception
        {
        }

        public class FailedToInitializedStoreException : StoreException
        {
            public FailedToInitializedStoreException(InitializationFailureReason reason)
            {
                Reason = reason;
            }

            public InitializationFailureReason Reason { get; }
        }
    }
}