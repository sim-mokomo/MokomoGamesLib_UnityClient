using GoogleMobileAds.Api;
using MokomoGamesLib.Runtime.Ads.Configs;

namespace MokomoGamesLib.Runtime.Ads.Banners
{
    public class Process
    {
        private Banner _banner;
        private AdsConfigList _adsConfigList;

        public void Start(AdsConfigList adsConfigList)
        {
            _adsConfigList = adsConfigList;
            _banner = CreateBanner();
            // note: ロードが完了した時点でバナーが表示されるため、明示的に表示命令を出す必要がない。
            _banner.Load();
        }

        private Banner CreateBanner()
        {
            var bannerView = new BannerView(
                _adsConfigList.GetCurrentPlatformUnitId(AdsType.Banner),
                AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth),
                AdPosition.Bottom
            );
            return new Banner(bannerView);
        }
    }
}