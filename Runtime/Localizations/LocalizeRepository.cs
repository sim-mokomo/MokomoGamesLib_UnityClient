using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MokomoGamesLib.Runtime.Localizations.MasterData;
using MokomoGamesLib.Runtime.Utilities;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Localizations
{
    public class LocalizeRepository : MonoBehaviour
    {
        [SerializeField] private List<Record> masterData = new();

        public void Load(AppLanguage language, Action<Table> onEnd)
        {
            PlayFabClientAPI.GetTitleData(
                new GetTitleDataRequest(),
                result =>
                {
                    var config = GetLocalizeConfig(language);
                    if (result.Data.TryGetValue(config.TableName, out var json))
                    {
                        var dic = JsonUtility.FromJson<SerializationDictionary<string, string>>(json).BuiltedDictionary;
                        onEnd?.Invoke(new Table(config, dic));
                    }
                },
                error => { });
        }

        public UniTask<Table> LoadAsync(AppLanguage language)
        {
            var source = new UniTaskCompletionSource<Table>();
            Load(
                language,
                entity => { source.TrySetResult(entity); });
            return source.Task;
        }

        private Record GetLocalizeConfig(AppLanguage language)
        {
            var config = masterData.FirstOrDefault(x => x.Language == language);
            return config == null ? masterData.FirstOrDefault(x => x.Language == AppLanguage.English) : config;
        }
    }
}