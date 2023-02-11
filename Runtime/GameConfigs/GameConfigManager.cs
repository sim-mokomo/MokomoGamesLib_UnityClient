using System;
using UnityEngine;

namespace MokomoGamesLib.Runtime.GameConfigs
{
    public class GameConfigManager : MonoBehaviour
    {
        private readonly PlayerPrefsRepository _gameConfigRepository;

        public GameConfigManager()
        {
            _gameConfigRepository = new PlayerPrefsRepository();
        }

        public GameConfig Config { get; private set; }
        public event Action<GameConfig> OnUpdatedGameConfig;

        public void Save()
        {
            _gameConfigRepository.Save(Config);
            OnUpdatedGameConfig?.Invoke(Config);
        }

        public GameConfig Load()
        {
            Config = _gameConfigRepository.Load();
            OnUpdatedGameConfig?.Invoke(Config);
            return Config;
        }
    }
}