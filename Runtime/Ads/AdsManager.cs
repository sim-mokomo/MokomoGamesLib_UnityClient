using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using MokomoGamesLib.Runtime.Ads.AppOpen;
using MokomoGamesLib.Runtime.Ads.Banner;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Ads
{
    public class AdsManager : MonoBehaviour
    {
        private readonly List<ShowAppOpenAdProcess> _showAppOpenAdProcessesList = new();
        private readonly List<ShowBannerProcess> _showBannerProcesses = new();
        private AdsConfigList _adsConfigList = new AdsConfigList();

        public void Init(Action<InitializationStatus> initCompleteAction)
        {
            MobileAds.Initialize(initCompleteAction);
        }
        
        public static AdRequest.Builder CreateAdMobRequest()
        {
            var request = new AdRequest.Builder();
            return request;
        }

        public void AddAdsConfig(AdsConfig adsConfig)
        {
            _adsConfigList.Add(adsConfig);
        }

        public ShowAppOpenAdProcess CreateAppOpenAdProcess()
        {
            var process = new ShowAppOpenAdProcess();
            _showAppOpenAdProcessesList.Add(process);
            return process;
        }

        public ShowBannerProcess CreateShowBannerProcess()
        {
            var process = new ShowBannerProcess();
            _showBannerProcesses.Add(process);
            return process;
        }
    }
}