using System;
using System.Threading.Tasks;
using MokomoGamesLib.Runtime.Debugs.GameDebug;
using MokomoGamesLib.Runtime.Localizations.MasterData;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Localizations
{
    public class LocalizeManager : MonoBehaviour
    {
        [SerializeField] private LocalizeRepository localizeRepository;

#if UNITY_EDITOR
        private GameDebugSaveData _gameDebugSaveData;
#endif
        public Table Table { get; private set; }

        private void Awake()
        {
#if UNITY_EDITOR
            _gameDebugSaveData = Debugs.GameDebug.Service.LoadIfNotExistCreate();
            _gameDebugSaveData.OnChangedGameLanguage += async language =>
            {
                if (!isEndedLoading()) return;

                Table = await LoadAsync(language);
            };
#endif
        }

        public event Action OnChangedLanguage;

        public bool isEndedLoading()
        {
            return Table != null;
        }

        public async Task<Table> LoadAsync(SystemLanguage language)
        {
            return await LoadAsync(Table.ConvertSystemLanguage2AppLanguage(language));
        }

        public async Task<Table> LoadAsync(AppLanguage language)
        {
            var localizedEntity = await localizeRepository.LoadAsync(language);
            Table = localizedEntity;
            OnChangedLanguage?.Invoke();
            return localizedEntity;
        }

        public string GetLocalizedString(string textKey)
        {
            return Table == null ? string.Empty : Table.GetMessage(textKey);
        }
    }
}