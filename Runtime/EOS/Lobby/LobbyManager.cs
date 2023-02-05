using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using PlayEveryWare.EpicOnlineServices;
using UnityEngine;

namespace MokomoGamesLib.Runtime.EOS.Lobby
{
    public class LobbyManager
    {
        private const string BucketId = "BattleLobby";
        private const string BucketIdAttributeKey = "BATTLE_LOBBY";
        private const string BucketIdAttributeValue = BucketId;
        private readonly NotifyEventHandle _addNotifyMemberStatusEventHandle;
        private readonly LobbyInterface _lobbyInterface;

        private Lobby _activeLobby;
        private List<Lobby> _currentLobbyList;

        public LobbyManager(LobbyInterface lobbyInterface)
        {
            _currentLobbyList = new List<Lobby>();
            _lobbyInterface = lobbyInterface;

            {
                var options = new AddNotifyLobbyMemberStatusReceivedOptions();
                _addNotifyMemberStatusEventHandle = new NotifyEventHandle(_lobbyInterface.AddNotifyLobbyMemberStatusReceived(
                        ref options,
                        null,
                        (ref LobbyMemberStatusReceivedCallbackInfo data) =>
                        {
                            Debug.Log($"参加メンバーのステータスが更新されました。 {data.CurrentStatus.ToString()} {data.LobbyId}");
                            if (data.CurrentStatus == LobbyMemberStatus.Joined)
                            {
                                Debug.Log($"参加メンバーのロビーに参加状態になりました。 {data.LobbyId} {data.TargetUserId}");
                                OnJoinedMember?.Invoke(data);
                            }
                        }),
                    handle => { _lobbyInterface.RemoveNotifyLobbyMemberStatusReceived(handle); }
                );
            }
        }

        public Lobby ActiveLobby
        {
            get => _activeLobby;
            set
            {
                _activeLobby = value;
                OnUpdatedActiveLobby?.Invoke();
            }
        }

        public bool IsJoiningLobby => ActiveLobby != null;
        public event Action<LobbyMemberStatusReceivedCallbackInfo> OnJoinedMember;
        public event Action OnUpdatedActiveLobby;
        public event Action<Result> OnLeaveLobbyError;

        public void OnApplicationQuit()
        {
            _addNotifyMemberStatusEventHandle.Dispose();

            foreach (var lobby in _currentLobbyList) lobby.Release();
        }

        public UniTask<CreateLobbyResponse> CreateLobbyAsync(ProductUserId localProductUserId, uint maxMemberNum)
        {
            var source = new UniTaskCompletionSource<CreateLobbyResponse>();
            CreateLobby(localProductUserId, maxMemberNum, result =>
            {
                var response = new CreateLobbyResponse
                {
                    Result = result
                };
                source.TrySetResult(response);
            });
            return source.Task;
        }

        public void CreateLobby(ProductUserId localProductUserId, uint maxMemberNum, Action<Result> onCreatedLobby)
        {
            var createLobbyOptions = new CreateLobbyOptions
            {
                LocalUserId = localProductUserId,
                MaxLobbyMembers = maxMemberNum,
                PermissionLevel = LobbyPermissionLevel.Publicadvertised,
                PresenceEnabled = false,
                AllowInvites = false,
                BucketId = BucketId,
                DisableHostMigration = true,
                EnableRTCRoom = false,
                LocalRTCOptions = null,
                EnableJoinById = false,
                RejoinAfterKickRequiresInvite = false
            };
            _lobbyInterface.CreateLobby(ref createLobbyOptions, null, (ref CreateLobbyCallbackInfo data) =>
            {
                if (data.ResultCode != Result.Success)
                {
                    Debug.LogError($"failed to create lobby {data.ResultCode}");
                    onCreatedLobby?.Invoke(data.ResultCode);
                    return;
                }

                LobbyModification modificationHandle = null;
                {
                    var options = new UpdateLobbyModificationOptions
                    {
                        LocalUserId = localProductUserId,
                        LobbyId = data.LobbyId
                    };
                    var result = _lobbyInterface.UpdateLobbyModification(ref options, out modificationHandle);
                    if (result != Result.Success)
                    {
                        Debug.LogError($"failed to create lobby modification handle {result}");
                        onCreatedLobby?.Invoke(result);
                        return;
                    }
                }

                {
                    var options = new LobbyModificationAddAttributeOptions
                    {
                        Attribute = new AttributeData
                        {
                            Key = BucketIdAttributeKey,
                            Value = new AttributeDataValue
                            {
                                AsUtf8 = BucketIdAttributeValue
                            }
                        },
                        Visibility = LobbyAttributeVisibility.Public
                    };
                    var result = modificationHandle.AddAttribute(ref options);
                    if (result != Result.Success)
                    {
                        Debug.LogError($"failed to modification lobby add attribute {result}");
                        onCreatedLobby?.Invoke(result);
                        return;
                    }
                }

                {
                    var options = new UpdateLobbyOptions
                    {
                        LobbyModificationHandle = modificationHandle
                    };
                    _lobbyInterface.UpdateLobby(ref options, null, (ref UpdateLobbyCallbackInfo info) =>
                    {
                        ActiveLobby = new Lobby(
                            new List<ProductUserId> { localProductUserId },
                            info.LobbyId
                        );
                        onCreatedLobby?.Invoke(info.ResultCode);
                    });
                }
            });
        }

        public UniTask<SearchLobbyResponse> SearchLobbyAsync(ProductUserId localProductUserId)
        {
            var source = new UniTaskCompletionSource<SearchLobbyResponse>();
            SearchLobby(localProductUserId, (result, list) =>
            {
                var response = new SearchLobbyResponse
                {
                    Result = result,
                    LobbyList = list
                };
                source.TrySetResult(response);
            });
            return source.Task;
        }

        public void SearchLobby(ProductUserId localProductUserId, Action<Result, List<Lobby>> onCompletedSearch)
        {
            var lobbySearchService = new SearchService(_lobbyInterface);
            lobbySearchService.SearchByBucketId(
                localProductUserId,
                BucketIdAttributeKey,
                BucketIdAttributeValue,
                (result, lobbyList) =>
                {
                    _currentLobbyList = lobbyList;
                    onCompletedSearch?.Invoke(result, lobbyList);
                }
            );
        }

        public UniTask<JoinLobbyResponse> JoinLobbyAsync(ProductUserId localProductUserId, Lobby lobby)
        {
            var source = new UniTaskCompletionSource<JoinLobbyResponse>();
            JoinLobby(localProductUserId, lobby, info =>
            {
                var response = new JoinLobbyResponse
                {
                    JoinLobbyInfo = info
                };
                source.TrySetResult(response);
            });
            return source.Task;
        }

        public void JoinLobby(ProductUserId localProductUserId, Lobby lobby, Action<JoinLobbyCallbackInfo> onJoined)
        {
            var options = new JoinLobbyOptions
            {
                LobbyDetailsHandle = lobby.LobbyDetails,
                LocalUserId = localProductUserId,
                PresenceEnabled = false,
                LocalRTCOptions = null
            };
            _lobbyInterface.JoinLobby(ref options, null, (ref JoinLobbyCallbackInfo data) =>
            {
                ActiveLobby = lobby;
                onJoined?.Invoke(data);
            });
        }

        public void LeaveLobby(ProductUserId localProductUserId, Lobby lobby)
        {
            if (ActiveLobby == null) return;
            ActiveLobby = null;
            var options = new LeaveLobbyOptions
            {
                LocalUserId = localProductUserId,
                LobbyId = lobby.Id
            };
            Debug.Log("LeaveLobbyが呼び出されました");
            _lobbyInterface.LeaveLobby(ref options, null, (ref LeaveLobbyCallbackInfo data) =>
            {
                if (data.ResultCode != Result.Success)
                {
                    if (data.ResultCode == Result.NotFound)
                    {
                        Debug.Log($"ロビーが見つかりませんでした {data.ResultCode}");
                        OnLeaveLobbyError?.Invoke(data.ResultCode);
                        return;
                    }

                    Debug.Log($"ロビーの退出に失敗した {data.ResultCode}");
                    return;
                }

                Debug.Log($"ロビーから退出しました {data.LobbyId}");
            });
        }

        public void DestroyLobbyAll(ProductUserId localProductUserId, Action<DestroyLobbyCallbackInfo> onDestroyed)
        {
            foreach (var lobby in _currentLobbyList)
            {
                var options = new DestroyLobbyOptions
                {
                    LocalUserId = localProductUserId,
                    LobbyId = lobby.Id
                };
                Debug.Log($"try destroy lobby id {lobby.Id}");
                _lobbyInterface.DestroyLobby(
                    ref options,
                    null,
                    (ref DestroyLobbyCallbackInfo data) =>
                    {
                        if (data.ResultCode == Result.NotFound)
                            Debug.Log($"failed to destroy lobby not found {data.LobbyId}");
                        else if (data.ResultCode != Result.Success) Debug.LogError($"failed to destroy from lobby {data.ResultCode}");
                        Debug.Log($"ロビーの破棄に成功しました {lobby.Id}");
                        onDestroyed?.Invoke(data);
                    });
            }

            ActiveLobby = null;
        }

        public Lobby FindLobbyById(string lobbyId)
        {
            return _currentLobbyList.FirstOrDefault(x => x.Id == lobbyId);
        }

        public class CreateLobbyResponse
        {
            public Result Result;
        }

        public class UpdateJoiningLobbyResponse
        {
            public Lobby ActiveLobby;
        }

        public class SearchLobbyResponse
        {
            public List<Lobby> LobbyList;
            public Result Result;
        }

        public class JoinLobbyResponse
        {
            public JoinLobbyCallbackInfo JoinLobbyInfo;
        }
    }
}