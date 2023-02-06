using System;
using GoogleMobileAds.Api;

namespace MokomoGamesLib.Runtime.Ads
{
    public class AdsBanner
    {
        private readonly BannerView _bannerView;

        public AdsBanner(BannerView bannerView)
        {
            _bannerView = bannerView;
            _bannerView.OnAdFullScreenContentClosed += () => OnAdClosed?.Invoke();
            _bannerView.OnBannerAdLoaded += () => OnAdLoaded?.Invoke();
        }

        public void Show()
        {
            _bannerView.Show();
        }

        public void Load()
        {
            _bannerView.LoadAd(AdsManager.CreateAdMobRequest().Build());
        }

        public void Destroy()
        {
            _bannerView.Destroy();
        }

        public event Action OnAdLoaded;
        public event Action OnAdClosed;
    }
}