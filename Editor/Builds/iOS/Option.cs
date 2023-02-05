using UnityEditor;

namespace MokomoGamesLib.Editor.Builds.iOS
{
    public class Option
    {
        public string AdmobApplicationIdentify;
        public Common.Option CommonOption;
        public string ProvisioningProfileName;
        public string TeamId;

        public ProvisioningProfileType GetProvisioningProfileType()
        {
            // NOTE: 端末にインストールしたいので、ステージング環境でもProfile自体はDevのものを使用する。
            if (CommonOption.IsDevelopment || CommonOption.Env == Env.Staging) return ProvisioningProfileType.Development;

            if (CommonOption.Env == Env.Production) return ProvisioningProfileType.Distribution;

            return ProvisioningProfileType.Automatic;
        }
    }
}