using Mirror;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Network.Mirror
{
    public class MirrorObjectNameProvider : MonoBehaviour
    {
        private NetworkIdentity _networkIdentity;
        private bool _provided;

        private void Awake()
        {
            _networkIdentity = GetComponent<NetworkIdentity>();
        }

        public void Update()
        {
            if (!NetworkServer.active) return;

            if (_provided) return;

            if (_networkIdentity.netId == 0) return;

            name = $"{gameObject.name} [conn id]={_networkIdentity.connectionToClient.connectionId}";
            _provided = true;
        }
    }
}