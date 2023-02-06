using System;
using GoogleMobileAds.Api;

namespace MokomoGamesLib.Runtime.Ads
{
    public class AdsReward
    {
        private readonly RewardedAd _ads;

        public AdsReward(RewardedAd ads)
        {
            _ads = ads;
            ads.OnAdFullScreenContentClosed += () => OnAdClosed?.Invoke();
        }

        public void Show()
        {
            _ads.Show(reward =>
            {
                OnEarnedReward?.Invoke();
            });
        }
        
        public event Action OnAdClosed;
        public event Action OnEarnedReward;
    }
}