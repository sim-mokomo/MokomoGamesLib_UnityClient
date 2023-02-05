using UnityEngine;

namespace MokomoGamesLib.Runtime.Utilities
{
    public static class FloatExtensions
    {
        public static float ToPercent(this float self, float origin)
        {
            return self.ToPercentRate(origin) * 100f;
        }

        public static float ToPercentRate(this float self, float origin)
        {
            if (Mathf.Approximately(origin, 0))
                return 0f;
            return self / origin;
        }
    }
}