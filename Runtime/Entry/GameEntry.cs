using MokomoGamesLib.Runtime.Network;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Entry
{
    public class GameEntry : MonoBehaviour
    {
        private ApiRequestProcess _apiRequestProcess;

        protected virtual async void Start()
        {
            // var adsManager = FindObjectOfType<AdsManager>();
            // adsManager.Init(_ => { });
            // await adsManager.CreateAppOpenAdProcess().Start();
            // adsManager.CreateShowBannerProcess().Start();
            _apiRequestProcess = new ApiRequestProcess();
        }

        protected virtual void Update()
        {
            _apiRequestProcess.Tick(Time.deltaTime);
        }
    }
}