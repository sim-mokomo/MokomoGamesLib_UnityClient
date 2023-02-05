using System;
using UnityEngine;
using UnityEngine.UI;

namespace MokomoGamesLib.Runtime.Audio
{
    public class MuteAudioButtonView : MonoBehaviour
    {
        [SerializeField] private Button muteButton;
        [SerializeField] private Image soundIcon;
        [SerializeField] private Sprite muteSprite;
        [SerializeField] private Sprite unMuteSprite;

        private void Awake()
        {
            muteButton.onClick.AddListener(() => { OnToggleMute?.Invoke(); });
        }

        public event Action OnToggleMute;

        public void UpdateRender(bool isMute)
        {
            soundIcon.sprite = isMute ? muteSprite : unMuteSprite;
        }
    }
}