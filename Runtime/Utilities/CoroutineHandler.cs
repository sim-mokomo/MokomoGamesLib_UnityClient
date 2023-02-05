using System.Collections;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Utilities
{
    public class CoroutineHandler : MonoBehaviour
    {
        public Coroutine WrapStartCoroutine(IEnumerator routine)
        {
            var coroutine = StartCoroutine(routine);
            return coroutine;
        }

        public void WrapStopCoroutine(Coroutine coroutine)
        {
            if (coroutine == null) return;
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }
}