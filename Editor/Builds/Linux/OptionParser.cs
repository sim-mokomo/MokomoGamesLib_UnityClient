using MokomoGamesLib.Editor.CommandLine;

namespace MokomoGamesLib.Editor.Builds.Linux
{
    public class OptionParser
    {
        public Option Parse(string[] args)
        {
            var option = new Option
            {
                CommonOption = new MokomoGamesLib.Editor.Builds.Common.OptionParser().CuiCommandParse(args),
                IsHeadlessMode = ArgsParser.GetBoolOption(Common.OptionParser.CreateOptionKey("headlessMode"), args)
            };
            return option;
        }
    }
}