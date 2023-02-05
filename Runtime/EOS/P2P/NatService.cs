using System;
using Epic.OnlineServices.P2P;

namespace MokomoGamesLib.Runtime.EOS.P2P
{
    public class NatService
    {
        private readonly P2PInterface _p2PInterface;

        public NatService(P2PInterface p2PInterface)
        {
            _p2PInterface = p2PInterface;
        }

        public void RequestNATType(Action<NATType> onQueryNATTypeComplete)
        {
            var options = new QueryNATTypeOptions();
            _p2PInterface.QueryNATType(ref options, null,
                (ref OnQueryNATTypeCompleteInfo data) => { onQueryNATTypeComplete?.Invoke(data.NATType); });
        }
    }
}