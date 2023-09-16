using LiteNetLibManager;
using LiteNetLib.Utils;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        public bool RequestCharacters(ResponseDelegate<ResponseCharactersMessage> callback)
        {
            return ClientSendRequest(MMORequestTypes.RequestCharacters, EmptyMessage.Value, responseDelegate: callback);
        }

        public bool RequestCreateCharacter(PlayerCharacterData characterData, ResponseDelegate<ResponseCreateCharacterMessage> callback)
        {
            return ClientSendRequest(MMORequestTypes.RequestCreateCharacter, new RequestCreateCharacterMessage()
            {
                characterName = characterData.CharacterName,
                dataId = characterData.DataId,
                entityId = characterData.EntityId,
                factionId = characterData.FactionId,
                publicBools = characterData.PublicBools,
                publicInts = characterData.PublicInts,
                publicFloats = characterData.PublicFloats,
            }, callback, extraRequestSerializer: (writer) => SerializeCreateCharacterExtra(characterData, writer));
        }

        private void SerializeCreateCharacterExtra(PlayerCharacterData characterData, NetDataWriter writer)
        {
            this.InvokeInstanceDevExtMethods("SerializeCreateCharacterExtra", characterData, writer);
        }

        public bool RequestDeleteCharacter(string characterId, ResponseDelegate<ResponseDeleteCharacterMessage> callback)
        {
            return ClientSendRequest(MMORequestTypes.RequestDeleteCharacter, new RequestDeleteCharacterMessage()
            {
                characterId = characterId,
            }, responseDelegate: callback);
        }

        public bool RequestSelectCharacter(string channelId, string characterId, ResponseDelegate<ResponseSelectCharacterMessage> callback)
        {
            return ClientSendRequest(MMORequestTypes.RequestSelectCharacter, new RequestSelectCharacterMessage()
            {
                channelId = channelId,
                characterId = characterId,
            }, responseDelegate: callback);
        }

        protected async UniTaskVoid HandleRequestCharacters(
            RequestHandlerData requestHandler,
            EmptyMessage request,
            RequestProceedResultDelegate<ResponseCharactersMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            long connectionId = requestHandler.ConnectionId;
            if (!_userPeers.TryGetValue(connectionId, out CentralUserPeerInfo userPeerInfo))
            {
                result.InvokeError(new ResponseCharactersMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            // Get characters from server
            DatabaseApiResult<CharactersResp> charactersResp = await DatabaseClient.ReadCharactersAsync(new ReadCharactersReq()
            {
                UserId = userPeerInfo.userId
            });
            if (!charactersResp.IsSuccess)
            {
                result.InvokeError(new ResponseCharactersMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            // Response
            result.InvokeSuccess(new ResponseCharactersMessage()
            {
                characters = charactersResp.Response.List,
            });
#endif
        }

        protected async UniTaskVoid HandleRequestCreateCharacter(
            RequestHandlerData requestHandler,
            RequestCreateCharacterMessage request,
            RequestProceedResultDelegate<ResponseCreateCharacterMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            long connectionId = requestHandler.ConnectionId;
            NetDataReader reader = requestHandler.Reader;
            string characterName = request.characterName.Trim();
            int dataId = request.dataId;
            int entityId = request.entityId;
            int factionId = request.factionId;
            IList<CharacterDataBoolean> publicBools = request.publicBools;
            IList<CharacterDataInt32> publicInts = request.publicInts;
            IList<CharacterDataFloat32> publicFloats = request.publicFloats;
            if (!NameExtensions.IsValidCharacterName(characterName))
            {
                result.InvokeError(new ResponseCreateCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_CHARACTER_NAME
                });
                return;
            }
            
            if (characterName.Length < minCharacterNameLength)
            {
                result.InvokeError(new ResponseCreateCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_CHARACTER_NAME_TOO_SHORT
                });
                return;
            }

            if (characterName.Length > maxCharacterNameLength)
            {
                result.InvokeError(new ResponseCreateCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_CHARACTER_NAME_TOO_LONG
                });
                return;
            }

            // Validate character name
            DatabaseApiResult<FindCharacterNameResp> findCharacterNameResp = await DatabaseClient.FindCharacterNameAsync(new FindCharacterNameReq()
            {
                CharacterName = characterName
            });
            if (!findCharacterNameResp.IsSuccess)
            {
                result.InvokeError(new ResponseCreateCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            if (findCharacterNameResp.Response.FoundAmount > 0)
            {
                result.InvokeError(new ResponseCreateCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_CHARACTER_NAME_EXISTED,
                });
                return;
            }
            if (!_userPeers.TryGetValue(connectionId, out CentralUserPeerInfo userPeerInfo))
            {
                result.InvokeError(new ResponseCreateCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (string.IsNullOrEmpty(characterName) || characterName.Length < minCharacterNameLength)
            {
                result.InvokeError(new ResponseCreateCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_CHARACTER_NAME_TOO_SHORT,
                });
                return;
            }
            if (characterName.Length > maxCharacterNameLength)
            {
                result.InvokeError(new ResponseCreateCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_CHARACTER_NAME_TOO_LONG,
                });
                return;
            }
            if (!DataManager.CanCreateCharacter(ref dataId, ref entityId, ref factionId, publicBools, publicInts, publicFloats, out UITextKeys errorMessage))
            {
                // If there is factions, it must have faction with the id stored in faction dictionary
                result.InvokeError(new ResponseCreateCharacterMessage()
                {
                    message = errorMessage,
                });
                return;
            }
            string characterId = DataManager.GenerateCharacterId();
            PlayerCharacterData characterData = new PlayerCharacterData();
            characterData.Id = characterId;
            DataManager.SetNewPlayerCharacterData(characterData, characterName, dataId, entityId, factionId, publicBools, publicInts, publicFloats);
            DeserializeCreateCharacterExtra(characterData, reader);
            DatabaseApiResult<CharacterResp> createResp = await DatabaseClient.CreateCharacterAsync(new CreateCharacterReq()
            {
                UserId = userPeerInfo.userId,
                CharacterData = characterData,
            });
            if (!createResp.IsSuccess)
            {
                result.InvokeError(new ResponseCreateCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            // Response
            result.InvokeSuccess(new ResponseCreateCharacterMessage());
#endif
        }

        private void DeserializeCreateCharacterExtra(PlayerCharacterData characterData, NetDataReader reader)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            this.InvokeInstanceDevExtMethods("DeserializeCreateCharacterExtra", characterData, reader);
#endif
        }

        protected async UniTaskVoid HandleRequestDeleteCharacter(
            RequestHandlerData requestHandler,
            RequestDeleteCharacterMessage request,
            RequestProceedResultDelegate<ResponseDeleteCharacterMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            long connectionId = requestHandler.ConnectionId;
            if (!_userPeers.TryGetValue(connectionId, out CentralUserPeerInfo userPeerInfo))
            {
                result.InvokeError(new ResponseDeleteCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            DatabaseApiResult deleteResp = await DatabaseClient.DeleteCharacterAsync(new DeleteCharacterReq()
            {
                UserId = userPeerInfo.userId,
                CharacterId = request.characterId
            });
            if (!deleteResp.IsSuccess)
            {
                result.InvokeError(new ResponseDeleteCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            // Kick from servers
            ClusterServer.PlayerCharacterRemoved(userPeerInfo.userId, request.characterId);
            // Response
            result.InvokeSuccess(new ResponseDeleteCharacterMessage());
#endif
        }

        protected async UniTaskVoid HandleRequestSelectCharacter(
            RequestHandlerData requestHandler,
            RequestSelectCharacterMessage request,
            RequestProceedResultDelegate<ResponseSelectCharacterMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            long connectionId = requestHandler.ConnectionId;
            if (!_userPeers.TryGetValue(connectionId, out CentralUserPeerInfo userPeerInfo))
            {
                result.InvokeError(new ResponseSelectCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            // Kick player's character from map-servers
            if (!await ClusterServer.ConfirmDespawnCharacter(request.characterId))
            {
                result.InvokeError(new ResponseSelectCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_ALREADY_LOGGED_IN,
                });
                return;
            }
            // Get channel, or use default one
            string channelId = request.channelId;
            if (string.IsNullOrEmpty(channelId))
                channelId = Channels.Keys.First();
            if (!Channels.TryGetValue(channelId, out ChannelData channel))
            {
                result.InvokeError(new ResponseSelectCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_CHANNEL_ID,
                });
                return;
            }
            int maxConnections = channel.maxConnections;
            if (maxConnections <= 0)
                maxConnections = defaultChannelMaxConnections;
            if (ClusterServer.GetChannelConnections(channelId) >= maxConnections)
            {
                result.InvokeError(new ResponseSelectCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_CHANNEL_IS_FULL,
                });
                return;
            }
            DatabaseApiResult<CharacterResp> characterResp = await DatabaseClient.ReadCharacterAsync(new ReadCharacterReq()
            {
                UserId = userPeerInfo.userId,
                CharacterId = request.characterId,
                ForceClearCache = true,
            });
            if (!characterResp.IsSuccess)
            {
                result.InvokeError(new ResponseSelectCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            PlayerCharacterData character = characterResp.Response.CharacterData;
            if (character == null)
            {
                result.InvokeError(new ResponseSelectCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_CHARACTER_DATA,
                });
                return;
            }
            if (!ClusterServer.MapServerPeersByMapId.TryGetValue(PeerInfoExtensions.GetPeerInfoKey(channelId, character.CurrentMapName), out CentralServerPeerInfo mapServerPeerInfo))
            {
                result.InvokeError(new ResponseSelectCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_MAP_SERVER_NOT_READY,
                });
                return;
            }
            // Response
            result.InvokeSuccess(new ResponseSelectCharacterMessage()
            {
                sceneName = mapServerPeerInfo.refId,
                networkAddress = mapServerPeerInfo.networkAddress,
                networkPort = mapServerPeerInfo.networkPort,
            });
#endif
        }
    }
}
