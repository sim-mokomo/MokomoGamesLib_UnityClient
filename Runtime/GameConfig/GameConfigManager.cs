using System;
using UnityEngine;

namespace MokomoGamesLib.Runtime.GameConfig
{
    public class GameConfigManager : MonoBehaviour
    {
        private readonly GameConfigPlayerPrefsRepository _gameConfigRepository;

        public GameConfigManager()
        {
            _gameConfigRepository = new GameConfigPlayerPrefsRepository();
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