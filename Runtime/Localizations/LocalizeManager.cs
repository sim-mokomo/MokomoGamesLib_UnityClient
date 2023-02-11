using System;
using System.Threading.Tasks;
using MokomoGamesLib.Runtime.Debugs.GameDebug;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Localizations
{
    public class LocalizeManager : MonoBehaviour
    {
        [SerializeField] private LocalizeRepository localizeRepository;

        public Entity CurrentEntity { get; private set; }

#if UNITY_EDITOR
        private GameDebugSaveData _gameDebugSaveData;
        private void Awake()
        {
            _gameDebugSaveData = Debugs.GameDebug.Service.LoadIfNotExistCreate();
            _gameDebugSaveData.OnChangedGameLanguage += async language =>
            {
                if (!IsEndedLoading()) return;

                CurrentEntity = await LoadAsync(language);
            };
        }
#endif

        public event Action OnChangedLanguage;

        public bool IsEndedLoading()
        {
            return CurrentEntity != null;
        }

        public async Task<Entity> LoadAsync(AppLanguage language)
        {
            CurrentEntity = await localizeRepository.LoadAsync(language);
            OnChangedLanguage?.Invoke();
            return CurrentEntity;
        }

        public string GetLocalizedString(string textKey) => CurrentEntity == null ? string.Empty : CurrentEntity.GetMessage(textKey);
    }
}