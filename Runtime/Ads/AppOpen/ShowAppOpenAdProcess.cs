using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Ads.AppOpen
{
    public class ShowAppOpenAdProcess
    {
        private AppOpenAdManager _appOpenAdManager;

        public async UniTask Start(AdsConfigList adsConfigList)
        {
            _appOpenAdManager = new AppOpenAdManager();
            _appOpenAdManager.LoadAd(adsConfigList);
            AppStateEventNotifier.AppStateChanged += AppStateChanged;

            await UniTask.WaitUntil(() => _appOpenAdManager.IsAdAvailable);
            _appOpenAdManager.ShowAdIfAvailable();
        }

        private void AppStateChanged(AppState appState)
        {
            Debug.Log("App State is " + appState);
            if (appState == AppState.Foreground) _appOpenAdManager.ShowAdIfAvailable();
        }
    }
}