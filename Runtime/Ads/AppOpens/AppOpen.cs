using GoogleMobileAds.Api;
using MokomoGamesLib.Runtime.Ads.Configs;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Ads.AppOpens
{
    public class AppOpen
    {
        private AppOpenAd _ad;
        private bool _isShowingAd;
        public bool IsAdAvailable => _ad != null;
        private AdsConfigList _adsConfigList;

        public void LoadAd(AdsConfigList adsConfigList)
        {
            // Load an app open ad for portrait orientation
            AppOpenAd.LoadAd(
                adsConfigList.GetCurrentPlatformUnitId(AdsType.AppOpen),
                ScreenOrientation.Portrait,
                AdsManager.CreateAdMobRequest().Build(),
                (appOpenAd, error) =>
                {
                    if (error != null)
                    {
                        // Handle the error.
                        Debug.LogFormat("Failed to load the ad. (reason: {0})", error.LoadAdError.GetMessage());
                        return;
                    }

                    // App open ad is loaded.
                    _ad = appOpenAd;
                }
            );
        }

        public void ShowAdIfAvailable()
        {
            if (!IsAdAvailable || _isShowingAd) return;

            _ad.OnAdDidDismissFullScreenContent += (sender, args) =>
            {
                Debug.Log("Closed app open ad");
                // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
                _ad = null;
                _isShowingAd = false;
                LoadAd(_adsConfigList);
            };
            _ad.OnAdFailedToPresentFullScreenContent += (sender, args) =>
            {
                Debug.LogFormat("Failed to present the ad (reason: {0})", args.AdError.GetMessage());
                // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
                _ad = null;
                LoadAd(_adsConfigList);
            };
            _ad.OnAdDidPresentFullScreenContent += (sender, args) =>
            {
                Debug.Log("Displayed app open ad");
                _isShowingAd = true;
            };
            _ad.OnAdDidRecordImpression += (sender, args) => { Debug.Log("Recorded ad impression"); };
            _ad.OnPaidEvent += (sender, args) =>
            {
                Debug.LogFormat("Received paid event. (currency: {0}, value: {1}",
                    args.AdValue.CurrencyCode,
                    args.AdValue.Value);
            };
            _ad.Show();
        }
    }
}