using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using MokomoGamesLib.Runtime.Debugs;
using MokomoGamesLib.Runtime.Debugs.GameDebug;
using MokomoGamesLib.Runtime.Loading;
using MokomoGamesLib.Runtime.StateMachine;
using MokomoGamesLib.Runtime.UI;
using PlayEveryWare.EpicOnlineServices;
using Object = UnityEngine.Object;

namespace MokomoGamesLib.Runtime.EOS.Auth
{
    public class RootState : State
    {
        private WheelLoadingPresenter _wheelLoadingPresenter;
        public event Action<ProductUserId> OnCompleted;

        public override async UniTask Begin(StateChangeRequest inputData, CancellationToken ct)
        {
            await base.Begin(inputData, ct);

            var uiManager = Object.FindObjectOfType<UIManager>();
            var eosManager = EOSManager.Instance;
            eosManager.SetIsLoggedIn(false);

            _wheelLoadingPresenter = uiManager.Create<WheelLoadingPresenter>(UIManager.CanvasOrder.Front);
            _wheelLoadingPresenter.Show(true);

            var eosLoginService =
                new LoginService(eosManager.GetEOSConnectInterface(), eosManager.GetEOSAuthInterface());

            var id = string.Empty;
            var token = string.Empty;
            var loginType = LoginService.LoginType.None;
#if UNITY_EDITOR
            // note: エピックアカウント別の開発者向けIDとTokenを設定する
            var debugSaveData = GameDebugRepository.LoadIfNotExistCreate();
            id = debugSaveData.CurrentEpicLoginInfo.Id;
            token = debugSaveData.CurrentEpicLoginInfo.UserName;
            loginType = LoginService.LoginType.Local;
#else
            id = "EOSUser";
            token = null;
            loginType = LoginService.LoginType.DeviceId;
#endif
            eosLoginService.Login(id, token, loginType, info =>
            {
                uiManager.Destroy(_wheelLoadingPresenter);
                eosManager.SetIsLoggedIn(true);
                OnCompleted?.Invoke(info);
            });
        }

        public override async UniTask Tick(float deltaTime, CancellationToken ct)
        {
            base.Tick(deltaTime, ct).Forget();

            EOSManager.Instance.Tick();
            _wheelLoadingPresenter.Tick(deltaTime);
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