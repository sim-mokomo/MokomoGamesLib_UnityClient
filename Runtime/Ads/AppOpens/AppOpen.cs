using GoogleMobileAds.Api;
using MokomoGamesLib.Runtime.Ads.Configs;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Ads.AppOpens
{
    public class AppOpen
    {
        private AppOpenAd _ad;
        private AdsConfigList _adsConfigList;
        private bool _isShowingAd;
        public bool IsAdAvailable => _ad != null;

        public void LoadAd(AdsConfigList adsConfigList)
        {
            _adsConfigList = adsConfigList;
            AppOpenAd.Load(
                adsConfigList.GetCurrentPlatformUnitId(AdsType.AppOpen),
                ScreenOrientation.AutoRotation,
                AdsManager.CreateAdMobRequest().Build(),
                (ad, error) =>
                {
                    if (error != null)
                    {
                        Debug.Log($"Failed to load the ad. (reason: {error.GetMessage()})");
                        return;
                    }
                    
                    _ad = ad;
                }
            );
        }

        public void ShowAdIfAvailable()
        {
            if (!IsAdAvailable || _isShowingAd) return;

            _ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("OpenAdを閉じる");
                _ad = null;
                _isShowingAd = false;
                LoadAd(_adsConfigList);
            };
            _ad.OnAdFullScreenContentFailed += error =>
            {
                Debug.Log($"OpenAdの表示に失敗 (reason: {error.GetMessage()})");
                _ad = null;
                _isShowingAd = false;
                LoadAd(_adsConfigList);
            };
            _ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("OpenAdの表示に成功");
                _isShowingAd = true;
            };
            _ad.Show();
        }
    }
}