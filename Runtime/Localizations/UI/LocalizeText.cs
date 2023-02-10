using TMPro;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Localizations.UI
{
    public class LocalizeText : MonoBehaviour
    {
        [SerializeField] private string textKey;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private bool isBitMapFont;
        private LocalizeManager _localizeManager;

        public void Awake()
        {
            _localizeManager = FindObjectOfType<LocalizeManager>();
            _localizeManager.OnChangedLanguage += UpdateSelfAll;
        }

        private void OnEnable()
        {
            UpdateSelfAll();
        }

        private void UpdateSelfAll()
        {
            if (string.IsNullOrEmpty(textKey))
                SetTextDirectly(text.text);
            else
                SetTextFromKey(textKey);
        }

        public void SetTextFromKey(string key)
        {
            textKey = key;
            if (!_localizeManager.IsEndedLoading()) return;

            text.text = string.IsNullOrEmpty(key) ? "" : _localizeManager.GetLocalizedString(textKey);
            UpdateSelfFont();
        }

        public void SetTextDirectly(string content)
        {
            textKey = string.Empty;
            if (!_localizeManager.IsEndedLoading()) return;
            text.text = content;
            UpdateSelfFont();
        }

        private void UpdateSelfFont()
        {
            text.font = Resources.Load<TMP_FontAsset>(_localizeManager.CurrentEntity.Record.GetFontPath(isBitMapFont));
            text.UpdateFontAsset();
        }
    }
}