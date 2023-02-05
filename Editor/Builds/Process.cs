using System;
using MokomoGamesLib.Editor.Builds.Common;
using MokomoGamesLib.Editor.CommandLine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MokomoGamesLib.Editor.Builds
{
    public class Process : MonoBehaviour
    {
        public static void BuildFromCui()
        {
            var args = Environment.GetCommandLineArgs();
            var platform = ArgsParser.GetEnumOption<BuildTarget>(OptionParser.CreateOptionKey("platform"), args);
            OptionBuilder optionBuilder = platform switch
            {
                BuildTarget.Android => new Android.OptionBuilder(Android.OptionParser.CuiCommandParse(args)),
                BuildTarget.iOS => new iOS.OptionBuilder(iOS.OptionParser.CuiCommandParse(args)),
                BuildTarget.StandaloneLinux64 => new Linux.OptionBuilder(new Linux.OptionParser().Parse(args)),
                _ => null
            };

            if (optionBuilder == null) throw new Exception($"#{platform}がビルドサポートされていません。");

            var result = BuildPipeline.BuildPlayer(optionBuilder.Build());
            var success = result.summary.result == BuildResult.Succeeded;
            if (!success)
            {
                Debug.Log("[DEBUG] failed to build");
                throw new Exception();
            }
        }
    }
}