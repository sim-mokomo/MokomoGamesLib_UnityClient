using System;
using UnityEngine;
using UnityEngine.UI;

namespace MokomoGamesLib.Runtime.Audio.UI
{
    public class MuteButtonView : MonoBehaviour
    {
        [SerializeField] private Button muteButton;
        [SerializeField] private Image soundIcon;
        [SerializeField] private Sprite muteSprite;
        [SerializeField] private Sprite unMuteSprite;

        private void Awake()
        {
            muteButton.onClick.AddListener(() => { OnClickedMuteButton?.Invoke(); });
        }

        public event Action OnClickedMuteButton;

        public void UpdateIcon(bool isMute)
        {
            soundIcon.sprite = isMute ? muteSprite : unMuteSprite;
        }
    }
}