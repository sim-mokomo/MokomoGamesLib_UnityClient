using System;
using GoogleMobileAds.Api;

namespace MokomoGamesLib.Runtime.Ads.Interstitials
{
    public class Interstitial
    {
        private readonly InterstitialAd _interstitialAd;

        public Interstitial(InterstitialAd interstitialAd)
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