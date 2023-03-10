using Cysharp.Threading.Tasks;
using Google.Protobuf;
using MokomoGamesLib.Runtime.Loadings;
using MokomoGamesLib.Runtime.Loadings.UI;
using MokomoGamesLib.Runtime.UI;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Network
{
    public class ApiRequestProcess
    {
        private WheelLoadingPresenter _loadingPresenter;

        public async UniTask<TResponse> Request<TRequest, TResponse>
            (ApiRequestRunner.ApiName apiName, TRequest request)
            where TRequest : IMessage<TRequest>
            where TResponse : IMessage<TResponse>, new()
        {
            var uiManager = Object.FindObjectOfType<UIManager>();
            var apiRequestRunner = new ApiRequestRunner();
            _loadingPresenter = uiManager.Create<WheelLoadingPresenter>(UIManager.CanvasOrder.Front);
            _loadingPresenter.Show(true);
            var response = await apiRequestRunner.Request<TRequest, TResponse>(apiName, request);
            uiManager.Destroy(_loadingPresenter);
            return response;
        }

        public void Tick(float deltaTime)
        {
            if (_loadingPresenter) _loadingPresenter.Tick(deltaTime);
        }
    }
}