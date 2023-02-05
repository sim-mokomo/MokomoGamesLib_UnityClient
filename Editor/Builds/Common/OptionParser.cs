using MokomoGamesLib.Editor.CommandLine;
using UnityEditor;

namespace MokomoGamesLib.Editor.Builds.Common
{
    public class OptionParser
    {
        private static readonly string Prefix = "option";

        public static string CreateOptionKey(string key)
        {
            return $"{Prefix}:{key}";
        }

        public Option CuiCommandParse(string[] cuiArgs)
        {
            var option = new Option
            {
                BuildTarget = ArgsParser.GetEnumOption<BuildTarget>(CreateOptionKey("platform"), cuiArgs),
                Env = ArgsParser.GetEnumOption<Env>(CreateOptionKey("environment"), cuiArgs),
                BundleVersion = ArgsParser.GetStringOption(CreateOptionKey("version"), cuiArgs),
                ProductName = ArgsParser.GetStringOption(CreateOptionKey("productName"), cuiArgs),
                IsHeadlessMode = ArgsParser.GetBoolOption(CreateOptionKey("headlessMode"), cuiArgs),
                OutputPath = ArgsParser.GetStringOption(CreateOptionKey("output_path"), cuiArgs),
                AppId = ArgsParser.GetStringOption(CreateOptionKey("app_id"), cuiArgs)
            };
            return option;
        }
    }
}