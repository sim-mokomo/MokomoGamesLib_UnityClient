using System.Collections.Generic;
using MokomoGamesLib.Runtime.Localizations.MasterData;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Localizations
{
    public class Entity
    {
        private readonly Dictionary<string, string> _messageKVS;

        public Entity(Record record, Dictionary<string, string> messageKVS)
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