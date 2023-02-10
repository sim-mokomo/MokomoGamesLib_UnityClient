using System;
using System.Threading.Tasks;
using MokomoGamesLib.Runtime.Debugs.GameDebug;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Localization
{
    public enum AppLanguage
    {
        Arabic,
        Japanese,
        English,
        Korean,
        ChineseSimplified,
        ChineseTraditional
    }

    public class LocalizeManager : MonoBehaviour
    {
        [SerializeField] private LocalizeRepository localizeRepository;

#if UNITY_EDITOR
        private GameDebugSaveData _gameDebugSaveData;
#endif
        public LocalizeEntity LocalizeEntity { get; private set; }

        private void Awake()
        {
#if UNITY_EDITOR
            _gameDebugSaveData = Debugs.GameDebug.Service.LoadIfNotExistCreate();
            _gameDebugSaveData.OnChangedGameLanguage += async language =>
            {
                if (!isEndedLoading()) return;

                LocalizeEntity = await LoadAsync(language);
            };
#endif
        }

        public event Action OnChangedLanguage;

        public bool isEndedLoading()
        {
            return LocalizeEntity != null;
        }

        public async Task<LocalizeEntity> LoadAsync(SystemLanguage language)
        {
            return await LoadAsync(LocalizeEntity.ConvertSystemLanguage2AppLanguage(language));
        }

        public async Task<LocalizeEntity> LoadAsync(AppLanguage language)
        {
            var localizedEntity = await localizeRepository.LoadAsync(language);
            LocalizeEntity = localizedEntity;
            OnChangedLanguage?.Invoke();
            return localizedEntity;
        }

        public string GetLocalizedString(string textKey)
        {
            return LocalizeEntity == null ? string.Empty : LocalizeEntity.GetLocalizedString(textKey);
        }
    }
}