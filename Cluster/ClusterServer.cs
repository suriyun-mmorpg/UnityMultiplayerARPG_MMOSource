using LiteNetLib;
using LiteNetLibManager;
using System.Collections.Generic;
using LiteNetLib.Utils;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MultiplayerARPG.MMO
{
    public class ClusterServer : LiteNetLibServer
    {
        public override string LogTag { get { return nameof(ClusterServer); } }

        private readonly CentralNetworkManager _centralNetworkManager;
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        private Dictionary<string, SocialCharacterData> _mapUsersByCharacterId = new Dictionary<string, SocialCharacterData>();
        public IReadOnlyDictionary<string, SocialCharacterData> MapUsersByCharacterId => _mapUsersByCharacterId;


        private Dictionary<string, long> _connectionIdsByCharacterId = new Dictionary<string, long>();
        public IReadOnlyDictionary<string, long> ConnectionIdsByCharacterId => _connectionIdsByCharacterId;

        private Dictionary<string, long> _connectionIdsByCharacterName = new Dictionary<string, long>();
        public IReadOnlyDictionary<string, long> ConnectionIdsByCharacterName => _connectionIdsByCharacterName;

        private Dictionary<string, long> _connectionIdsByDespawningCharacterId = new Dictionary<string, long>();
        public IReadOnlyDictionary<string, long> ConnectionIdsByDespawningCharacterId => _connectionIdsByDespawningCharacterId;

        // Map spawn server peers
        private Dictionary<long, CentralServerPeerInfo> _mapSpawnServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        public IReadOnlyDictionary<long, CentralServerPeerInfo> MapSpawnServerPeers => _mapSpawnServerPeers;

        // Map server peers
        private Dictionary<long, CentralServerPeerInfo> _mapServerPeers = new Dictionary<long, CentralServerPeerInfo>();
        public IReadOnlyDictionary<long, CentralServerPeerInfo> MapServerPeers => _mapServerPeers;

        private Dictionary<string, CentralServerPeerInfo> _mapServerPeersByKey = new Dictionary<string, CentralServerPeerInfo>();
        /// <summary>
        /// Key is `{channelId}_{refId}`
        /// </summary>
        public IReadOnlyDictionary<string, CentralServerPeerInfo> MapServerPeersByKey => _mapServerPeersByKey;

        private Dictionary<string, CentralServerPeerInfo> _instanceMapServerPeersByKey = new Dictionary<string, CentralServerPeerInfo>();
        /// <summary>
        /// Key is `{channelId}_{refId}`
        /// </summary>
        public IReadOnlyDictionary<string, CentralServerPeerInfo> InstanceMapServerPeersByKey => _instanceMapServerPeersByKey;

        private Dictionary<string, List<CentralServerPeerInfo>> _allocateMapServerPeersByRefId = new Dictionary<string, List<CentralServerPeerInfo>>();
        /// <summary>
        /// Key is `{refId}`
        /// </summary>
        public IReadOnlyDictionary<string, List<CentralServerPeerInfo>> AllocateMapServerPeersByRefId => _allocateMapServerPeersByRefId;

        private Dictionary<string, RequestProceedResultDelegate<ResponseSpawnMapMessage>> _mapSpawnResultActions = new Dictionary<string, RequestProceedResultDelegate<ResponseSpawnMapMessage>>();
        /// <summary>
        /// Key is `{channelId}_{refId}`
        /// </summary>
        public IReadOnlyDictionary<string, RequestProceedResultDelegate<ResponseSpawnMapMessage>> MapSpawnResultActions => _mapSpawnResultActions;
#endif

        public ClusterServer(CentralNetworkManager centralNetworkManager) : base(new LiteNetLibTransport("CLUSTER", 16, 16))
        {
            _centralNetworkManager = centralNetworkManager;
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            EnableRequestResponse(MMOMessageTypes.Request, MMOMessageTypes.Response);
            // Generic
            RegisterRequestHandler<RequestAppServerRegisterMessage, ResponseAppServerRegisterMessage>(MMORequestTypes.AppServerRegister, HandleRequestAppServerRegister);
            RegisterRequestHandler<RequestAppServerAddressMessage, ResponseAppServerAddressMessage>(MMORequestTypes.AppServerAddress, HandleRequestAppServerAddress);
            // Map
            RegisterResponseHandler<RequestForceDespawnCharacterMessage, EmptyMessage>(MMORequestTypes.ForceDespawnCharacter);
            RegisterResponseHandler<RequestSpawnMapMessage, ResponseSpawnMapMessage>(MMORequestTypes.RunMap);
            RegisterMessageHandler(MMOMessageTypes.Chat, HandleChat);
            RegisterMessageHandler(MMOMessageTypes.UpdateMapUser, HandleUpdateMapUser);
            RegisterMessageHandler(MMOMessageTypes.UpdatePartyMember, HandleUpdatePartyMember);
            RegisterMessageHandler(MMOMessageTypes.UpdateParty, HandleUpdateParty);
            RegisterMessageHandler(MMOMessageTypes.UpdateGuildMember, HandleUpdateGuildMember);
            RegisterMessageHandler(MMOMessageTypes.UpdateGuild, HandleUpdateGuild);
            // Map-spawn
            RegisterRequestHandler<RequestSpawnMapMessage, ResponseSpawnMapMessage>(MMORequestTypes.SpawnMap, HandleRequestSpawnMap);
            RegisterResponseHandler<RequestSpawnMapMessage, ResponseSpawnMapMessage>(MMORequestTypes.SpawnMap);
            RegisterRequestHandler<EmptyMessage, ResponseUserCountMessage>(MMORequestTypes.UserCount, HandleRequestUserCount);
#endif
        }

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        public bool StartServer()
        {
            return StartServer(_centralNetworkManager.clusterServerPort, int.MaxValue);
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        protected override void OnStopServer()
        {
            base.OnStopServer();
            _mapUsersByCharacterId.Clear();
            _connectionIdsByCharacterId.Clear();
            _connectionIdsByCharacterName.Clear();
            _connectionIdsByDespawningCharacterId.Clear();
            _mapSpawnServerPeers.Clear();
            _mapServerPeers.Clear();
            _mapServerPeersByKey.Clear();
            _instanceMapServerPeersByKey.Clear();
            _allocateMapServerPeersByRefId.Clear();
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        public override void OnServerReceive(TransportEventData eventData)
        {
            CentralServerPeerInfo tempPeerInfo;
            switch (eventData.type)
            {
                case ENetworkEvent.ConnectEvent:
                    Logging.Log(LogTag, $"OnPeerConnected peer.ConnectionId: {eventData.connectionId}");
                    ConnectionIds.Add(eventData.connectionId);
                    break;
                case ENetworkEvent.DataEvent:
                    ReadPacket(eventData.connectionId, eventData.reader);
                    break;
                case ENetworkEvent.DisconnectEvent:
                    Logging.Log(LogTag, $"OnPeerDisconnected peer.ConnectionId: {eventData.connectionId} disconnectInfo.Reason: {eventData.disconnectInfo.Reason}");
                    ConnectionIds.Remove(eventData.connectionId);
                    // Remove disconnect map spawn server
                    _mapSpawnServerPeers.Remove(eventData.connectionId);
                    // Remove disconnect map server
                    if (_mapServerPeers.TryGetValue(eventData.connectionId, out tempPeerInfo))
                    {
                        string key = tempPeerInfo.GetPeerInfoKey();
                        _mapServerPeersByKey.Remove(key);
                        _instanceMapServerPeersByKey.Remove(key);
                        _mapServerPeers.Remove(eventData.connectionId);
                        RemoveMapUsers(eventData.connectionId);
                    }
                    RemoveAllocateMapServer(eventData.connectionId);
                    break;
                case ENetworkEvent.ErrorEvent:
                    Logging.LogError(LogTag, $"OnPeerNetworkError endPoint: {eventData.endPoint } socketErrorCode {eventData.socketError } errorMessage {eventData.errorMessage}");
                    break;
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        public void RemoveAllocateMapServer(long connectionId)
        {
            List<string> keys = new List<string>(_allocateMapServerPeersByRefId.Keys);
            for (int i = 0; i < keys.Count; ++i)
            {
                string key = keys[i];
                List<CentralServerPeerInfo> allocatePeers = _allocateMapServerPeersByRefId[key];
                for (int j = allocatePeers.Count - 1; j >= 0; --j)
                {
                    if (allocatePeers[j].connectionId == connectionId)
                    {
                        _allocateMapServerPeersByRefId[key].RemoveAt(j);
                    }
                }
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        private void RemoveMapUsers(long connectionId)
        {
            List<KeyValuePair<string, SocialCharacterData>> mapUsers = _mapUsersByCharacterId.ToList();
            foreach (KeyValuePair<string, SocialCharacterData> entry in mapUsers)
            {
                // Find characters which connected to disconnecting map server
                if (!_connectionIdsByCharacterId.TryGetValue(entry.Key, out long tempConnectionId) || connectionId != tempConnectionId)
                    continue;

                // Clear disconnecting map users data
                _mapUsersByCharacterId.Remove(entry.Key);
                _connectionIdsByCharacterId.Remove(entry.Key);
                _connectionIdsByCharacterName.Remove(entry.Value.characterName);
                _connectionIdsByDespawningCharacterId[entry.Key] = connectionId;

                // Send remove message to other map servers
                UpdateMapUser(UpdateUserCharacterMessage.UpdateType.Remove, entry.Value, connectionId);
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        private UniTaskVoid HandleRequestAppServerRegister(
            RequestHandlerData requestHandler,
            RequestAppServerRegisterMessage request,
            RequestProceedResultDelegate<ResponseAppServerRegisterMessage> result)
        {
            long connectionId = requestHandler.ConnectionId;
            UITextKeys message = UITextKeys.NONE;
            if (request.ValidateHash())
            {
                CentralServerPeerInfo peerInfo = request.peerInfo;
                string key = peerInfo.GetPeerInfoKey();
                peerInfo.connectionId = connectionId;
                switch (request.peerInfo.peerType)
                {
                    case CentralServerPeerType.MapSpawnServer:
                        _mapSpawnServerPeers[connectionId] = peerInfo;
                        Logging.Log(LogTag, $"Register Map Spawn Server: [{connectionId}]");
                        break;
                    case CentralServerPeerType.MapServer:
                        if (!_mapServerPeersByKey.ContainsKey(key))
                        {
                            BroadcastAppServers(connectionId, peerInfo);
                            // Tell the map-server which request for a spawning that the map spawned
                            if (_mapSpawnResultActions.TryGetValue(key, out RequestProceedResultDelegate<ResponseSpawnMapMessage> resultForMapServer))
                            {
                                resultForMapServer.Invoke(AckResponseCode.Success, new ResponseSpawnMapMessage()
                                {
                                    message = UITextKeys.NONE,
                                    peerInfo = peerInfo,
                                });
                            }
                            // Collects server data
                            _mapServerPeersByKey[key] = peerInfo;
                            _mapServerPeers[connectionId] = peerInfo;
                            Logging.Log(LogTag, $"Register Map Server: [{connectionId}] [{key}]");
                        }
                        else
                        {
                            message = UITextKeys.UI_ERROR_MAP_EXISTED;
                            Logging.Log(LogTag, $"Register Map Server Failed: [{connectionId}] [{key}] [{message}]");
                        }
                        break;
                    case CentralServerPeerType.InstanceMapServer:
                        if (!_instanceMapServerPeersByKey.ContainsKey(key))
                        {
                            BroadcastAppServers(connectionId, peerInfo);
                            // Tell the map-server which request for a spawning that the map spawned
                            if (_mapSpawnResultActions.TryGetValue(key, out RequestProceedResultDelegate<ResponseSpawnMapMessage> resultForMapServer))
                            {
                                resultForMapServer.Invoke(AckResponseCode.Success, new ResponseSpawnMapMessage()
                                {
                                    message = UITextKeys.NONE,
                                    peerInfo = peerInfo,
                                });
                            }
                            // Collects server data
                            _instanceMapServerPeersByKey[key] = peerInfo;
                            _mapServerPeers[connectionId] = peerInfo;
                            Logging.Log(LogTag, $"Register Instance Map Server: [{connectionId}] [{key}]");
                        }
                        else
                        {
                            message = UITextKeys.UI_ERROR_EVENT_EXISTED;
                            Logging.Log(LogTag, $"Register Instance Map Server Failed: [{connectionId}] [{key}] [{message}]");
                        }
                        break;
                    case CentralServerPeerType.AllocateMapServer:
                        // Create a new collection if it is not existed
                        if (!_allocateMapServerPeersByRefId.ContainsKey(peerInfo.refId))
                            _allocateMapServerPeersByRefId.Add(peerInfo.refId, new List<CentralServerPeerInfo>());
                        _allocateMapServerPeersByRefId[peerInfo.refId].Add(peerInfo);
                        Logging.Log(LogTag, $"Register Allocate Map Server: [{connectionId}] [{peerInfo.refId}] [{_allocateMapServerPeersByRefId[peerInfo.refId].Count}]");
                        break;
                }
            }
            else
            {
                message = UITextKeys.UI_ERROR_INVALID_SERVER_HASH;
                Logging.Log(LogTag, $"Register Server Failed: [{connectionId}] [{message}]");
            }
            // Response
            result.Invoke(
                !message.IsError() ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseAppServerRegisterMessage()
                {
                    message = message,
                });
            return default;
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        /// <summary>
        /// This function will be used to send connection information to connected map servers and cluster servers
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="broadcastPeerInfo"></param>
        private void BroadcastAppServers(long connectionId, CentralServerPeerInfo broadcastPeerInfo)
        {
            // Send map peer info to other map server
            foreach (CentralServerPeerInfo mapPeerInfo in _mapServerPeers.Values)
            {
                // Send other info to current peer
                SendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.AppServerAddress, (writer) => writer.PutValue(new ResponseAppServerAddressMessage()
                {
                    message = UITextKeys.NONE,
                    peerInfo = mapPeerInfo,
                }));
                // Send current info to other peer
                SendPacket(mapPeerInfo.connectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.AppServerAddress, (writer) => writer.PutValue(new ResponseAppServerAddressMessage()
                {
                    message = UITextKeys.NONE,
                    peerInfo = broadcastPeerInfo,
                }));
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        private UniTaskVoid HandleRequestAppServerAddress(
            RequestHandlerData requestHandler,
            RequestAppServerAddressMessage request,
            RequestProceedResultDelegate<ResponseAppServerAddressMessage> result)
        {
            long connectionId = requestHandler.ConnectionId;
            UITextKeys message = UITextKeys.NONE;
            CentralServerPeerInfo peerInfo = new CentralServerPeerInfo();
            string key = request.GetPeerInfoKey();
            switch (request.peerType)
            {
                // TODO: Balancing servers when there are multiple servers with same type
                case CentralServerPeerType.MapSpawnServer:
                    if (_mapSpawnServerPeers.Count > 0)
                    {
                        peerInfo = _mapSpawnServerPeers.Values.First();
                        Logging.Log(LogTag, $"Request Map Spawn Address: [{connectionId}]");
                    }
                    else
                    {
                        message = UITextKeys.UI_ERROR_SERVER_NOT_FOUND;
                        Logging.Log(LogTag, $"Request Map Spawn Address: [{connectionId}] [{message}]");
                    }
                    break;
                case CentralServerPeerType.MapServer:
                    if (!_mapServerPeersByKey.TryGetValue(key, out peerInfo))
                    {
                        message = UITextKeys.UI_ERROR_SERVER_NOT_FOUND;
                        Logging.Log(LogTag, $"Request Map Address: [{connectionId}] [{key}] [{message}]");
                    }
                    break;
                case CentralServerPeerType.InstanceMapServer:
                    if (!_instanceMapServerPeersByKey.TryGetValue(key, out peerInfo))
                    {
                        message = UITextKeys.UI_ERROR_SERVER_NOT_FOUND;
                        Logging.Log(LogTag, $"Request Map Address: [{connectionId}] [{key}] [{message}]");
                    }
                    break;
            }
            // Response
            result.Invoke(
                !message.IsError() ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseAppServerAddressMessage()
                {
                    message = message,
                    peerInfo = peerInfo,
                });
            return default;
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        private void HandleChat(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            ChatMessage message = messageHandler.ReadMessage<ChatMessage>();
            switch (message.channel)
            {
                case ChatChannel.Local:
                case ChatChannel.Global:
                case ChatChannel.System:
                case ChatChannel.Party:
                case ChatChannel.Guild:
                    // Send message to all map servers, let's map servers filter messages
                    SendPacketToAllConnections(0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.Chat, (writer) =>
                    {
                        writer.PutValue(message);
                    });
                    break;
                case ChatChannel.Whisper:
                    long senderConnectionId = 0;
                    long receiverConnectionId = 0;
                    // Send message to map server which have the character
                    if (!string.IsNullOrEmpty(message.senderName) && _connectionIdsByCharacterName.TryGetValue(message.senderName, out senderConnectionId))
                        SendPacket(senderConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.Chat, (writer) => writer.PutValue(message));
                    if (!string.IsNullOrEmpty(message.receiverName) && _connectionIdsByCharacterName.TryGetValue(message.receiverName, out receiverConnectionId) && (receiverConnectionId != senderConnectionId))
                        SendPacket(receiverConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.Chat, (writer) => writer.PutValue(message));
                    break;
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        private void HandleUpdateMapUser(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            UpdateUserCharacterMessage message = messageHandler.ReadMessage<UpdateUserCharacterMessage>();
            SocialCharacterData character = message.character;
            switch (message.type)
            {
                case UpdateUserCharacterMessage.UpdateType.Add:
                    if (!_mapUsersByCharacterId.ContainsKey(character.id))
                    {
                        _mapUsersByCharacterId[character.id] = character;
                        _connectionIdsByCharacterId[character.id] = connectionId;
                        _connectionIdsByCharacterName[character.characterName] = connectionId;
                        _connectionIdsByDespawningCharacterId.Remove(character.id);
                        UpdateMapUser(UpdateUserCharacterMessage.UpdateType.Add, character, connectionId);
                    }
                    break;
                case UpdateUserCharacterMessage.UpdateType.Remove:
                    if (_mapUsersByCharacterId.TryGetValue(message.character.id, out character))
                    {
                        _mapUsersByCharacterId.Remove(character.id);
                        _connectionIdsByCharacterId.Remove(character.id);
                        _connectionIdsByCharacterName.Remove(character.characterName);
                        _connectionIdsByDespawningCharacterId[character.id] = connectionId;
                        UpdateMapUser(UpdateUserCharacterMessage.UpdateType.Remove, character, connectionId);
                    }
                    break;
                case UpdateUserCharacterMessage.UpdateType.Online:
                    if (_mapUsersByCharacterId.ContainsKey(character.id))
                    {
                        _mapUsersByCharacterId[character.id] = character;
                        UpdateMapUser(UpdateUserCharacterMessage.UpdateType.Online, character, connectionId);
                    }
                    break;
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        public void HandleUpdatePartyMember(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            UpdateSocialMemberMessage message = messageHandler.ReadMessage<UpdateSocialMemberMessage>();
            if (!_mapServerPeers.ContainsKey(connectionId))
                return;
            foreach (long mapServerConnectionId in _mapServerPeers.Keys)
            {
                if (mapServerConnectionId != connectionId)
                    SendPacket(mapServerConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdatePartyMember, (writer) => writer.PutValue(message));
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        public void HandleUpdateParty(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            UpdatePartyMessage message = messageHandler.ReadMessage<UpdatePartyMessage>();
            if (!_mapServerPeers.ContainsKey(connectionId))
                return;
            foreach (long mapServerConnectionId in _mapServerPeers.Keys)
            {
                if (mapServerConnectionId != connectionId)
                    SendPacket(mapServerConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateParty, (writer) => writer.PutValue(message));
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        public void HandleUpdateGuildMember(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            UpdateSocialMemberMessage message = messageHandler.ReadMessage<UpdateSocialMemberMessage>();
            if (!_mapServerPeers.ContainsKey(connectionId))
                return;
            foreach (long mapServerConnectionId in _mapServerPeers.Keys)
            {
                if (mapServerConnectionId != connectionId)
                    SendPacket(mapServerConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateGuildMember, (writer) => writer.PutValue(message));
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        public void HandleUpdateGuild(MessageHandlerData messageHandler)
        {
            long connectionId = messageHandler.ConnectionId;
            UpdateGuildMessage message = messageHandler.ReadMessage<UpdateGuildMessage>();
            if (!_mapServerPeers.ContainsKey(connectionId))
                return;
            foreach (long mapServerConnectionId in _mapServerPeers.Keys)
            {
                if (mapServerConnectionId != connectionId)
                    SendPacket(mapServerConnectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateGuild, (writer) => writer.PutValue(message));
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        public void UpdateMapUser(UpdateUserCharacterMessage.UpdateType updateType, SocialCharacterData userData, long exceptConnectionId)
        {
            foreach (long mapServerConnectionId in _mapServerPeers.Keys)
            {
                if (mapServerConnectionId == exceptConnectionId)
                    continue;

                UpdateMapUser(mapServerConnectionId, updateType, userData);
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        public void UpdateMapUser(long connectionId, UpdateUserCharacterMessage.UpdateType updateType, SocialCharacterData userData)
        {
            UpdateUserCharacterMessage message = new UpdateUserCharacterMessage();
            message.type = updateType;
            message.character = userData;
            SendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.UpdateMapUser, (writer) => writer.PutValue(message));
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        public void PlayerCharacterRemoved(string userId, string characterId)
        {
            List<long> mapServerPeerConnectionIds = new List<long>(_mapServerPeers.Keys);
            foreach (long connectionId in mapServerPeerConnectionIds)
            {
                SendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.PlayerCharacterRemoved, (writer) =>
                {
                    writer.Put(userId);
                    writer.Put(characterId);
                });
            }
            List<SocialCharacterData> mapUsers = new List<SocialCharacterData>(_mapUsersByCharacterId.Values);
            foreach (SocialCharacterData mapUser in mapUsers)
            {
                _mapUsersByCharacterId.Remove(userId);
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        public void KickUser(string userId, UITextKeys message)
        {
            List<long> mapServerPeerConnectionIds = new List<long>(_mapServerPeers.Keys);
            foreach (long connectionId in mapServerPeerConnectionIds)
            {
                SendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.KickUser, (writer) =>
                {
                    writer.Put(userId);
                    writer.PutPackedUShort((ushort)message);
                });
            }
            List<SocialCharacterData> mapUsers = new List<SocialCharacterData>(_mapUsersByCharacterId.Values);
            foreach (SocialCharacterData mapUser in mapUsers)
            {
                _mapUsersByCharacterId.Remove(userId);
            }
        }
#endif

        public bool MapContainsUser(string userId)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            foreach (SocialCharacterData mapUser in _mapUsersByCharacterId.Values)
            {
                if (mapUser.userId.Equals(userId))
                    return true;
            }
#endif
            return false;
        }

        public async UniTask RequestSpawnMap(long mapSpawnConnectionId, RequestSpawnMapMessage request, string key, RequestProceedResultDelegate<ResponseSpawnMapMessage> resultForMapServer)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            AsyncResponseData<ResponseSpawnMapMessage> spawnResult = await SendRequestAsync<RequestSpawnMapMessage, ResponseSpawnMapMessage>(mapSpawnConnectionId, MMORequestTypes.SpawnMap, request, _centralNetworkManager.mapSpawnMillisecondsTimeout);
            if (!spawnResult.IsSuccess)
            {
                // Send error to map-server immediately
                resultForMapServer.Invoke(spawnResult.ResponseCode, spawnResult.Response);
            }
            // Awaiting for the instance's connection
            _mapSpawnResultActions[key] = resultForMapServer;
#else
            await UniTask.Yield();
#endif
        }

        /// <summary>
        /// This is function which read request from map server to spawn another map server
        /// Then it will response back when requested map server is ready
        /// </summary>
        /// <param name="messageHandler"></param>
        protected async UniTaskVoid HandleRequestSpawnMap(
            RequestHandlerData requestHandler,
            RequestSpawnMapMessage request,
            RequestProceedResultDelegate<ResponseSpawnMapMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            // Generate a new instance ID, it won't being generated by map-server anymore
            request.instanceId = _centralNetworkManager.DataManager.GenerateMapSpawnInstanceId();
            // Run a map from allocated map-server collection
            if (_allocateMapServerPeersByRefId.ContainsKey(request.mapName))
            {
                List<CentralServerPeerInfo> allocatePeers = _allocateMapServerPeersByRefId[request.mapName];
                for (int i = 0; i < allocatePeers.Count; ++i)
                {
                    AsyncResponseData<ResponseSpawnMapMessage> runResult = await SendRequestAsync<RequestSpawnMapMessage, ResponseSpawnMapMessage>(_allocateMapServerPeersByRefId[request.mapName][i].connectionId, MMORequestTypes.RunMap, request);
                    if (!runResult.IsSuccess)
                        continue;
                    _allocateMapServerPeersByRefId[request.mapName].RemoveAt(i);
                    result.Invoke(AckResponseCode.Success, runResult.Response);
                    return;
                }
            }
            // Spawning a map-server by a map-spawn server
            string key = PeerInfoExtensions.GetPeerInfoKey(request.channelId, request.instanceId);
            List<long> connectionIds = new List<long>(_mapSpawnServerPeers.Keys);
            // Random map-spawn server to spawn map, will use returning ackId as reference to map-server's transport handler and ackId
            System.Random random = new System.Random(System.DateTime.Now.Millisecond);
            await RequestSpawnMap(connectionIds[random.Next(0, connectionIds.Count)], request, key, result);
#else
            await UniTask.Yield();
#endif
        }

        /// <summary>
        /// This is function which read request from any server to get online users count (May being used by GM command)
        /// </summary>
        /// <param name="messageHandler"></param>
        protected UniTaskVoid HandleRequestUserCount(
            RequestHandlerData requestHandler,
            EmptyMessage request,
            RequestProceedResultDelegate<ResponseUserCountMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new ResponseUserCountMessage()
            {
                userCount = _mapUsersByCharacterId.Count,
            });
#endif
            return default;
        }

        public List<ChannelEntry> GetChannels()
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            Dictionary<string, int> connectionCounts = new Dictionary<string, int>();
            foreach (long connectionId in ConnectionIdsByCharacterId.Values)
            {
                if (!_mapServerPeers.ContainsKey(connectionId))
                    continue;
                string channelId = _mapServerPeers[connectionId].channelId;
                if (!connectionCounts.ContainsKey(channelId))
                    connectionCounts.Add(channelId, 0);
                connectionCounts[channelId]++;
            }
            List<ChannelEntry> result = new List<ChannelEntry>();
            foreach (ChannelData data in _centralNetworkManager.Channels.Values)
            {
                result.Add(new ChannelEntry()
                {
                    id = data.id,
                    title = data.title,
                    maxConnections = data.maxConnections <= 0 ? _centralNetworkManager.defaultChannelMaxConnections : data.maxConnections,
                    connections = connectionCounts.ContainsKey(data.id) ? connectionCounts[data.id] : 0,
                });
            }
            return result;
#else
            return new List<ChannelEntry>();
#endif
        }

        public int GetChannelConnections(string channelId)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            int connections = 0;
            foreach (long connectionId in ConnectionIdsByCharacterId.Values)
            {
                if (!_mapServerPeers.ContainsKey(connectionId))
                    continue;
                if (!string.Equals(channelId, _mapServerPeers[connectionId].channelId))
                    continue;
                connections++;
            }
            return connections;
#else
            return 0;
#endif
        }

        public async UniTask<bool> ConfirmDespawnCharacter(string characterId)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            if (!_connectionIdsByDespawningCharacterId.TryGetValue(characterId, out long connectionId))
            {
                // No despawning character, so it may not played by player or already despawned
                return true;
            }
            if (!_mapServerPeers.ContainsKey(connectionId))
            {
                // No map-server peer, it may be disconnected, so determine that it is despawned
                return true;
            }
            AsyncResponseData<EmptyMessage> result = await SendRequestAsync<RequestForceDespawnCharacterMessage, EmptyMessage>(connectionId, MMORequestTypes.ForceDespawnCharacter, new RequestForceDespawnCharacterMessage()
            {
                characterId = characterId,
            });
            switch (result.ResponseCode)
            {
                case AckResponseCode.Success:
                    // Confirmed by map-server that the character was despawned, so it can be removed from despawning collection
                    _connectionIdsByDespawningCharacterId.Remove(characterId);
                    return true;
                case AckResponseCode.Timeout:
                    // TODO: May tell client what is happening
                    break;
                case AckResponseCode.Error:
                    // TODO: May tell client what is happening
                    break;
                case AckResponseCode.Unimplemented:
                    // TODO: May tell client what is happening
                    break;
                case AckResponseCode.Exception:
                    // TODO: May tell client what is happening
                    break;
            }
#else
            await UniTask.Yield();
#endif
            return false;
        }

        public static string GetAppServerRegisterHash(CentralServerPeerType peerType, long time)
        {
            MD5 algorithm = MD5.Create();  // or use SHA256.Create();
            return Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(peerType.ToString() + time.ToString())));
        }
    }
}
