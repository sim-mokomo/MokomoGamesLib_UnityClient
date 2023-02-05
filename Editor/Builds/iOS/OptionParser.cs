using MokomoGamesLib.Editor.CommandLine;

namespace MokomoGamesLib.Editor.Builds.iOS
{
    public static class OptionParser
    {
        public static Option CuiCommandParse(string[] cuiArgs)
        {
            var option = new Option
            {
                CommonOption = new MokomoGamesLib.Editor.Builds.Common.OptionParser().CuiCommandParse(cuiArgs),
                ProvisioningProfileName = ArgsParser.GetStringOption(Common.OptionParser.CreateOptionKey("provisioningProfileName"), cuiArgs),
                TeamId = ArgsParser.GetStringOption(Common.OptionParser.CreateOptionKey("teamId"), cuiArgs),
                AdmobApplicationIdentify = ArgsParser.GetStringOption(Common.OptionParser.CreateOptionKey("admob_application_identify"), cuiArgs)
            };
            return option;
        }
    }
}