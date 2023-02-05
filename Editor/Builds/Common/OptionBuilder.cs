using System;
using System.IO;
using System.Linq;
using MokomoGamesLib.Editor.DefineSymbol;
using UnityEditor;
using UnityEditor.Callbacks;

namespace MokomoGamesLib.Editor.Builds.Common
{
    public abstract class OptionBuilder
    {
        protected readonly Option _option;

        protected OptionBuilder(Option option)
        {
            _option = option;
        }

        public static event Action<BuildTarget, string> OnPostprocessBuildEvent;

        protected abstract string GetAppExtension();

        public virtual BuildPlayerOptions Build()
        {
            var option = new BuildPlayerOptions
            {
                locationPathName = Path.Join(_option.OutputPath, $"game{GetAppExtension()}"),
                target = _option.BuildTarget,
                scenes = EditorBuildSettings.scenes.Select(x => x.path).ToArray(),
                options = _option.CalcCurrentBuildOptions()
            };

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS | BuildTargetGroup.Android, _option.AppId);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS | BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetIncrementalIl2CppBuild(BuildTargetGroup.iOS | BuildTargetGroup.Android, true);
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android | BuildTargetGroup.iOS,
                _option.CalcCurrentIl2CppCompilerConfiguration());
            PlayerSettings.bundleVersion = _option.BundleVersion;
            PlayerSettings.productName = _option.ProductName;
            PlayerSettings.companyName = "MokomoGames";
            EditorUserBuildSettings.il2CppCodeGeneration = _option.CalcCurrentIl2CppCodeGeneration();

            // NOTE: Dev環境向けのビルドの場合、DEVシンボルを有効にし、デバッグを有効化させるように
            var defineSymbolSettingList = new DefineSymbolService().LoadDefineSymbol();
            const string devDefineSymbol = "DEV";
            if (_option.IsDevelopment)
                defineSymbolSettingList.AddSymbol(devDefineSymbol);
            else
                defineSymbolSettingList.RemoveSymbol(devDefineSymbol);
            UnityDefineSymbolRepository.SaveSymbols(defineSymbolSettingList);

            return option;
        }

        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            OnPostprocessBuildEvent?.Invoke(target, pathToBuiltProject);
            OnPostprocessBuildEvent = null;
        }
    }
}