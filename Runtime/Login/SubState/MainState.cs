using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MokomoGamesLib.Runtime.Extensions;
using MokomoGamesLib.Runtime.GameConfigs;
using MokomoGamesLib.Runtime.Localizations;
using MokomoGamesLib.Runtime.Login.PlayFab;
using MokomoGamesLib.Runtime.Network;
using MokomoGamesLib.Runtime.Network.PlayFab;
using MokomoGamesLib.Runtime.StateMachine;
using MokomoGamesLib.Runtime.User;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
#endif

namespace MokomoGamesLib.Runtime.Login.SubState
{
    internal class MainState : State
    {
        public event Action OnCompletedBegin;

        public override async UniTask Begin(StateChangeRequest inputData, CancellationToken ct)
        {
            await base.Begin(inputData, ct);

#if DEV
            await new LoginService().CustomLoginAsync("LoginUser");
#else
            // TODO: APIからPlayFabTitleIdを受け取る
            await new Service("7FDF5").LoginAsync();
#endif

#if UNITY_EDITOR && DEV
            var gameDebugSaveData = AssetDatabase.LoadAssetAtPath<GameDebugSaveData>(GameDebugSaveData.SaveDataFileName);
            await Object
                .FindObjectOfType<LocalizeManager>()
                .LoadAsync(gameDebugSaveData.GameLanguage);
#else
            await Object
                .FindObjectOfType<LocalizeManager>()
                .LoadAsync(Application.systemLanguage.ConvertToAppLanguage());
#endif

            var inventoryManager = Object.FindObjectOfType<InventoryManager>();
            if (inventoryManager != null) await inventoryManager.LoadInventoryItems();

            // var storeManager = Object.FindObjectOfType<StoreManager>();
            // storeManager.IapService.OnPurchased += async () =>
            // {
            //     await inventoryManager.LoadInventoryItems();
            // };
            // storeManager.IapService.OnRestored += async () =>
            // {
            //     await inventoryManager.LoadInventoryItems();
            // };
            // await storeManager.InitializeAsync();

            Object.FindObjectOfType<GameConfigManager>().Load();

            OnCompletedBegin?.Invoke();
        }

        public override StateHistoryItem CreateHistory()
        {
            return null;
        }

        public class Request : StateChangeRequest
        {
        }
    }
}