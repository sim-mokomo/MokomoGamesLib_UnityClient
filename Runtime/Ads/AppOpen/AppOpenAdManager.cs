using GoogleMobileAds.Api;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Ads.AppOpen
{
    public class AppOpenAdManager
    {
        private AppOpenAd ad;
        private bool isShowingAd;
        public bool IsAdAvailable => ad != null;
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
                    ad = appOpenAd;
                }
            );
        }

        public void ShowAdIfAvailable()
        {
            if (!IsAdAvailable || isShowingAd) return;

            ad.OnAdDidDismissFullScreenContent += (sender, args) =>
            {
                Debug.Log("Closed app open ad");
                // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
                ad = null;
                isShowingAd = false;
                LoadAd(_adsConfigList);
            };
            ad.OnAdFailedToPresentFullScreenContent += (sender, args) =>
            {
                Debug.LogFormat("Failed to present the ad (reason: {0})", args.AdError.GetMessage());
                // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
                ad = null;
                LoadAd(_adsConfigList);
            };
            ad.OnAdDidPresentFullScreenContent += (sender, args) =>
            {
                Debug.Log("Displayed app open ad");
                isShowingAd = true;
            };
            ad.OnAdDidRecordImpression += (sender, args) => { Debug.Log("Recorded ad impression"); };
            ad.OnPaidEvent += (sender, args) =>
            {
                Debug.LogFormat("Received paid event. (currency: {0}, value: {1}",
                    args.AdValue.CurrencyCode,
                    args.AdValue.Value);
            };
            ad.Show();
        }
    }
}