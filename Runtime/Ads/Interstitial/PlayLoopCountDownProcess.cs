using GoogleMobileAds.Api;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Ads.Interstitial
{
    public class PlayLoopCountDownProcess
    {
        private readonly int _adsIntervalMax;
        private readonly int _adsIntervalMin;
        private AdsInterstitial _ads;
        private Counter.Counter _counter;
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
            _ads = new AdsInterstitial(new InterstitialAd(adsConfigList.GetCurrentPlatformUnitId(AdsType.Interstitial)));
            _ads!.OnAdClosed += args => { BuildInterstitialAd(_adsConfigList); };

            _counter = new Counter.Counter(0, Random.Range(_adsIntervalMin, _adsIntervalMax));
            _counter.OnEnd += () => { _ads.Show(); };
        }
    }
}