﻿using Cysharp.Threading.Tasks;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public class ClusterClient : LiteNetLibClient
    {
        public override string LogTag { get { return nameof(ClusterClient) + ":" + _appServer.PeerType; } }
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        public delegate void OnResponseAppServerRegister(AckResponseCode responseCode);
        public OnResponseAppServerRegister onResponseAppServerRegister;
        public delegate void OnResponseAppServerAddress(AckResponseCode responseCode, CentralServerPeerInfo peerInfo);
        public OnResponseAppServerAddress onResponseAppServerAddress;
        public delegate void OnResponseUserCount(AckResponseCode responseCode, int userCount);
        public OnResponseUserCount onResponseUserCount;
        public delegate void OnKickUser(string userId, UITextKeys message);
        public OnKickUser onKickUser;
        public delegate void OnPlayerCharacterRemovedDelegate(string userId, string characterId);
        public OnPlayerCharacterRemovedDelegate onPlayerCharacterRemoved;
        public bool IsAppRegistered { get; private set; }
#endif
        private readonly IAppServer _appServer;

        public ClusterClient(IAppServer appServer) : base(new LiteNetLibTransport("CLUSTER", 16, 16))
        {
            _appServer = appServer;
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            EnableRequestResponse(MMOMessageTypes.Request, MMOMessageTypes.Response);
            RegisterResponseHandler<RequestAppServerRegisterMessage, ResponseAppServerRegisterMessage>(MMORequestTypes.AppServerRegister, HandleResponseAppServerRegister);
            RegisterResponseHandler<RequestAppServerAddressMessage, ResponseAppServerAddressMessage>(MMORequestTypes.AppServerAddress, HandleResponseAppServerAddress);
            RegisterResponseHandler<EmptyMessage, ResponseUserCountMessage>(MMORequestTypes.UserCount);
            RegisterMessageHandler(MMOMessageTypes.AppServerAddress, HandleAppServerAddress);
            RegisterMessageHandler(MMOMessageTypes.KickUser, HandleKickUser);
            RegisterMessageHandler(MMOMessageTypes.PlayerCharacterRemoved, HandlePlayerCharacterRemoved);
#endif
        }

        public ClusterClient(MapSpawnNetworkManager mapSpawnNetworkManager) : this(mapSpawnNetworkManager as IAppServer)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            RegisterRequestHandler<RequestSpawnMapMessage, ResponseSpawnMapMessage>(MMORequestTypes.SpawnMap, mapSpawnNetworkManager.HandleRequestSpawnMap);
#endif
        }

        public override void OnClientReceive(TransportEventData eventData)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            switch (eventData.type)
            {
                case ENetworkEvent.ConnectEvent:
                    Logging.Log(LogTag, "OnClientConnected");
                    OnConnectedToClusterServer();
                    break;
                case ENetworkEvent.DataEvent:
                    ReadPacket(-1, eventData.reader);
                    break;
                case ENetworkEvent.DisconnectEvent:
                    Logging.Log(LogTag, "OnClientDisconnected peer. disconnectInfo.Reason: " + eventData.disconnectInfo.Reason);
                    StopClient();
                    OnDisconnectedFromClusterServer().Forget();
                    break;
                case ENetworkEvent.ErrorEvent:
                    Logging.LogError(LogTag, "OnClientNetworkError endPoint: " + eventData.endPoint + " socketErrorCode " + eventData.socketError + " errorMessage " + eventData.errorMessage);
                    break;
            }
#endif
        }

        public void OnAppStart()
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            Logging.Log(LogTag, "Starting server");
            ConnectToClusterServer();
#endif
        }

        public void OnAppStop()
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            Logging.Log(LogTag, "Stopping server");
            DisconnectFromClusterServer();
#endif
        }

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        private void ConnectToClusterServer()
        {
            Logging.Log(LogTag, "Connecting to Cluster Server: " + _appServer.ClusterServerAddress + ":" + _appServer.ClusterServerPort);
            StartClient(_appServer.ClusterServerAddress, _appServer.ClusterServerPort);
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        private void DisconnectFromClusterServer()
        {
            Logging.Log(LogTag, "Disconnecting from Cluster Server");
            StopClient();
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        private void OnConnectedToClusterServer()
        {
            Logging.Log(LogTag, "Connected to Cluster Server");
            // Send Request
            RequestAppServerRegister(new CentralServerPeerInfo()
            {
                peerType = _appServer.PeerType,
                networkAddress = _appServer.AppAddress,
                networkPort = _appServer.AppPort,
                channelId = _appServer.ChannelId,
                refId = _appServer.RefId,
            });
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        private async UniTaskVoid OnDisconnectedFromClusterServer()
        {
            Logging.Log(LogTag, "Disconnected from Central Server");
            IsAppRegistered = false;
            Logging.Log(LogTag, "Reconnect to central in 5 seconds...");
            await DelayOneSec();
            Logging.Log(LogTag, "Reconnect to central in 4 seconds...");
            await DelayOneSec();
            Logging.Log(LogTag, "Reconnect to central in 3 seconds...");
            await DelayOneSec();
            Logging.Log(LogTag, "Reconnect to central in 2 seconds...");
            await DelayOneSec();
            Logging.Log(LogTag, "Reconnect to central in 1 seconds...");
            ConnectToClusterServer();
        }
#endif

        private async UniTask DelayOneSec()
        {
#if NET || NETCOREAPP
            await Task.Delay(1000);
#else
            await UniTask.Delay(1000, true);
#endif
        }

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        public bool RequestAppServerRegister(CentralServerPeerInfo peerInfo)
        {
            Logging.Log(LogTag, "App Register is requesting");
            return SendRequest(MMORequestTypes.AppServerRegister, new RequestAppServerRegisterMessage()
            {
                peerInfo = peerInfo,
            });
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        private void HandleResponseAppServerRegister(
            ResponseHandlerData responseHandler,
            AckResponseCode responseCode,
            ResponseAppServerRegisterMessage response)
        {
            if (responseCode == AckResponseCode.Success)
            {
                Logging.Log(LogTag, "App Registered successfully");
                IsAppRegistered = true;
            }
            if (onResponseAppServerRegister != null)
                onResponseAppServerRegister.Invoke(responseCode);
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        public bool RequestAppServerAddress(CentralServerPeerType peerType, string channelId, string refId)
        {
            Logging.Log(LogTag, "App Address is requesting");
            return SendRequest(MMORequestTypes.AppServerAddress, new RequestAppServerAddressMessage()
            {
                peerType = peerType,
                channelId = channelId,
                refId = refId,
            });
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        private void HandleResponseAppServerAddress(
            ResponseHandlerData responseHandler,
            AckResponseCode responseCode,
            ResponseAppServerAddressMessage response)
        {
            if (onResponseAppServerAddress != null)
                onResponseAppServerAddress.Invoke(responseCode, response.peerInfo);
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        private void HandleResponseUserCount(
            ResponseHandlerData responseHandler,
            AckResponseCode responseCode,
            ResponseUserCountMessage response)
        {
            if (onResponseUserCount != null)
                onResponseUserCount.Invoke(responseCode, response.userCount);
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        private void HandleAppServerAddress(MessageHandlerData messageHandler)
        {
            ResponseAppServerAddressMessage response = messageHandler.ReadMessage<ResponseAppServerAddressMessage>();
            if (onResponseAppServerAddress != null)
                onResponseAppServerAddress.Invoke(AckResponseCode.Success, response.peerInfo);
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        private void HandlePlayerCharacterRemoved(MessageHandlerData messageHandler)
        {
            string userId = messageHandler.Reader.GetString();
            string characterId = messageHandler.Reader.GetString();
            if (onPlayerCharacterRemoved != null)
                onPlayerCharacterRemoved.Invoke(userId, characterId);
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        private void HandleKickUser(MessageHandlerData messageHandler)
        {
            string kickUserId = messageHandler.Reader.GetString();
            UITextKeys message = (UITextKeys)messageHandler.Reader.GetPackedUShort();
            if (onKickUser != null)
                onKickUser.Invoke(kickUserId, message);
        }
#endif
    }
}
