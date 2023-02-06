using System;
using GoogleMobileAds.Api;

namespace MokomoGamesLib.Runtime.Ads.Rewards
{
    public class Reward
    {
        private readonly RewardedAd _ads;

        public Reward(RewardedAd ads)
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