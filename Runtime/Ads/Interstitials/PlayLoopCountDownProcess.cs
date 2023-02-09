using GoogleMobileAds.Api;
using MokomoGamesLib.Runtime.Ads.Configs;
using MokomoGamesLib.Runtime.Counters;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Ads.Interstitials
{
    public class PlayLoopCountDownProcess
    {
        private readonly int _adsIntervalMax;
        private readonly int _adsIntervalMin;
        private Interstitial _ads;
        private Counter _counter;
        private readonly AdsConfigList _adsConfigList;

        public PlayLoopCountDownProcess(int adsIntervalMin, int adsIntervalMax, AdsConfigList adsConfigList)
        {
            _adsIntervalMin = adsIntervalMin;
            _adsIntervalMax = adsIntervalMax;
            _adsConfigList = adsConfigList;

            BuildInterstitialAd(_adsConfigList);
        }

        public void IncreaseCount()
        {
            _counter?.Increase(1);
        }

        private void BuildInterstitialAd(AdsConfigList adsConfigList)
        {
            InterstitialAd.Load(
                _adsConfigList.GetCurrentPlatformUnitId(AdsType.Interstitial),
                AdsManager.CreateAdMobRequest().Build(),
                (ad, error) =>
                {
                    _ads = new Interstitial(ad);
                    _ads.OnAdClosed += () =>
                    {
                        _ads.Destroy();
                        BuildInterstitialAd(_adsConfigList);
                    };
                }
            );

            _counter = new Counter(0, Random.Range(_adsIntervalMin, _adsIntervalMax));
            _counter.OnEnd += () => { _ads.Show(); };
        }
    }
}