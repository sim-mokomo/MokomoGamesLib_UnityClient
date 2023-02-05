using System;
using GoogleMobileAds.Api;

namespace MokomoGamesLib.Runtime.Ads.Banner
{
    public class AdsBanner : IAds
    {
        private readonly BannerView _bannerView;

        public AdsBanner(BannerView bannerView)
        {
            _bannerView = bannerView;
            _bannerView.OnAdClosed += (sender, args) => { OnAdClosed?.Invoke(args); };
            _bannerView.OnAdLoaded += (sender, args) => { OnAdLoaded?.Invoke(args); };
        }

        public AdsType AdsType => AdsType.Banner;

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

        public event Action<EventArgs> OnAdLoaded;
        public event Action<EventArgs> OnAdClosed;
    }
}