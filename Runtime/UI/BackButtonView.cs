using System;
using UnityEngine;
using UnityEngine.UI;

namespace MokomoGamesLib.Runtime.UI
{
    public class BackButtonView : MonoBehaviour
    {
        [SerializeField] private Button backButton;

        private void Start()
        {
            backButton.onClick.AddListener(() => { OnClickedButton?.Invoke(); });
        }

        public event Action OnClickedButton;
    }
}