using UnityEngine;

namespace MokomoGamesLib.Runtime.Loading
{
    public class WheelLoadingPresenter : MonoBehaviour
    {
        [SerializeField] private LoadingIcon loadingIcon;

        public void Tick(float deltaTime)
        {
            loadingIcon.Tick(deltaTime);
        }

        public void Show(bool show)
        {
            gameObject.SetActive(show);
        }
    }
}