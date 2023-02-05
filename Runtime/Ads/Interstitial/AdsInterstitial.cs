using System;
using GoogleMobileAds.Api;

namespace MokomoGamesLib.Runtime.Ads.Interstitial
{
    public class AdsInterstitial : IAds
    {
        private readonly InterstitialAd _interstitialAd;

        public AdsInterstitial(InterstitialAd interstitialAd)
        {
            _interstitialAd = interstitialAd;
            _interstitialAd.OnAdClosed += (sender, args) => { OnAdClosed?.Invoke(args); };
            _interstitialAd.OnAdLoaded += (sender, args) => { OnAdLoaded?.Invoke(args); };
        }

        public AdsType AdsType => AdsType.Interstitial;

        public void Show()
        {
            _interstitialAd.Show();
        }

        public void Load()
        {
            _interstitialAd.LoadAd(AdsManager.CreateAdMobRequest().Build());
        }

        public void Destroy()
        {
            _interstitialAd.Destroy();
        }

        public event Action<EventArgs> OnAdLoaded;
        public event Action<EventArgs> OnAdClosed;
    }
}