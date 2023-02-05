using MokomoGamesLib.Editor.Builds.Common;
using UnityEditor;

namespace MokomoGamesLib.Editor.Builds.Android
{
    public class OptionBuilder : Common.OptionBuilder
    {
        public OptionBuilder(Option option) : base(option.CommonOption)
        {
            EditorUserBuildSettings.buildAppBundle = true;
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keyaliasName = option.KeyStoreInfo.KeyAliasName;
            PlayerSettings.Android.keyaliasPass = option.KeyStoreInfo.KeyAliasPassword;
            PlayerSettings.Android.keystoreName = option.KeyStoreInfo.KeyPath;
            PlayerSettings.Android.keystorePass = option.KeyStoreInfo.KeyPassword;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            OnPostprocessBuildEvent += OnAndroidPostprocessBuildEvent;
        }

        private static void OnAndroidPostprocessBuildEvent(BuildTarget target, string pathToBuiltProject)
        {
            PlayerSettings.Android.bundleVersionCode += 1;
        }

        protected override string GetAppExtension()
        {
            return ".aab";
        }

        public class KeyStoreInfo
        {
            public string KeyAliasName;
            public string KeyAliasPassword;
            public string KeyPassword;
            public string KeyPath;
        }
    }
}