using System;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using PlayEveryWare.EpicOnlineServices;
using UnityEngine;
using CopyIdTokenOptions = Epic.OnlineServices.Auth.CopyIdTokenOptions;
using Credentials = Epic.OnlineServices.Auth.Credentials;
using LoginCallbackInfo = Epic.OnlineServices.Auth.LoginCallbackInfo;
using LoginOptions = Epic.OnlineServices.Auth.LoginOptions;

namespace MokomoGamesLib.Runtime.EOS.Auth
{
    public class LoginService
    {
        public enum LoginType
        {
            None,
            Local,
            DeviceId
        }

        private readonly AuthInterface _authInterface;
        private ConnectInterface _connectInterface;

        public LoginService(ConnectInterface connectInterface, AuthInterface authInterface)
        {
            _connectInterface = connectInterface;
            _authInterface = authInterface;
        }

        public void Login(
            string id,
            string token,
            LoginType loginType,
            Action<ProductUserId> onCompletedLogin)
        {
            var getCredentialsType = new Func<LoginType, LoginCredentialType>(x =>
            {
                switch (x)
                {
                    case LoginType.Local:
                        return LoginCredentialType.Developer;
                    case LoginType.DeviceId:
                        return LoginCredentialType.ExchangeCode;
                    case LoginType.None:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(x), x, null);
                }
            });

            var getExternalType = new Func<LoginType, ExternalCredentialType>(x =>
            {
                switch (x)
                {
                    case LoginType.Local:
                        return ExternalCredentialType.Epic;
                    case LoginType.DeviceId:
                        return ExternalCredentialType.DeviceidAccessToken;
                    case LoginType.None:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(x), x, null);
                }
            });

            var externalType = getExternalType(loginType);

            var options = new LoginOptions
            {
                Credentials = new Credentials
                {
                    Id = id,
                    Token = token,
                    Type = getCredentialsType(loginType),
                    ExternalType = externalType
                },
                ScopeFlags = AuthScopeFlags.NoFlags
            };

            var displayName = "DisplayUser";

            _authInterface.Login(
                ref options,
                null,
                (ref LoginCallbackInfo data) =>
                {
                    Debug.Log($"?????????????????????????????? Epic Account Id: {data.LocalUserId}");

                    if (loginType == LoginType.DeviceId)
                    {
                        var createDeviceIdOptions = new CreateDeviceIdOptions
                        {
                            DeviceModel = SystemInfo.deviceModel
                        };
                        var eosConnectInterface = EOSManager.Instance.GetEOSConnectInterface();
                        eosConnectInterface.CreateDeviceId(
                            ref createDeviceIdOptions,
                            null,
                            (ref CreateDeviceIdCallbackInfo createDeviceIdCallbackInfo) =>
                            {
                                var success = false;
                                if (createDeviceIdCallbackInfo.ResultCode == Result.Success)
                                {
                                    success = true;
                                    Debug.Log("DeviceId?????????");
                                }
                                else if (createDeviceIdCallbackInfo.ResultCode == Result.DuplicateNotAllowed)
                                {
                                    success = true;
                                    Debug.Log("DeviceId?????????????????????");
                                }
                                else
                                {
                                    Debug.Log($"?????????????????????????????????????????????{createDeviceIdCallbackInfo.ResultCode}");
                                }

                                if (success)
                                    EOSManager.Instance.StartConnectLoginWithDeviceToken(
                                        displayName,
                                        info => { onCompletedLogin?.Invoke(info.LocalUserId); });
                            });
                    }
                    else if (loginType == LoginType.Local)
                    {
                        var copyIdTokenOptions = new CopyIdTokenOptions
                        {
                            AccountId = data.LocalUserId
                        };

                        _authInterface.CopyIdToken(ref copyIdTokenOptions, out var outIdToken);
                        if (outIdToken == null)
                        {
                            Debug.LogError("Auth Token?????????????????????????????????");
                            return;
                        }

                        var connectLoginOptions = new Epic.OnlineServices.Connect.LoginOptions
                        {
                            Credentials = new Epic.OnlineServices.Connect.Credentials
                            {
                                Token = outIdToken.Value.JsonWebToken,
                                Type = ExternalCredentialType.EpicIdToken
                            }
                        };
                        EOSManager.Instance.StartConnectLoginWithOptions(connectLoginOptions, info =>
                        {
                            if (info.ResultCode != Result.Success && info.ContinuanceToken == null)
                            {
                                Debug.Log($"???????????????????????????????????? {info.ResultCode}");
                                return;
                            }

                            // note: ContinuanceToken?????????????????????????????????????????????
                            if (info.ContinuanceToken != null)
                                // note: ??????????????????????????????
                                EOSManager.Instance.CreateConnectUserWithContinuanceToken(
                                    info.ContinuanceToken, createUserInfo =>
                                    {
                                        if (createUserInfo.ResultCode != Result.Success) Debug.Log($"??????????????????????????? {createUserInfo.ResultCode}");

                                        onCompletedLogin?.Invoke(createUserInfo.LocalUserId);
                                    });
                            else
                                onCompletedLogin?.Invoke(info.LocalUserId);
                        });
                    }
                });
        }
    }
}