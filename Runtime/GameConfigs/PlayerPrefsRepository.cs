using UnityEngine;

namespace MokomoGamesLib.Runtime.GameConfigs
{
    public class PlayerPrefsRepository
    {
        private const string IsMuteKey = "isMute";

        public void Save(GameConfig gameConfig)
        {
            PlayerPrefs.SetString(IsMuteKey, gameConfig.IsMute.ToString());
        }

        public GameConfig Load()
        {
            var isMute = false;
            if (PlayerPrefs.HasKey(IsMuteKey)) isMute = bool.Parse(PlayerPrefs.GetString(IsMuteKey));
            return new GameConfig(isMute);
        }
    }
}