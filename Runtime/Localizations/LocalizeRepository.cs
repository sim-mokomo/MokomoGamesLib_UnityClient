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

        private void Load(AppLanguage language, Action<Entity> onEnd)
        {
            PlayFabClientAPI.GetTitleData(
                new GetTitleDataRequest(),
                result =>
                {
                    var config = GetMasterDataRecord(language);
                    if (!result.Data.TryGetValue(config.TableName, out var json)) return;

                    var dic = JsonUtility.FromJson<SerializationDictionary<string, string>>(json).BuiltedDictionary;
                    onEnd?.Invoke(new Entity(config, dic));
                },
                error =>
                {
                    Debug.LogError($"PlayFabからローカライズタイトルデータを取得する事に失敗した。Reason:{error.ErrorMessage}");
                });
        }

        public UniTask<Entity> LoadAsync(AppLanguage language)
        {
            var source = new UniTaskCompletionSource<Entity>();
            Load(language, entity => { source.TrySetResult(entity); });
            return source.Task;
        }

        private Record GetMasterDataRecord(AppLanguage language)
        {
            var record = masterData.FirstOrDefault(x => x.Language == language);
            if (record != null) return record;
            
            Debug.LogError($"{language}がマスターデータに登録されていません");
            return masterData.FirstOrDefault(x => x.Language == AppLanguage.English);
        }
    }
}