using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using MokomoGamesLib.Runtime.Ads.Configs;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Ads.AppOpens
{
    public class Process
    {
        private AppOpen _appOpen;

        public async UniTask Start(AdsConfigList adsConfigList)
        {
            _appOpen = new AppOpen();
            _appOpen.LoadAd(adsConfigList);
            AppStateEventNotifier.AppStateChanged += AppStateChanged;

            await UniTask.WaitUntil(() => _appOpen.IsAdAvailable);
            _appOpen.ShowAdIfAvailable();
        }

        private void AppStateChanged(AppState appState)
        {
            Debug.Log($"App State が次のStateに変化しました: {appState}");
            if (appState == AppState.Foreground) _appOpen.ShowAdIfAvailable();
        }
    }
}