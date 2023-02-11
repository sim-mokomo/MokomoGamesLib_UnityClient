using System;
using MokomoGamesLib.Runtime.UI.Animation;
using UnityEngine;
using UnityEngine.UI;

namespace MokomoGamesLib.Runtime.Store
{
    public class StoreItemButtonView : MonoBehaviour
    {
        [SerializeField] private Button purchaseButton;
        [SerializeField] private ScaleAnimator scaleAnimator;
        public event Action OnClickedPurchaseButton;

        private void Awake()
        {
            purchaseButton.onClick.AddListener(() =>
            {
                OnClickedPurchaseButton?.Invoke();
            });
        }

        public void Show(bool show,float duration)
        {
            scaleAnimator.PlayShowAnimation(show, duration);
        }
    }
}

