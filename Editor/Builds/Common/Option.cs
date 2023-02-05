using UnityEditor;
using UnityEditor.Build;

namespace MokomoGamesLib.Editor.Builds.Common
{
    public class Option
    {
        public string AppId;
        public BuildTarget BuildTarget;
        public string BundleVersion;
        public Env Env;
        public bool IsHeadlessMode;
        public string OutputPath;
        public string ProductName;
        public bool ShouldRaiseError;
        public bool IsDevelopment => Env == Env.Development;

        public Il2CppCodeGeneration CalcCurrentIl2CppCodeGeneration()
        {
            return IsDevelopment ? Il2CppCodeGeneration.OptimizeSize : Il2CppCodeGeneration.OptimizeSpeed;
        }

        public Il2CppCompilerConfiguration CalcCurrentIl2CppCompilerConfiguration()
        {
            return IsDevelopment ? Il2CppCompilerConfiguration.Debug : Il2CppCompilerConfiguration.Release;
        }

        public BuildOptions CalcCurrentBuildOptions()
        {
            return IsDevelopment ? BuildOptions.Development | BuildOptions.AllowDebugging : BuildOptions.None;
        }
    }
}