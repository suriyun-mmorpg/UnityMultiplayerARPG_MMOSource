﻿using System.Collections.Generic;
using Insthync.DevExtension;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using Cysharp.Threading.Tasks;
using System.Net.Sockets;
using System.Threading.Tasks;
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
using UnityEngine;
#endif

namespace MultiplayerARPG.MMO
{
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
    [DefaultExecutionOrder(DefaultExecutionOrders.CENTRAL_NETWORK_MANAGER)]
#endif
    public partial class CentralNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
        public const string DEFAULT_CHANNEL_ID = "default";
        protected static readonly NetDataWriter s_Writer = new NetDataWriter();

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        // User peers (Login / Register / Manager characters)
        protected readonly Dictionary<long, CentralUserPeerInfo> _userPeers = new Dictionary<long, CentralUserPeerInfo>();
        protected readonly Dictionary<string, CentralUserPeerInfo> _userPeersByUserId = new Dictionary<string, CentralUserPeerInfo>();
#endif
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        [Header("Cluster")]
#endif
        public int clusterServerPort = 6010;

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        [Header("Map Spawn")]
#endif
        public int mapSpawnMillisecondsTimeout = 0;

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        [Header("Channels")]
#endif
        public int defaultChannelMaxConnections = 500;
        public List<ChannelData> channels = new List<ChannelData>();

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        [Header("User Account")]
#endif
        public bool disableDefaultLogin = false;
        public int minUsernameLength = 2;
        public int maxUsernameLength = 24;
        public int minPasswordLength = 2;
        public int minCharacterNameLength = 2;
        public int maxCharacterNameLength = 16;
        public bool requireEmail = false;
        public bool requireEmailVerification = false;

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        [Header("Statistic")]
#endif
        public float updateUserCountInterval = 5f;

        public System.Action onClientConnected;
        public System.Action<DisconnectReason, SocketError, UITextKeys> onClientDisconnected;
        public System.Action onClientStopped;

        private long _lastUserCountUpdateTime = 0;

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        public ClusterServer ClusterServer { get; private set; }
        public IDatabaseClient DatabaseClient { get; set; }
        public ICentralServerDataManager DataManager { get; set; }
#endif
        private Dictionary<string, ChannelData> _channels;
        public Dictionary<string, ChannelData> Channels
        {
            get
            {
                if (_channels == null)
                {
                    _channels = new Dictionary<string, ChannelData>();
                    foreach (ChannelData channel in channels)
                    {
                        if (string.IsNullOrEmpty(channel.id))
                        {
                            Logging.LogWarning("Cannot add channel with empty ID.");
                            continue;
                        }
                        if (_channels.ContainsKey(channel.id))
                        {
                            Logging.LogWarning($"Already has a channel with ID: {channel.id}, it won't being added again.");
                            continue;
                        }
                        _channels[channel.id] = channel;
                    }
                    if (_channels.Count == 0)
                    {
                        _channels[DEFAULT_CHANNEL_ID] = new ChannelData()
                        {
                            id = DEFAULT_CHANNEL_ID,
                            title = "Default",
                            maxConnections = defaultChannelMaxConnections,
                        };
                    }
                }
                return _channels;
            }
        }

#if NET || NETCOREAPP
        public CentralNetworkManager() : base()
        {
            Initialize();
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        protected override void Start()
        {
            Initialize();
            base.Start();
        }
#endif

        protected virtual void Initialize()
        {
            if (defaultChannelMaxConnections <= 0)
                defaultChannelMaxConnections = 500;
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            GameInstance.OnGameDataLoadedEvent += GameInstance_OnGameDataLoadedEvent;
#endif
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            ClusterServer = new ClusterServer(this);
#endif
        }

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        private void GameInstance_OnGameDataLoadedEvent()
        {
            GameInstance.OnGameDataLoadedEvent -= GameInstance_OnGameDataLoadedEvent;
            minCharacterNameLength = GameInstance.Singleton.minCharacterNameLength;
            maxCharacterNameLength = GameInstance.Singleton.maxCharacterNameLength;
        }
#endif

        protected override void Update()
        {
            base.Update();

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            if (IsServer)
            {
                ClusterServer.Update();
                long currentTime = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (currentTime - _lastUserCountUpdateTime > updateUserCountInterval * 1000)
                {
                    _lastUserCountUpdateTime = currentTime;
                    UpdateCountUsers().Forget();
                }
            }
#endif
        }

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        protected async UniTaskVoid UpdateCountUsers()
        {
            // Update user count
            await DatabaseClient.UpdateUserCount(new UpdateUserCountReq()
            {
                UserCount = ClusterServer.CountUsers(),
            });
        }
#endif

        protected override void RegisterMessages()
        {
            EnableRequestResponse(MMOMessageTypes.Request, MMOMessageTypes.Response);
            // Requests
            RegisterRequestToServer<RequestUserLoginMessage, ResponseUserLoginMessage>(MMORequestTypes.UserLogin, HandleRequestUserLogin);
            RegisterRequestToServer<RequestUserRegisterMessage, ResponseUserRegisterMessage>(MMORequestTypes.UserRegister, HandleRequestUserRegister);
            RegisterRequestToServer<EmptyMessage, EmptyMessage>(MMORequestTypes.UserLogout, HandleRequestUserLogout);
            RegisterRequestToServer<EmptyMessage, ResponseCharactersMessage>(MMORequestTypes.Characters, HandleRequestCharacters);
            RegisterRequestToServer<RequestCreateCharacterMessage, ResponseCreateCharacterMessage>(MMORequestTypes.CreateCharacter, HandleRequestCreateCharacter);
            RegisterRequestToServer<RequestDeleteCharacterMessage, ResponseDeleteCharacterMessage>(MMORequestTypes.DeleteCharacter, HandleRequestDeleteCharacter);
            RegisterRequestToServer<RequestSelectCharacterMessage, ResponseSelectCharacterMessage>(MMORequestTypes.SelectCharacter, HandleRequestSelectCharacter);
            RegisterRequestToServer<RequestValidateAccessTokenMessage, ResponseValidateAccessTokenMessage>(MMORequestTypes.ValidateAccessToken, HandleRequestValidateAccessToken);
            RegisterRequestToServer<EmptyMessage, ResponseChannelsMessage>(MMORequestTypes.Channels, HandleRequestChannels);
            // Client messages
            RegisterClientMessage(MMOMessageTypes.Disconnect, HandleServerDisconnect);
            // Keeping `RegisterClientMessages` and `RegisterServerMessages` for backward compatibility, can use any of below dev extension methods
            this.InvokeInstanceDevExtMethods("RegisterClientMessages");
            this.InvokeInstanceDevExtMethods("RegisterServerMessages");
            this.InvokeInstanceDevExtMethods("RegisterMessages");
        }

        public async void KickClient(long connectionId, byte[] data)
        {
            if (!IsServer)
                return;
            ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.Disconnect, (writer) => writer.PutBytesWithLength(data));
#if NET || NETCOREAPP
            await Task.Delay(500);
#else
            await UniTask.Delay(500);
#endif
            ServerTransport.ServerDisconnect(connectionId);
        }

        public void KickClient(long connectionId, UITextKeys message)
        {
            if (!IsServer)
                return;
            s_Writer.Reset();
            s_Writer.PutPackedUShort((ushort)message);
            KickClient(connectionId, s_Writer.CopyData());
        }

        protected void HandleServerDisconnect(MessageHandlerData messageHandler)
        {
            Client.SetDisconnectData(messageHandler.Reader.GetBytesWithLength());
        }

        protected virtual void Clean()
        {
            this.InvokeInstanceDevExtMethods("Clean");
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            _userPeers.Clear();
            _userPeersByUserId.Clear();
#endif
        }

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        public override async void OnStartServer()
        {
            this.InvokeInstanceDevExtMethods("OnStartServer");
            base.OnStartServer();
            ClusterServer.StartServer();
            await Task.Delay(1000);
            await DatabaseClient.DeleteAllReservedStorageAsync();
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        public override void OnStopServer()
        {
            Clean();
            base.OnStopServer();
            ClusterServer.StopServer();
        }
#endif

        public override void OnStartClient(LiteNetLibClient client)
        {
            this.InvokeInstanceDevExtMethods("OnStartClient", client);
            base.OnStartClient(client);
        }

        public override void OnStopClient()
        {
            if (!IsServer)
                Clean();
            base.OnStopClient();
            if (onClientStopped != null)
                onClientStopped.Invoke();
        }

        public override void OnClientConnected()
        {
            base.OnClientConnected();
            if (onClientConnected != null)
                onClientConnected.Invoke();
        }

        public override void OnClientDisconnected(DisconnectReason reason, SocketError socketError, byte[] data)
        {
            UITextKeys message = UITextKeys.NONE;
            if (data != null && data.Length > 0)
            {
                NetDataReader reader = new NetDataReader(data);
                message = (UITextKeys)reader.GetPackedUShort();
            }
            if (onClientDisconnected != null)
                onClientDisconnected.Invoke(reason, socketError, message);
        }

        public override void OnPeerDisconnected(long connectionId, DisconnectReason reason, SocketError socketError)
        {
            base.OnPeerDisconnected(connectionId, reason, socketError);
            RemoveUserPeerByConnectionId(connectionId, out _);
        }

        public bool RemoveUserPeerByConnectionId(long connectionId, out CentralUserPeerInfo userPeerInfo)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            if (_userPeers.TryGetValue(connectionId, out userPeerInfo))
            {
                _userPeersByUserId.Remove(userPeerInfo.userId);
                _userPeers.Remove(connectionId);
                return true;
            }
            return false;
#else
            userPeerInfo = default;
            return false;
#endif
        }

        public bool RemoveUserPeerByUserId(string userId, out CentralUserPeerInfo userPeerInfo)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            if (_userPeersByUserId.TryGetValue(userId, out userPeerInfo))
            {
                _userPeersByUserId.Remove(userPeerInfo.userId);
                _userPeers.Remove(userPeerInfo.connectionId);
                return true;
            }
            return false;
#else
            userPeerInfo = default;
            return false;
#endif
        }

        public async UniTask<bool> MapContainsUser(string userId)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            return await ClusterServer.MapContainsUser(userId);
#else
            await UniTask.Yield();
            return false;
#endif
        }
    }
}
