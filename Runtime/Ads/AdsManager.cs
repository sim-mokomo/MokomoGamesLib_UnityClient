using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using MokomoGamesLib.Runtime.Ads.Configs;
using UnityEngine;
using Process = MokomoGamesLib.Runtime.Ads.Banners.Process;

namespace MokomoGamesLib.Runtime.Ads
{
    public class AdsManager : MonoBehaviour
    {
        private readonly List<AppOpens.Process> _showAppOpenAdProcessesList = new();
        private readonly List<Process> _showBannerProcesses = new();
        private AdsConfigList _adsConfigList = new AdsConfigList();

        public void Init(Action<InitializationStatus> initCompleteAction)
        {
            MobileAds.Initialize(initCompleteAction);
        }
        
        public static AdRequest CreateAdMobRequest()
        {
            return new AdRequest.Builder().Build();
        }

        public void AddAdsConfig(AdsConfig adsConfig)
        {
            _adsConfigList.Add(adsConfig);
        }

        public AppOpens.Process CreateAppOpenAdProcess()
        {
            var process = new AppOpens.Process();
            _showAppOpenAdProcessesList.Add(process);
            return process;
        }

        public Process CreateShowBannerProcess()
        {
            var process = new Process();
            _showBannerProcesses.Add(process);
            return process;
        }
    }
}