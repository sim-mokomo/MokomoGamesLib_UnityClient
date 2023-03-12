using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MokomoGamesLib.Runtime.Loadings;
using MokomoGamesLib.Runtime.Loadings.UI;
using MokomoGamesLib.Runtime.Login.SubState;
using MokomoGamesLib.Runtime.StateMachine;
using MokomoGamesLib.Runtime.UI;
using Object = UnityEngine.Object;

namespace MokomoGamesLib.Runtime.Login
{
    public class RootState : State
    {
        private StateMachine.StateMachine _stateMachine;
        private UIManager _uiManager;
        private WheelLoadingPresenter _wheelLoadingPresenter;

        public RootState(Action onCompleted)
        {
            OnCompleted = onCompleted;
        }

        public event Action OnCompleted;

        public override async UniTask Begin(StateChangeRequest inputData, CancellationToken ct)
        {
            await base.Begin(inputData, ct);
            _uiManager = Object.FindObjectOfType<UIManager>();
            var mainState = new MainState();
            mainState.OnCompletedBegin += async () =>
            {
                _uiManager.Destroy(_wheelLoadingPresenter);
                _wheelLoadingPresenter = null;
#if DEV
                await _stateMachine.ChangeStateAsync(
                    typeof(SelectLoginDebugUserState),
                    new SelectLoginDebugUserState.Request(),
                    ct
                );
#else
                OnCompleted?.Invoke();
#endif
            };

#if DEV
            var selectLoginDebugUserState = new SelectLoginDebugUserState();
            selectLoginDebugUserState.OnLoggedIn += () => { OnCompleted?.Invoke(); };
#endif

            _stateMachine = new StateMachine.StateMachine(
                new List<State>
                {
                    mainState,
#if DEV
                    selectLoginDebugUserState
#endif
                },
                ct
            );

            _wheelLoadingPresenter = _uiManager.Create<WheelLoadingPresenter>(UIManager.CanvasOrder.Center);
            _wheelLoadingPresenter.Show(true);
            _stateMachine.ChangeStateAsync(typeof(MainState), new MainState.Request(), ct).Forget();
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            _stateMachine.Tick(deltaTime);

            if (_wheelLoadingPresenter != null) _wheelLoadingPresenter.Tick(deltaTime);
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