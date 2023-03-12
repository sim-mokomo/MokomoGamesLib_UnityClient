using System;
using GoogleMobileAds.Api;

namespace MokomoGamesLib.Runtime.Ads.Banners
{
    public class Banner
    {
        private readonly BannerView _bannerView;

        public Banner(BannerView bannerView)
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
            _bannerView.LoadAd(AdsManager.CreateAdMobRequest());
        }

        public void Destroy()
        {
            _bannerView.Destroy();
        }

        public event Action OnAdLoaded;
        public event Action OnAdClosed;
    }
}