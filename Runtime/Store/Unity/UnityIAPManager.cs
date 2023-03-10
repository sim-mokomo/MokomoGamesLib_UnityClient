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

        // NOTE: Pending中のままアプリが落ちた場合、IAP初期化以降再度実行される。
        // Androidの場合、IAP初期化後、リストア目的で各種プロダクトの処理が走る。
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEventArgs)
        {
            if (purchaseEventArgs.purchasedProduct == null)
            {
                Debug.LogError("知らない製品を購入しようとしています");
                return PurchaseProcessingResult.Complete;
            }

            // NOTE: 未購入/消費済みの場合はfalse
            if (!purchaseEventArgs.purchasedProduct.hasReceipt)
            {
                Debug.LogError("このプロダクトはレシートを持っていません");
                return PurchaseProcessingResult.Complete;
            }

#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR_OSX
            // NOTE: 購入済みの永続アイテムは復元を行う。
            var isPermanentItem =
                purchaseEventArgs.purchasedProduct.definition.type == ProductType.Subscription ||
                purchaseEventArgs.purchasedProduct.definition.type == ProductType.NonConsumable;
            var purchasedPermanentItem = isPermanentItem && purchaseEventArgs.purchasedProduct.hasReceipt;
            if (purchasedPermanentItem)
            {
                try
                {
                    Debug.Log("復元時のレシートのローカル検証を開始");
                    // var validator = new CrossPlatformValidator(
                    //     GooglePlayTangle.Data(),
                    //     AppleTangle.Data(),
                    //     Application.identifier
                    // );
                    // var receipts = validator.Validate(purchaseEventArgs.purchasedProduct.receipt);
                    Debug.Log("復元時のレシートのローカル検証が成功");
                    // foreach (var purchaseReceipt in receipts)
                    // {
                    //     Debug.Log(purchaseReceipt.productID);
                    //     Debug.Log(purchaseReceipt.purchaseDate);
                    //     Debug.Log(purchaseReceipt.transactionID);
                    // }

                    // TODO: アイテム追加
                    _storeController.ConfirmPendingPurchase(purchaseEventArgs.purchasedProduct);
                }
                catch (IAPSecurityException exception)
                {
                    Debug.Log("復元時のレシートのローカル検証が失敗・または違法");
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
            //         Debug.Log("Android購入確定");
            //     },
            //     error =>
            //     {
            //         _storeController.ConfirmPendingPurchase(purchaseEventArgs.purchasedProduct);
            //         Debug.Log($"Android購入確定失敗 {error.GenerateErrorReport()}");
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
                    Debug.Log("iOS購入確定成功");
                },
                error =>
                {
                    _storeController.ConfirmPendingPurchase(purchaseEventArgs.purchasedProduct);
                    Debug.Log($"iOS購入確定失敗 {error.GenerateErrorReport()}");
                }
            );
#endif
            Debug.Log($"トランザクション確定待ち... {purchaseEventArgs.purchasedProduct.transactionID}");
            return PurchaseProcessingResult.Pending;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            Debug.LogError($"購入失敗: {reason.ToString()}");
        }

        public event Action OnPurchased;

        public async UniTask InitializeAsync()
        {
            var task = UniTask.WhenAny(
                OnInitializedStoreAsync,
                OnInitializeStoreFailedAsync
            );

            // NOTE: カタログ作成
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
                Debug.Log(success ? "リストアトランザクション成功" : "リストアトランザクション失敗");
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