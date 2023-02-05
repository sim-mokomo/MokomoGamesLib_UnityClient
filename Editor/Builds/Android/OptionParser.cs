using MokomoGamesLib.Editor.CommandLine;

namespace MokomoGamesLib.Editor.Builds.Android
{
    public static class OptionParser
    {
        public static Option CuiCommandParse(string[] cuiArgs)
        {
            var option = new Option
            {
                CommonOption = new MokomoGamesLib.Editor.Builds.Common.OptionParser().CuiCommandParse(cuiArgs),
                KeyStoreInfo = new OptionBuilder.KeyStoreInfo
                {
                    KeyAliasName = ArgsParser.GetStringOption(Common.OptionParser.CreateOptionKey("key_alias_name"), cuiArgs),
                    KeyAliasPassword = ArgsParser.GetStringOption(Common.OptionParser.CreateOptionKey("key_alias_password"), cuiArgs),
                    KeyPassword = ArgsParser.GetStringOption(Common.OptionParser.CreateOptionKey("key_password"), cuiArgs),
                    KeyPath = ArgsParser.GetStringOption(Common.OptionParser.CreateOptionKey("key_path"), cuiArgs)
                },
            };
            return option;
        }
    }
}