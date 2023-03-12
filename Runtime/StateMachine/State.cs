using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MokomoGamesLib.Runtime.StateMachine
{
    public abstract class State
    {
        private StateChangeRequest _stateInputData;
        private DateTime? _timeWhenBackground;
        public bool IsInitialized { get; private set; }
        public event Action OnCompletedEndAfter;

        public TStateChangeRequest GetRequest<TStateChangeRequest>() where TStateChangeRequest : StateChangeRequest
        {
            return _stateInputData as TStateChangeRequest;
        }

        public virtual async UniTask Begin(StateChangeRequest inputData, CancellationToken ct)
        {
            _stateInputData = inputData;
        }

        public virtual async UniTask ForgetBegin(StateChangeRequest request, CancellationToken ct)
        {
        }

        public virtual async UniTask BeginAfter(CancellationToken token)
        {
            IsInitialized = true;
        }

        public virtual async void Tick(float deltaTime)
        {
        }

        public virtual async UniTask End(CancellationToken token)
        {
            IsInitialized = false;
        }

        public virtual async UniTask EndAfter(CancellationToken token)
        {
            OnCompletedEndAfter?.Invoke();
        }

        public abstract StateHistoryItem CreateHistory();

        public virtual void OnAppFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                TimeSpan diffTime = default;
                if (_timeWhenBackground != null) diffTime = DateTime.Now - _timeWhenBackground.Value;

                _timeWhenBackground = null;
                OnForeground(diffTime);
            }
            else
            {
                _timeWhenBackground = DateTime.Now;
                OnBackground();
            }
        }

        public virtual void OnForeground(TimeSpan diffTimeFromBackground)
        {
        }

        public virtual void OnBackground()
        {
        }

        public virtual void OnApplicationQuit()
        {
        }
    }
}