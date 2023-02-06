using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Ads.Configs
{
    public class AdsConfigList
    {
        private readonly List<AdsConfig> _adsConfigList = new List<AdsConfig>();

        public void Add(AdsConfig adsConfig)
        {
            _adsConfigList.Add(adsConfig);
        }
        
        public string GetCurrentPlatformUnitId(AdsType adsType)
        {
            return FindUnitIDByPlatform(CalcCurrentAdsPlatform(), adsType);
        }
        
        private RuntimePlatform CalcCurrentAdsPlatform()
        {
            var platform = Application.platform;
            if (Application.isEditor)
                return platform == RuntimePlatform.WindowsEditor || platform == RuntimePlatform.LinuxEditor
                    ? RuntimePlatform.Android
                    : RuntimePlatform.IPhonePlayer;

            return platform;
        }
        
        public string FindUnitIDByPlatform(RuntimePlatform platform, AdsType adsType)
        {
            var record = _adsConfigList
                .First(x => x.RuntimePlatform == platform).UnitIdTable
                .First(x => x.AdsType == adsType);
            if (record.UnitID == string.Empty) Debug.LogError($"{platform} {adsType} の広告が設定されていません");
            var isTest = Application.isEditor || Debug.isDebugBuild;
            return isTest ? record.UnitTestId : record.UnitID;
        }
    }
}