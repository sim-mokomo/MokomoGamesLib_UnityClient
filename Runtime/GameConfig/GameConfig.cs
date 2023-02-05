namespace MokomoGamesLib.Runtime.GameConfig
{
    public class GameConfig
    {
        public GameConfig(bool isMute)
        {
            IsMute = isMute;
        }

        public bool IsMute { get; private set; }

        public void ToggleMute()
        {
            IsMute = !IsMute;
        }
    }
}