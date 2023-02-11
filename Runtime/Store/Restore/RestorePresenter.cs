using System;
using MokomoGamesLib.Runtime.UI.Animation;
using UnityEngine;
using UnityEngine.UI;

namespace MokomoGamesLib.Runtime.Store.Restore
{
    public class RestorePresenter : MonoBehaviour
    {
        [SerializeField] private Button restoreButton;
        [SerializeField] private DrawerAnimator drawerAnimator;
        [SerializeField] private float drawAnimDuration;

        private void Awake()
        {
            restoreButton.onClick.AddListener(() => { OnClickedRestoreButton?.Invoke(); });
        }

        public event Action OnClickedRestoreButton;

        public void Show(bool show)
        {
            if (show)
                drawerAnimator.OpenAnimation(drawAnimDuration);
            else
                drawerAnimator.CloseAnimation(drawAnimDuration);
        }
    }
}

