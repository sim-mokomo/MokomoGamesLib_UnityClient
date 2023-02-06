using System;
using GoogleMobileAds.Api;

namespace MokomoGamesLib.Runtime.Ads.Interstitial
{
    public class AdsInterstitial
    {
        private readonly InterstitialAd _interstitialAd;

        public AdsInterstitial(InterstitialAd interstitialAd)
        {
            _interstitialAd = interstitialAd;
            _interstitialAd.OnAdFullScreenContentClosed += () => OnAdClosed?.Invoke();
        }

        public void Show()
        {
            _interstitialAd.Show();
        }

        public void Destroy()
        {
            _interstitialAd.Destroy();
        }
        
        public event Action OnAdClosed;
    }
}