using TMPro;
using UnityEngine;

namespace MokomoGamesLib.Runtime.EOS
{
    public class EOSDebugPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI natTypeText;
        [SerializeField] private TextMeshProUGUI localProductUserIdText;
        [SerializeField] private TextMeshProUGUI activateLobbyIdText;

        public void SetNatType(string natType)
        {
            natTypeText.text = $"NAT Type: {natType}";
        }

        public void SetLocalProductUserId(string localProductUserId)
        {
            localProductUserIdText.text = $"Local Id: {localProductUserId}";
        }

        public void SetActivateLobbyIdText(string lobbyId)
        {
            activateLobbyIdText.text = $"Lobby Id: {lobbyId}";
        }
    }
}