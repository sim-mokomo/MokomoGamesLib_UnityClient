using System;

namespace MokomoGamesLib.Runtime.Timers
{
    // 秒数ベースにカウントダウンするタイマー
    public class Timer
    {
        private const int DecreaseSecondsInterval = 1;
        private readonly int _initSeconds;
        private int _currentSeconds;
        private float _elapsedTime;

        public Timer(int initSeconds)
        {
            _initSeconds = Math.Max(initSeconds, 0);
            Reset();
        }

        public TimeSpan CurrentTime => TimeSpan.FromSeconds(_currentSeconds);
        public bool IsTimedUp => _currentSeconds <= 0;
        public bool IsPausing { get; private set; }

        public event Action OnElapsedSecond;
        public event Action OnCompleted;

        public void Start()
        {
            Reset();
            Pause(false);
        }

        public void Update(float deltaTime)
        {
            if (IsTimedUp || IsPausing) return;

            _elapsedTime -= deltaTime;
            if (_elapsedTime > 0.0f) return;

            _elapsedTime = DecreaseSecondsInterval;
            DecreaseSeconds(1);
        }

        public void Pause(bool enable)
        {
            IsPausing = enable;
        }

        public void Reset()
        {
            _currentSeconds = _initSeconds;
            _elapsedTime = DecreaseSecondsInterval;
            OnElapsedSecond?.Invoke();
        }

        public void DecreaseSeconds(int diff)
        {
            var prevSeconds = _currentSeconds;
            _currentSeconds = Math.Max(_currentSeconds - diff, 0);
            OnElapsedSecond?.Invoke();

            if (prevSeconds > 0 && IsTimedUp) OnCompleted?.Invoke();
        }
    }
}