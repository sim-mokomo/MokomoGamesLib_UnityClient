using System;

namespace MokomoGamesLib.Runtime.Counters
{
    public class Counter
    {
        private readonly int _endCounter;
        private int _currentCount;

        public Counter(int startCount, int endCounter)
        {
            _currentCount = startCount;
            _endCounter = endCounter;
        }

        public event Action OnEnd;

        public void Increase(int diff)
        {
            _currentCount += diff;
            if (_currentCount == _endCounter) OnEnd?.Invoke();
        }
    }
}