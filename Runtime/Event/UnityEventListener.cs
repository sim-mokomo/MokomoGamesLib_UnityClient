using System;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Event
{
    public class UnityEventListener : MonoBehaviour
    {
        private void OnApplicationFocus(bool hasFocus)
        {
            ApplicationFocus?.Invoke(hasFocus);
        }

        public event Action<bool> ApplicationFocus;

        public static UnityEventListener Create()
        {
            return new GameObject("UnityEventListener").AddComponent<UnityEventListener>();
        }
    }
}