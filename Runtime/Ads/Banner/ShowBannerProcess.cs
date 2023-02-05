using GoogleMobileAds.Api;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Ads.Banner
{
    public class ShowBannerProcess
    {
        private AdsBanner _adsBanner;
        private AdsConfigList _adsConfigList;

        public void Start(AdsConfigList adsConfigList)
        {
            _adsConfigList = adsConfigList;
            _adsBanner = CreateBanner();
            // note: ロードが完了した時点でバナーが表示されるため、明示的に表示命令を出す必要がない。
            _adsBanner.Load();
        }

        private AdsBanner CreateBanner()
        {
            var bannerView = new BannerView(
                _adsConfigList.GetCurrentPlatformUnitId(AdsType.Banner),
                AdSize.SmartBanner,
                AdPosition.Bottom);
            return new AdsBanner(bannerView);
        }
    }
}