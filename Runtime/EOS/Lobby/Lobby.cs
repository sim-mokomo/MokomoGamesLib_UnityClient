using System.Collections.Generic;
using System.Linq;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using UnityEngine;

namespace MokomoGamesLib.Runtime.EOS.Lobby
{
    public class Lobby
    {
        private ProductUserId _ownerProductUserId;

        public Lobby(List<ProductUserId> joinedProductUserList, string lobbyId)
        {
            JoinedUserList = joinedProductUserList;
            Id = lobbyId;
            IsSetupFromHandle = false;
        }

        public Lobby(LobbyDetails lobbyDetails)
        {
            JoinedUserList = new List<ProductUserId>();

            LobbyDetails = lobbyDetails;
            UpdatePropertiesFromDetails(LobbyDetails);
        }

        public List<ProductUserId> JoinedUserList { get; }

        public string Id { get; private set; }
        public uint MaxMemberNum { get; private set; }
        public uint AvailableMemberNum { get; private set; }
        public LobbyDetails LobbyDetails { get; }
        public bool IsSetupFromHandle { get; private set; }
        public bool IsFullCapacity => AvailableMemberNum == 0;

        private void UpdatePropertiesFromDetails(LobbyDetails lobbyDetails)
        {
            IsSetupFromHandle = true;
            {
                var options = new LobbyDetailsCopyInfoOptions();
                var result = lobbyDetails.CopyInfo(ref options, out var detailsInfo);
                if (result != Result.Success)
                {
                    Debug.LogError($"failed to copy lobby info #{result}");
                    return;
                }

                if (detailsInfo != null)
                {
                    var lobbyDetailsInfo = detailsInfo.Value;

                    Id = lobbyDetailsInfo.LobbyId;
                    MaxMemberNum = lobbyDetailsInfo.MaxMembers;
                    AvailableMemberNum = lobbyDetailsInfo.AvailableSlots;
                    _ownerProductUserId = lobbyDetailsInfo.LobbyOwnerUserId;
                }
            }

            {
                var getMemberCountOptions = new LobbyDetailsGetMemberCountOptions();
                var memberCount = lobbyDetails.GetMemberCount(ref getMemberCountOptions);
                for (uint i = 0; i < memberCount; i++)
                {
                    var getMemberOptions = new LobbyDetailsGetMemberByIndexOptions
                    {
                        MemberIndex = i
                    };
                    var productUserId = lobbyDetails.GetMemberByIndex(ref getMemberOptions);
                    JoinedUserList.Add(productUserId);
                }
            }
        }

        public bool IsOwner(ProductUserId localProductUserId)
        {
            return _ownerProductUserId == localProductUserId;
        }

        public ProductUserId FindOtherProductUserId(ProductUserId localProductUserId)
        {
            return JoinedUserList.FirstOrDefault(x => x != localProductUserId);
        }

        public void Release()
        {
            if (LobbyDetails != null) LobbyDetails.Release();
        }
    }
}