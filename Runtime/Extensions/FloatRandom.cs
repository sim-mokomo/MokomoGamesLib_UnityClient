using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MokomoGamesLib.Runtime.Extensions
{
    [Serializable]
    public class FloatRandom
    {
        [SerializeField] private float min;
        [SerializeField] private float max;

        public FloatRandom(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public float Rand()
        {
            return Random.Range(min, max);
        }
    }
}