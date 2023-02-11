using MokomoGamesLib.Runtime.Localizations;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Extensions
{
    public static class SystemLanguageExtensions
    {
        public static AppLanguage ConvertToAppLanguage(this SystemLanguage self)
        {
            return self switch
            {
                SystemLanguage.Arabic => AppLanguage.Arabic,
                SystemLanguage.Korean => AppLanguage.Korean,
                SystemLanguage.ChineseSimplified => AppLanguage.ChineseSimplified,
                SystemLanguage.ChineseTraditional => AppLanguage.ChineseTraditional,
                SystemLanguage.Japanese => AppLanguage.Japanese,
                SystemLanguage.English => AppLanguage.English,
                _ => AppLanguage.English
            };
        }
    }
}