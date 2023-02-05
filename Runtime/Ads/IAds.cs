namespace MokomoGamesLib.Runtime.Ads
{
    public interface IAds
    {
        AdsType AdsType { get; }
        void Show();
        void Load();
        void Destroy();
    }
}