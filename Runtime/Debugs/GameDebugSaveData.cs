using System;
using MokomoGamesLib.Runtime.Localization;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Debugs
{
    [Serializable]
    public class GameDebugSaveData : ScriptableObject
    {
        [SerializeField] private AppLanguage _gameLanguage;

        [SerializeField] private EpicLoginInfo _currentEpicLoginInfo = new();

        public Action<AppLanguage> OnChangedGameLanguage;

        public AppLanguage GameLanguage
        {
            get => _gameLanguage;
            set
            {
                _gameLanguage = value;
                OnChangedGameLanguage?.Invoke(_gameLanguage);
            }
        }

        public EpicLoginInfo CurrentEpicLoginInfo
        {
            get => _currentEpicLoginInfo;
            set => _currentEpicLoginInfo = value;
        }

        [Serializable]
        public class EpicLoginInfo
        {
            public string Port;
            public string UserName;

            public string Id => $"localhost:{Port}";
        }
    }
}