using System;
using GoogleMobileAds.Api;

namespace MokomoGamesLib.Runtime.Ads
{
    public class AdsReward : IAds
    {
        private readonly RewardedAd ads;

        public AdsReward(RewardedAd ads)
        {
            this.ads = ads;
            ads.OnAdClosed += (sender, args) => { OnAdClosed?.Invoke(args); };
            ads.OnAdLoaded += (sender, args) => { OnAdLoaded?.Invoke(args); };
        }

        public AdsType AdsType => AdsType.Reward;

        public void Show()
        {
            ads.Show();
        }

        public void Load()
        {
            ads.LoadAd(AdsManager.CreateAdMobRequest().Build());
        }

        public void Destroy()
        {
            // NOTE: 明示的に破壊できない
        }

        public event Action<EventArgs> OnAdLoaded;
        public event Action<EventArgs> OnAdClosed;
    }
}