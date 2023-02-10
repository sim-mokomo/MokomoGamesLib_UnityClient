using System.Collections.Generic;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Localizations.MasterData
{
    public class Table
    {
        private readonly Dictionary<string, string> _messageKVS;

        public Table(Record record, Dictionary<string, string> messageKVS)
        {
            _messageKVS = messageKVS;
            Record = record;
        }

        public Record Record { get; }

        public string GetMessage(string key)
        {
            if (_messageKVS.TryGetValue(key, out var message)) return message;

            Debug.LogError($"{Record.Language.ToString()} には {key} は登録されていません");
            return string.Empty;
        }

        public static AppLanguage ConvertSystemLanguage2AppLanguage(SystemLanguage systemLanguage)
        {
            return systemLanguage switch
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