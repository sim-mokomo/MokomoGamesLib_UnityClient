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
    }
}