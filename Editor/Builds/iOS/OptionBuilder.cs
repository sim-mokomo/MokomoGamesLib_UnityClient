using UnityEditor;
#if UNITY_IOS || UNITY_EDITOR_OSX
#define IS_IOS_PLATFORM
#endif

#if IS_IOS_PLATFORM
using System;
using System.IO;
using UnityEditor.iOS.Xcode;
using UnityEngine;
#endif

namespace MokomoGamesLib.Editor.Builds.iOS
{
    public class OptionBuilder : Common.OptionBuilder
    {
        private static Option _option;

        public OptionBuilder(Option option) : base(option.CommonOption)
        {
            PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            PlayerSettings.iOS.appleDeveloperTeamID = option.TeamId;
            PlayerSettings.iOS.iOSManualProvisioningProfileType = option.GetProvisioningProfileType();
            PlayerSettings.iOS.iOSManualProvisioningProfileID = option.ProvisioningProfileName;
            _option = option;
            OnPostprocessBuildEvent += OnIOSPostprocessBuildEvent;
        }

        private static int BuildNumber => int.Parse(PlayerSettings.iOS.buildNumber);

        private static void OnIOSPostprocessBuildEvent(BuildTarget target, string pathToBuiltProject)
        {
#if IS_IOS_PLATFORM
            static void UpdatePBXProjectProcess(string pathToBuiltProject)
            {
                var projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
                var content = File.ReadAllText(projectPath);
                content = content.Replace("ENABLE_BITCODE = YES", "ENABLE_BITCODE = NO");
                File.WriteAllText(projectPath, content);
            }
            
            static void UpdatePlistProcess(string pathToBuiltProject, Action<PlistDocument> process)
            {
                var plistPath = Path.Join(pathToBuiltProject, "Info.plist");
                var plist = new PlistDocument();
                plist.ReadFromFile(plistPath);
                process(plist);
                plist.WriteToFile(plistPath);
            }

            if (!string.IsNullOrEmpty(_option.AdmobApplicationIdentify))
            {
                UpdatePlistProcess(pathToBuiltProject, plist =>
                {
                    // NOTE: 広告向けのIDを登録しておく必要がある
                    plist.root.SetString("GADApplicationIdentifier", _option.AdmobApplicationIdentify);
                });
            }

            PlayerSettings.iOS.buildNumber = (BuildNumber + 1).ToString();
            UpdatePBXProjectProcess(pathToBuiltProject);
#endif
        }

        protected override string GetAppExtension()
        {
            return string.Empty;
        }
    }
}