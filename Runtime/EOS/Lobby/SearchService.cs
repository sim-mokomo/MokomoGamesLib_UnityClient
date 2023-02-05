using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using UnityEngine;

namespace MokomoGamesLib.Runtime.EOS.Lobby
{
    public class SearchService
    {
        private readonly LobbyInterface _lobbyInterface;

        public SearchService(LobbyInterface lobbyInterface)
        {
            _lobbyInterface = lobbyInterface;
        }

        public void SearchByBucketId(
            ProductUserId localProductUserId,
            string bucketIdKey,
            string bucketIdValue,
            Action<Result, List<Lobby>> onCompletedSearch
        )
        {
            var result = Result.UnexpectedError;
            LobbySearch searchHandle = null;

            {
                var options = new CreateLobbySearchOptions
                {
                    MaxResults = 50
                };

                result = _lobbyInterface.CreateLobbySearch(ref options, out searchHandle);
                if (result != Result.Success)
                {
                    Debug.LogError($"failed to create lobby search {result}");
                    onCompletedSearch?.Invoke(result, new List<Lobby>());
                    return;
                }
            }

            {
                var options = new LobbySearchSetParameterOptions
                {
                    Parameter = new AttributeData
                    {
                        Key = bucketIdKey,
                        Value = new AttributeDataValue
                        {
                            AsUtf8 = bucketIdValue
                        }
                    },
                    ComparisonOp = ComparisonOp.Equal
                };
                result = searchHandle.SetParameter(ref options);
                if (result != Result.Success)
                {
                    Debug.LogError($"failed to search lobby set lobby id {result}");
                    onCompletedSearch?.Invoke(result, new List<Lobby>());
                    return;
                }
            }

            {
                var options = new LobbySearchFindOptions
                {
                    LocalUserId = localProductUserId
                };
                searchHandle.Find(ref options, null, (ref LobbySearchFindCallbackInfo data) =>
                {
                    if (data.ResultCode == Result.NotFound)
                    {
                        Debug.Log("not found lobby");
                        onCompletedSearch?.Invoke(data.ResultCode, new List<Lobby>());
                        return;
                    }

                    if (data.ResultCode != Result.Success)
                    {
                        Debug.LogError("failed to search lobby infos");
                        onCompletedSearch?.Invoke(data.ResultCode, new List<Lobby>());
                        return;
                    }

                    {
                        var resultCountOptions = new LobbySearchGetSearchResultCountOptions();
                        var resultCount = searchHandle.GetSearchResultCount(ref resultCountOptions);
                        var lobbyList = new List<Lobby>();
                        for (uint i = 0; i < resultCount; i++)
                        {
                            var getResultOptions = new LobbySearchCopySearchResultByIndexOptions
                            {
                                LobbyIndex = i
                            };
                            result = searchHandle.CopySearchResultByIndex(ref getResultOptions, out var lobbyDetails);
                            if (result != Result.Success || lobbyDetails == null)
                            {
                                Debug.LogError($"failed to get lobby detail {result}");
                                onCompletedSearch?.Invoke(result, lobbyList);
                                continue;
                            }

                            lobbyList.Add(new Lobby(lobbyDetails));
                        }

                        onCompletedSearch?.Invoke(Result.Success, lobbyList);
                    }
                });
            }
        }
    }
}