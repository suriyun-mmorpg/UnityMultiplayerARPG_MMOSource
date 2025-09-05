using ConcurrentCollections;
using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public partial class DatabaseNetworkManager
    {
        public static GuildRoleData[] GuildMemberRoles { get; set; } = new GuildRoleData[] {
            new GuildRoleData() { roleName = "Master", canInvite = true, canKick = true, canUseStorage = true },
            new GuildRoleData() { roleName = "Member 1", canInvite = false, canKick = false, canUseStorage = false },
            new GuildRoleData() { roleName = "Member 2", canInvite = false, canKick = false, canUseStorage = false },
            new GuildRoleData() { roleName = "Member 3", canInvite = false, canKick = false, canUseStorage = false },
            new GuildRoleData() { roleName = "Member 4", canInvite = false, canKick = false, canUseStorage = false },
            new GuildRoleData() { roleName = "Member 5", canInvite = false, canKick = false, canUseStorage = false },
        };
        public static int[] GuildExpTree { get; set; } = new int[0];
        protected readonly ConcurrentHashSet<string> _insertingCharacterNames = new ConcurrentHashSet<string>();
        protected readonly ConcurrentHashSet<string> _insertingGuildNames = new ConcurrentHashSet<string>();

        protected async UniTaskVoid ValidateUserLogin(RequestHandlerData requestHandler, DbRequestMessage<ValidateUserLoginReq> request, RequestProceedResultDelegate<ValidateUserLoginResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new ValidateUserLoginResp()
            {
                UserId = await Database.ValidateUserLogin(request.Data.Username, request.Data.Password),
            });
#endif
        }

        protected async UniTaskVoid ValidateAccessToken(RequestHandlerData requestHandler, DbRequestMessage<ValidateAccessTokenReq> request, RequestProceedResultDelegate<ValidateAccessTokenResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new ValidateAccessTokenResp()
            {
                IsPass = await ValidateAccessToken(request.Data.UserId, request.Data.AccessToken),
            });
#endif
        }

        protected async UniTaskVoid GetUserLevel(RequestHandlerData requestHandler, DbRequestMessage<GetUserLevelReq> request, RequestProceedResultDelegate<GetUserLevelResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new GetUserLevelResp()
            {
                UserLevel = await Database.GetUserLevel(request.Data.UserId),
            });
#endif
        }

        protected async UniTaskVoid GetGold(RequestHandlerData requestHandler, DbRequestMessage<GetGoldReq> request, RequestProceedResultDelegate<GoldResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new GoldResp()
            {
                Gold = await GetGold(request.Data.UserId)
            });
#endif
        }

        protected async UniTaskVoid ChangeGold(RequestHandlerData requestHandler, DbRequestMessage<ChangeGoldReq> request, RequestProceedResultDelegate<GoldResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            // Update data to database
            int changedGold = await Database.ChangeGold(request.Data.UserId, request.Data.ChangeAmount);
            result.InvokeSuccess(new GoldResp()
            {
                Gold = changedGold,
            });
#endif
        }

        protected async UniTaskVoid GetCash(RequestHandlerData requestHandler, DbRequestMessage<GetCashReq> request, RequestProceedResultDelegate<CashResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new CashResp()
            {
                Cash = await GetCash(request.Data.UserId)
            });
#endif
        }

        protected async UniTaskVoid ChangeCash(RequestHandlerData requestHandler, DbRequestMessage<ChangeCashReq> request, RequestProceedResultDelegate<CashResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            // Update data to database
            int changedCash = await Database.ChangeCash(request.Data.UserId, request.Data.ChangeAmount);
            result.InvokeSuccess(new CashResp()
            {
                Cash = changedCash,
            });
#endif
        }

        protected async UniTaskVoid UpdateAccessToken(RequestHandlerData requestHandler, DbRequestMessage<UpdateAccessTokenReq> request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            // Update data to database
            await Database.UpdateAccessToken(request.Data.UserId, request.Data.AccessToken);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid CreateUserLogin(RequestHandlerData requestHandler, DbRequestMessage<CreateUserLoginReq> request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            // Insert new user login to database
            await Database.CreateUserLogin(request.Data.Username, request.Data.Password, request.Data.Email);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid FindUsername(RequestHandlerData requestHandler, DbRequestMessage<FindUsernameReq> request, RequestProceedResultDelegate<FindUsernameResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new FindUsernameResp()
            {
                FoundAmount = await FindUsername(request.Data.Username),
            });
#endif
        }

        protected async UniTaskVoid CreateCharacter(RequestHandlerData requestHandler, DbRequestMessage<CreateCharacterReq> request, RequestProceedResultDelegate<CharacterResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            PlayerCharacterData character = request.Data.CharacterData;
            if (_insertingCharacterNames.Contains(character.CharacterName))
            {
                result.InvokeError(new CharacterResp());
                return;
            }
            _insertingCharacterNames.Add(character.CharacterName);
            long foundAmount = await FindCharacterName(character.CharacterName);
            if (foundAmount > 0)
            {
                _insertingCharacterNames.TryRemove(character.CharacterName);
                result.InvokeError(new CharacterResp());
                return;
            }
            // Insert new character to database
            await Database.CreateCharacter(request.Data.UserId, character);
            _insertingCharacterNames.TryRemove(character.CharacterName);
            result.InvokeSuccess(new CharacterResp()
            {
                CharacterData = character
            });
#endif
        }

        protected async UniTaskVoid GetCharacter(RequestHandlerData requestHandler, DbRequestMessage<GetCharacterReq> request, RequestProceedResultDelegate<CharacterResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new CharacterResp()
            {
                CharacterData = await GetCharacterWithUserIdValidation(request.Data.CharacterId, request.Data.UserId),
            });
#endif
        }

        protected async UniTaskVoid GetCharacters(RequestHandlerData requestHandler, DbRequestMessage<GetCharactersReq> request, RequestProceedResultDelegate<CharactersResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            List<PlayerCharacterData> characters = await Database.GetCharacters(request.Data.UserId);
            result.InvokeSuccess(new CharactersResp()
            {
                List = characters
            });
#endif
        }

        protected async UniTaskVoid UpdateCharacter(RequestHandlerData requestHandler, DbRequestMessage<UpdateCharacterReq> request, RequestProceedResultDelegate<CharacterResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            PlayerCharacterData playerCharacter = await GetCharacter(request.Data.CharacterData.Id);
            if (playerCharacter == null)
            {
                result.InvokeError(new CharacterResp()
                {
                    CharacterData = null,
                });
            }
            await Database.UpdateCharacter(request.Data.State, request.Data.CharacterData, request.Data.SummonBuffs, request.Data.DeleteStorageReservation);
            List<UniTask> tasks = new List<UniTask>
            {
                DatabaseCache.SetPlayerCharacter(request.Data.CharacterData),
                DatabaseCache.SetSummonBuffs(request.Data.CharacterData.Id, request.Data.SummonBuffs),
            };
            await UniTask.WhenAll(tasks);
            result.InvokeSuccess(new CharacterResp()
            {
                CharacterData = request.Data.CharacterData,
            });
#endif
        }

        protected async UniTaskVoid DeleteCharacter(RequestHandlerData requestHandler, DbRequestMessage<DeleteCharacterReq> request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            PlayerCharacterData playerCharacter = await GetCharacter(request.Data.CharacterId);
            if (playerCharacter == null)
            {
                result.InvokeError(EmptyMessage.Value);
            }
            // Delete data from database
            await Database.DeleteCharacter(request.Data.UserId, request.Data.CharacterId);
            // Remove data from cache
            if (playerCharacter != null)
            {
                await UniTask.WhenAll(
                    DatabaseCache.RemovePlayerCharacter(playerCharacter.Id),
                    DatabaseCache.RemoveSocialCharacter(playerCharacter.Id));
            }
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid FindCharacterName(RequestHandlerData requestHandler, DbRequestMessage<FindCharacterNameReq> request, RequestProceedResultDelegate<FindCharacterNameResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new FindCharacterNameResp()
            {
                FoundAmount = await FindCharacterName(request.Data.CharacterName),
            });
#endif
        }

        protected async UniTaskVoid FindCharacters(RequestHandlerData requestHandler, DbRequestMessage<FindCharacterNameReq> request, RequestProceedResultDelegate<SocialCharactersResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new SocialCharactersResp()
            {
                List = await Database.FindCharacters(request.Data.FinderId, request.Data.CharacterName, request.Data.Skip, request.Data.Limit)
            });
#endif
        }

        protected async UniTaskVoid CreateFriend(RequestHandlerData requestHandler, DbRequestMessage<CreateFriendReq> request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            await Database.CreateFriend(request.Data.Character1Id, request.Data.Character2Id, request.Data.State);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid DeleteFriend(RequestHandlerData requestHandler, DbRequestMessage<DeleteFriendReq> request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            await Database.DeleteFriend(request.Data.Character1Id, request.Data.Character2Id);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid GetFriends(RequestHandlerData requestHandler, DbRequestMessage<GetFriendsReq> request, RequestProceedResultDelegate<SocialCharactersResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new SocialCharactersResp()
            {
                List = await Database.GetFriends(request.Data.CharacterId, request.Data.ReadById2, request.Data.State, request.Data.Skip, request.Data.Limit),
            });
#endif
        }

        protected async UniTaskVoid CreateBuilding(RequestHandlerData requestHandler, DbRequestMessage<CreateBuildingReq> request, RequestProceedResultDelegate<BuildingResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            BuildingSaveData building = request.Data.BuildingData;
            // Insert data to database
            await Database.CreateBuilding(request.Data.ChannelId, request.Data.MapName, building);
            // Cache building data
            await DatabaseCache.SetBuilding(request.Data.ChannelId, request.Data.MapName, building);
            result.InvokeSuccess(new BuildingResp()
            {
                BuildingData = request.Data.BuildingData
            });
#endif
        }

        protected async UniTaskVoid UpdateBuilding(RequestHandlerData requestHandler, DbRequestMessage<UpdateBuildingReq> request, RequestProceedResultDelegate<BuildingResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            // Update data to database
            await Database.UpdateBuilding(request.Data.ChannelId, request.Data.MapName, request.Data.BuildingData);
            // Cache building data
            await DatabaseCache.SetBuilding(request.Data.ChannelId, request.Data.MapName, request.Data.BuildingData);
            result.InvokeSuccess(new BuildingResp()
            {
                BuildingData = request.Data.BuildingData
            });
#endif
        }

        protected async UniTaskVoid DeleteBuilding(RequestHandlerData requestHandler, DbRequestMessage<DeleteBuildingReq> request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            // Remove data from cache
            await DatabaseCache.RemoveBuilding(request.Data.ChannelId, request.Data.MapName, request.Data.BuildingId);
            // Remove data from database
            await Database.DeleteBuilding(request.Data.ChannelId, request.Data.MapName, request.Data.BuildingId);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid GetBuildings(RequestHandlerData requestHandler, DbRequestMessage<GetBuildingsReq> request, RequestProceedResultDelegate<BuildingsResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new BuildingsResp()
            {
                List = await GetBuildings(request.Data.ChannelId, request.Data.MapName),
            });
#endif
        }

        protected async UniTaskVoid CreateParty(RequestHandlerData requestHandler, DbRequestMessage<CreatePartyReq> request, RequestProceedResultDelegate<PartyResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            // Insert to database
            int partyId = await Database.CreateParty(request.Data.ShareExp, request.Data.ShareItem, request.Data.LeaderCharacterId);
            PartyData party = new PartyData(partyId, request.Data.ShareExp, request.Data.ShareItem, request.Data.LeaderCharacterId);
            // Cache the data, it will be used later
            await UniTask.WhenAll(
                DatabaseCache.SetParty(party),
                DatabaseCache.SetPlayerCharacterPartyId(request.Data.LeaderCharacterId, partyId),
                DatabaseCache.SetSocialCharacterPartyId(request.Data.LeaderCharacterId, partyId));
            result.InvokeSuccess(new PartyResp()
            {
                PartyData = party
            });
#endif
        }

        protected async UniTaskVoid UpdateParty(RequestHandlerData requestHandler, DbRequestMessage<UpdatePartyReq> request, RequestProceedResultDelegate<PartyResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            PartyData party = await GetParty(request.Data.PartyId);
            if (party == null)
            {
                result.InvokeError(new PartyResp()
                {
                    PartyData = null
                });
                return;
            }
            // Update to database
            await Database.UpdateParty(request.Data.PartyId, request.Data.ShareExp, request.Data.ShareItem);
            // Update to cache
            party.Setting(request.Data.ShareExp, request.Data.ShareItem);
            await DatabaseCache.SetParty(party);
            result.InvokeSuccess(new PartyResp()
            {
                PartyData = party
            });
#endif
        }

        protected async UniTaskVoid UpdatePartyLeader(RequestHandlerData requestHandler, DbRequestMessage<UpdatePartyLeaderReq> request, RequestProceedResultDelegate<PartyResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            PartyData party = await GetParty(request.Data.PartyId);
            if (party == null)
            {
                result.InvokeError(new PartyResp()
                {
                    PartyData = null
                });
                return;
            }
            // Update to database
            await Database.UpdatePartyLeader(request.Data.PartyId, request.Data.LeaderCharacterId);
            // Update to cache
            party.SetLeader(request.Data.LeaderCharacterId);
            await DatabaseCache.SetParty(party);
            result.InvokeSuccess(new PartyResp()
            {
                PartyData = party
            });
#endif
        }

        protected async UniTaskVoid DeleteParty(RequestHandlerData requestHandler, DbRequestMessage<DeletePartyReq> request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            // Remove data from database
            await Database.DeleteParty(request.Data.PartyId);
            // Remove data from cache
            await DatabaseCache.RemoveParty(request.Data.PartyId);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid UpdateCharacterParty(RequestHandlerData requestHandler, DbRequestMessage<UpdateCharacterPartyReq> request, RequestProceedResultDelegate<PartyResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            PartyData party = await GetParty(request.Data.PartyId);
            if (party == null)
            {
                result.InvokeError(new PartyResp()
                {
                    PartyData = null
                });
                return;
            }
            SocialCharacterData character = request.Data.SocialCharacterData;
            // Update to database
            await Database.UpdateCharacterParty(character.id, request.Data.PartyId);
            // Update to cache
            party.AddMember(character);
            await UniTask.WhenAll(
                DatabaseCache.SetParty(party),
                DatabaseCache.SetPlayerCharacterPartyId(character.id, party.id),
                DatabaseCache.SetSocialCharacterPartyId(character.id, party.id));
            result.InvokeSuccess(new PartyResp()
            {
                PartyData = party
            });
#endif
        }

        protected async UniTaskVoid ClearCharacterParty(RequestHandlerData requestHandler, DbRequestMessage<ClearCharacterPartyReq> request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            PlayerCharacterData character = await GetCharacter(request.Data.CharacterId);
            if (character == null)
            {
                result.InvokeSuccess(EmptyMessage.Value);
                return;
            }
            PartyData party = await GetParty(character.PartyId);
            if (party == null)
            {
                result.InvokeSuccess(EmptyMessage.Value);
                return;
            }
            // Update to database
            await Database.UpdateCharacterParty(request.Data.CharacterId, 0);
            // Update to cache
            party.RemoveMember(request.Data.CharacterId);
            await UniTask.WhenAll(
                DatabaseCache.SetParty(party),
                DatabaseCache.SetPlayerCharacterPartyId(request.Data.CharacterId, 0),
                DatabaseCache.SetSocialCharacterPartyId(request.Data.CharacterId, 0));
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid GetParty(RequestHandlerData requestHandler, DbRequestMessage<GetPartyReq> request, RequestProceedResultDelegate<PartyResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            if (request.Data.ForceClearCache)
                await DatabaseCache.RemoveParty(request.Data.PartyId);
            result.InvokeSuccess(new PartyResp()
            {
                PartyData = await GetParty(request.Data.PartyId)
            });
#endif
        }

        protected async UniTaskVoid CreateGuild(RequestHandlerData requestHandler, DbRequestMessage<CreateGuildReq> request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            if (_insertingGuildNames.Contains(request.Data.GuildName))
            {
                result.InvokeError(new GuildResp());
                return;
            }
            _insertingGuildNames.Add(request.Data.GuildName);
            long foundAmount = await FindGuildName(request.Data.GuildName);
            if (foundAmount > 0)
            {
                _insertingGuildNames.TryRemove(request.Data.GuildName);
                result.InvokeError(new GuildResp());
                return;
            }
            // Insert to database
            int guildId = await Database.CreateGuild(request.Data.GuildName, request.Data.LeaderCharacterId);
            GuildData guild = new GuildData(guildId, request.Data.GuildName, request.Data.LeaderCharacterId, GuildMemberRoles);
            // Cache the data, it will be used later
            await UniTask.WhenAll(
                DatabaseCache.SetGuild(guild),
                DatabaseCache.SetPlayerCharacterGuildIdAndRole(request.Data.LeaderCharacterId, guildId, 0),
                DatabaseCache.SetSocialCharacterGuildIdAndRole(request.Data.LeaderCharacterId, guildId, 0));
            _insertingGuildNames.TryRemove(request.Data.GuildName);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid UpdateGuildLeader(RequestHandlerData requestHandler, DbRequestMessage<UpdateGuildLeaderReq> request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            GuildData guild = await GetGuild(request.Data.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            // Update to database
            await Database.UpdateGuildLeader(request.Data.GuildId, request.Data.LeaderCharacterId);
            // Update to cache
            guild.SetLeader(request.Data.LeaderCharacterId);
            await DatabaseCache.SetGuild(guild);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid UpdateGuildMessage(RequestHandlerData requestHandler, DbRequestMessage<UpdateGuildMessageReq> request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            GuildData guild = await GetGuild(request.Data.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            // Update to database
            await Database.UpdateGuildMessage(request.Data.GuildId, request.Data.GuildMessage);
            // Update to cache
            guild.guildMessage = request.Data.GuildMessage;
            await DatabaseCache.SetGuild(guild);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid UpdateGuildMessage2(RequestHandlerData requestHandler, DbRequestMessage<UpdateGuildMessageReq> request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            GuildData guild = await GetGuild(request.Data.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            // Update to database
            await Database.UpdateGuildMessage2(request.Data.GuildId, request.Data.GuildMessage);
            // Update to cache
            guild.guildMessage2 = request.Data.GuildMessage;
            await DatabaseCache.SetGuild(guild);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid UpdateGuildScore(RequestHandlerData requestHandler, DbRequestMessage<UpdateGuildScoreReq> request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            GuildData guild = await GetGuild(request.Data.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            // Update to database
            await Database.UpdateGuildScore(request.Data.GuildId, request.Data.Score);
            // Update to cache
            guild.score = request.Data.Score;
            await DatabaseCache.SetGuild(guild);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid UpdateGuildOptions(RequestHandlerData requestHandler, DbRequestMessage<UpdateGuildOptionsReq> request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            GuildData guild = await GetGuild(request.Data.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            // Update to database
            await Database.UpdateGuildOptions(request.Data.GuildId, request.Data.Options);
            // Update to cache
            guild.options = request.Data.Options;
            await DatabaseCache.SetGuild(guild);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid UpdateGuildAutoAcceptRequests(RequestHandlerData requestHandler, DbRequestMessage<UpdateGuildAutoAcceptRequestsReq> request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            GuildData guild = await GetGuild(request.Data.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            // Update to database
            await Database.UpdateGuildAutoAcceptRequests(request.Data.GuildId, request.Data.AutoAcceptRequests);
            // Update to cache
            guild.autoAcceptRequests = request.Data.AutoAcceptRequests;
            await DatabaseCache.SetGuild(guild);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid UpdateGuildRank(RequestHandlerData requestHandler, DbRequestMessage<UpdateGuildRankReq> request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            GuildData guild = await GetGuild(request.Data.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            // Update to database
            await Database.UpdateGuildRank(request.Data.GuildId, request.Data.Rank);
            // Update to cache
            guild.score = request.Data.Rank;
            await DatabaseCache.SetGuild(guild);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid UpdateGuildRole(RequestHandlerData requestHandler, DbRequestMessage<UpdateGuildRoleReq> request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            GuildData guild = await GetGuild(request.Data.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            // Update to database
            await Database.UpdateGuildRole(request.Data.GuildId, request.Data.GuildRole, request.Data.GuildRoleData);
            // Update to cache
            guild.SetRole(request.Data.GuildRole, request.Data.GuildRoleData);
            await DatabaseCache.SetGuild(guild);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid UpdateGuildMemberRole(RequestHandlerData requestHandler, DbRequestMessage<UpdateGuildMemberRoleReq> request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            GuildData guild = await GetGuild(request.Data.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            // Update to database
            await Database.UpdateGuildMemberRole(request.Data.MemberCharacterId, request.Data.GuildRole);
            // Update to cache
            guild.SetMemberRole(request.Data.MemberCharacterId, request.Data.GuildRole);
            await DatabaseCache.SetGuild(guild);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid DeleteGuild(RequestHandlerData requestHandler, DbRequestMessage<DeleteGuildReq> request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            // Remove data from database
            await Database.DeleteGuild(request.Data.GuildId);
            // Remove data from cache
            await DatabaseCache.RemoveGuild(request.Data.GuildId);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid UpdateCharacterGuild(RequestHandlerData requestHandler, DbRequestMessage<UpdateCharacterGuildReq> request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            GuildData guild = await GetGuild(request.Data.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            SocialCharacterData character = request.Data.SocialCharacterData;
            // Update to database
            await Database.UpdateCharacterGuild(character.id, request.Data.GuildId, request.Data.GuildRole);
            // Update to cache
            guild.AddMember(character, request.Data.GuildRole);
            await UniTask.WhenAll(
                DatabaseCache.SetGuild(guild),
                DatabaseCache.SetPlayerCharacterGuildIdAndRole(character.id, guild.id, request.Data.GuildRole),
                DatabaseCache.SetSocialCharacterGuildIdAndRole(character.id, guild.id, request.Data.GuildRole));
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid ClearCharacterGuild(RequestHandlerData requestHandler, DbRequestMessage<ClearCharacterGuildReq> request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            PlayerCharacterData character = await GetCharacter(request.Data.CharacterId);
            if (character == null)
            {
                result.InvokeSuccess(EmptyMessage.Value);
                return;
            }
            GuildData guild = await GetGuild(character.GuildId);
            if (guild == null)
            {
                result.InvokeSuccess(EmptyMessage.Value);
                return;
            }
            // Update to database
            await Database.UpdateCharacterGuild(request.Data.CharacterId, 0, 0);
            // Update to cache
            guild.RemoveMember(request.Data.CharacterId);
            await UniTask.WhenAll(
                DatabaseCache.SetGuild(guild),
                DatabaseCache.SetPlayerCharacterGuildIdAndRole(request.Data.CharacterId, 0, 0),
                DatabaseCache.SetSocialCharacterGuildIdAndRole(request.Data.CharacterId, 0, 0));
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid FindGuildName(RequestHandlerData requestHandler, DbRequestMessage<FindGuildNameReq> request, RequestProceedResultDelegate<FindGuildNameResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new FindGuildNameResp()
            {
                FoundAmount = await FindGuildName(request.Data.GuildName),
            });
#endif
        }

        protected async UniTaskVoid GetGuild(RequestHandlerData requestHandler, DbRequestMessage<GetGuildReq> request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            if (request.Data.ForceClearCache)
                await DatabaseCache.RemoveGuild(request.Data.GuildId);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = await GetGuild(request.Data.GuildId)
            });
#endif
        }

        protected async UniTaskVoid IncreaseGuildExp(RequestHandlerData requestHandler, DbRequestMessage<IncreaseGuildExpReq> request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            GuildData guild = await GetGuild(request.Data.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            guild.IncreaseGuildExp(GuildExpTree, request.Data.Exp);
            // Update to cache
            await DatabaseCache.SetGuild(guild);
            // Update to database
            await Database.UpdateGuildLevel(request.Data.GuildId, guild.level, guild.exp, guild.skillPoint);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = await GetGuild(request.Data.GuildId)
            });
#endif
        }

        protected async UniTaskVoid AddGuildSkill(RequestHandlerData requestHandler, DbRequestMessage<AddGuildSkillReq> request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            GuildData guild = await GetGuild(request.Data.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            if (!guild.AddSkillLevel(request.Data.SkillId))
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            // Update to database
            await Database.UpdateGuildSkillLevel(request.Data.GuildId, request.Data.SkillId, guild.GetSkillLevel(request.Data.SkillId), guild.skillPoint);
            // Update to cache
            await DatabaseCache.SetGuild(guild);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = await GetGuild(request.Data.GuildId)
            });
#endif
        }

        protected async UniTaskVoid GetGuildGold(RequestHandlerData requestHandler, DbRequestMessage<GetGuildGoldReq> request, RequestProceedResultDelegate<GuildGoldResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            GuildData guild = await GetGuild(request.Data.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildGoldResp()
                {
                    GuildGold = 0
                });
                return;
            }
            result.InvokeSuccess(new GuildGoldResp()
            {
                GuildGold = guild.gold
            });
#endif
        }

        protected async UniTaskVoid ChangeGuildGold(RequestHandlerData requestHandler, DbRequestMessage<ChangeGuildGoldReq> request, RequestProceedResultDelegate<GuildGoldResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            // Update data to database
            int changedGuildGold = await Database.ChangeGuildGold(request.Data.GuildId, request.Data.ChangeAmount);
            // Cache the data, it will be used later
            DatabaseCacheResult<GuildData> getGuildResult = await DatabaseCache.GetGuild(request.Data.GuildId);
            if (getGuildResult.HasValue)
            {
                GuildData guildData = getGuildResult.Value;
                guildData.gold = changedGuildGold;
                await DatabaseCache.SetGuild(guildData);
            }
            result.InvokeSuccess(new GuildGoldResp()
            {
                GuildGold = changedGuildGold,
            });
#endif
        }

        protected async UniTaskVoid GetStorageItems(RequestHandlerData requestHandler, DbRequestMessage<GetStorageItemsReq> request, RequestProceedResultDelegate<GetStorageItemsResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            if (request.Data.StorageType == StorageType.Guild)
            {
                if (await Database.FindReservedStorage(request.Data.StorageType, request.Data.StorageOwnerId) > 0)
                {
                    result.InvokeError(new GetStorageItemsResp()
                    {
                        Error = UITextKeys.UI_ERROR_OTHER_GUILD_MEMBER_ACCESSING_STORAGE,
                    });
                    return;
                }
                await Database.UpdateReservedStorage(request.Data.StorageType, request.Data.StorageOwnerId, request.Data.ReserverId);
            }
            result.InvokeSuccess(new GetStorageItemsResp()
            {
                StorageItems = await GetStorageItems(request.Data.StorageType, request.Data.StorageOwnerId),
            });
#endif
        }

        protected async UniTaskVoid UpdateStorageItems(RequestHandlerData requestHandler, DbRequestMessage<UpdateStorageItemsReq> request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            if (request.Data.DeleteStorageReservation)
            {
                // Delete reserver
                await Database.DeleteReservedStorage(request.Data.StorageType, request.Data.StorageOwnerId);
            }
            // Update to database
            await Database.UpdateStorageItems(request.Data.StorageType, request.Data.StorageOwnerId, request.Data.StorageItems);
            // Update to cache
            await DatabaseCache.SetStorageItems(request.Data.StorageType, request.Data.StorageOwnerId, request.Data.StorageItems);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid UpdateStorageAndCharacterItems(RequestHandlerData requestHandler, DbRequestMessage<UpdateStorageAndCharacterItemsReq> request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            if (request.Data.DeleteStorageReservation)
            {
                // Delete reserver
                await Database.DeleteReservedStorage(request.Data.StorageType, request.Data.StorageOwnerId);
            }
            // Update to database
            await Database.UpdateStorageAndCharacterItems(
                request.Data.StorageType,
                request.Data.StorageOwnerId,
                request.Data.StorageItems,
                request.Data.CharacterId,
                request.Data.SelectableWeaponSets,
                request.Data.EquipItems,
                request.Data.NonEquipItems);
            // Update to cache
            await UniTask.WhenAll(
                DatabaseCache.SetStorageItems(request.Data.StorageType, request.Data.StorageOwnerId, request.Data.StorageItems),
                DatabaseCache.SetPlayerCharacterSelectableWeaponSets(request.Data.CharacterId, request.Data.SelectableWeaponSets),
                DatabaseCache.SetPlayerCharacterEquipItems(request.Data.CharacterId, request.Data.EquipItems),
                DatabaseCache.SetPlayerCharacterNonEquipItems(request.Data.CharacterId, request.Data.NonEquipItems));
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid DeleteAllReservedStorage(RequestHandlerData requestHandler, DbRequestMessage<EmptyMessage> request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            await Database.DeleteAllReservedStorage();
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid MailList(RequestHandlerData requestHandler, DbRequestMessage<MailListReq> request, RequestProceedResultDelegate<MailListResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new MailListResp()
            {
                List = await Database.MailList(request.Data.UserId, request.Data.OnlyNewMails)
            });
#endif
        }

        protected async UniTaskVoid UpdateReadMailState(RequestHandlerData requestHandler, DbRequestMessage<UpdateReadMailStateReq> request, RequestProceedResultDelegate<UpdateReadMailStateResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            long updated = await Database.UpdateReadMailState(request.Data.MailId, request.Data.UserId);
            if (updated <= 0)
            {
                result.InvokeError(new UpdateReadMailStateResp()
                {
                    Error = UITextKeys.UI_ERROR_MAIL_READ_NOT_ALLOWED
                });
                return;
            }
            result.InvokeSuccess(new UpdateReadMailStateResp()
            {
                Mail = await Database.GetMail(request.Data.MailId, request.Data.UserId)
            });
#endif
        }

        protected async UniTaskVoid UpdateClaimMailItemsState(RequestHandlerData requestHandler, DbRequestMessage<UpdateClaimMailItemsStateReq> request, RequestProceedResultDelegate<UpdateClaimMailItemsStateResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            long updated = await Database.UpdateClaimMailItemsState(request.Data.MailId, request.Data.UserId);
            if (updated <= 0)
            {
                result.InvokeError(new UpdateClaimMailItemsStateResp()
                {
                    Error = UITextKeys.UI_ERROR_MAIL_CLAIM_NOT_ALLOWED
                });
                return;
            }
            result.InvokeSuccess(new UpdateClaimMailItemsStateResp()
            {
                Mail = await Database.GetMail(request.Data.MailId, request.Data.UserId)
            });
#endif
        }

        protected async UniTaskVoid UpdateDeleteMailState(RequestHandlerData requestHandler, DbRequestMessage<UpdateDeleteMailStateReq> request, RequestProceedResultDelegate<UpdateDeleteMailStateResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            long updated = await Database.UpdateDeleteMailState(request.Data.MailId, request.Data.UserId);
            if (updated <= 0)
            {
                result.InvokeError(new UpdateDeleteMailStateResp()
                {
                    Error = UITextKeys.UI_ERROR_MAIL_DELETE_NOT_ALLOWED
                });
                return;
            }
            result.InvokeSuccess(new UpdateDeleteMailStateResp());
#endif
        }

        protected async UniTaskVoid SendMail(RequestHandlerData requestHandler, DbRequestMessage<SendMailReq> request, RequestProceedResultDelegate<SendMailResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            Mail mail = request.Data.Mail;
            if (string.IsNullOrEmpty(mail.ReceiverId))
            {
                result.InvokeError(new SendMailResp()
                {
                    Error = UITextKeys.UI_ERROR_MAIL_SEND_NO_RECEIVER
                });
                return;
            }
            long created = await Database.CreateMail(mail);
            if (created <= 0)
            {
                result.InvokeError(new SendMailResp()
                {
                    Error = UITextKeys.UI_ERROR_MAIL_SEND_NOT_ALLOWED
                });
                return;
            }
            result.InvokeSuccess(new SendMailResp());
#endif
        }

        protected async UniTaskVoid GetMail(RequestHandlerData requestHandler, DbRequestMessage<GetMailReq> request, RequestProceedResultDelegate<GetMailResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new GetMailResp()
            {
                Mail = await Database.GetMail(request.Data.MailId, request.Data.UserId),
            });
#endif
        }

        protected async UniTaskVoid GetMailNotification(RequestHandlerData requestHandler, DbRequestMessage<GetMailNotificationReq> request, RequestProceedResultDelegate<GetMailNotificationResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new GetMailNotificationResp()
            {
                NotificationCount = await Database.GetMailNotification(request.Data.UserId),
            });
#endif
        }

        protected async UniTaskVoid GetIdByCharacterName(RequestHandlerData requestHandler, DbRequestMessage<GetIdByCharacterNameReq> request, RequestProceedResultDelegate<GetIdByCharacterNameResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new GetIdByCharacterNameResp()
            {
                Id = await Database.GetIdByCharacterName(request.Data.CharacterName),
            });
#endif
        }

        protected async UniTaskVoid GetUserIdByCharacterName(RequestHandlerData requestHandler, DbRequestMessage<GetUserIdByCharacterNameReq> request, RequestProceedResultDelegate<GetUserIdByCharacterNameResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new GetUserIdByCharacterNameResp()
            {
                UserId = await Database.GetUserIdByCharacterName(request.Data.CharacterName),
            });
#endif
        }

        protected async UniTaskVoid GetUserUnbanTime(RequestHandlerData requestHandler, DbRequestMessage<GetUserUnbanTimeReq> request, RequestProceedResultDelegate<GetUserUnbanTimeResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            long unbanTime = await Database.GetUserUnbanTime(request.Data.UserId);
            result.InvokeSuccess(new GetUserUnbanTimeResp()
            {
                UnbanTime = unbanTime,
            });
#endif
        }

        protected async UniTaskVoid SetUserUnbanTimeByCharacterName(RequestHandlerData requestHandler, DbRequestMessage<SetUserUnbanTimeByCharacterNameReq> request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            await Database.SetUserUnbanTimeByCharacterName(request.Data.CharacterName, request.Data.UnbanTime);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid SetCharacterUnmuteTimeByName(RequestHandlerData requestHandler, DbRequestMessage<SetCharacterUnmuteTimeByNameReq> request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            await Database.SetCharacterUnmuteTimeByName(request.Data.CharacterName, request.Data.UnmuteTime);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid GetSummonBuffs(RequestHandlerData requestHandler, DbRequestMessage<GetSummonBuffsReq> request, RequestProceedResultDelegate<GetSummonBuffsResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new GetSummonBuffsResp()
            {
                SummonBuffs = await Database.GetSummonBuffs(request.Data.CharacterId),
            });
#endif
        }

        protected async UniTaskVoid FindEmail(RequestHandlerData requestHandler, DbRequestMessage<FindEmailReq> request, RequestProceedResultDelegate<FindEmailResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new FindEmailResp()
            {
                FoundAmount = await FindEmail(request.Data.Email),
            });
#endif
        }

        protected async UniTaskVoid ValidateEmailVerification(RequestHandlerData requestHandler, DbRequestMessage<ValidateEmailVerificationReq> request, RequestProceedResultDelegate<ValidateEmailVerificationResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new ValidateEmailVerificationResp()
            {
                IsPass = await Database.ValidateEmailVerification(request.Data.UserId),
            });
#endif
        }

        protected async UniTaskVoid GetFriendRequestNotification(RequestHandlerData requestHandler, DbRequestMessage<GetFriendRequestNotificationReq> request, RequestProceedResultDelegate<GetFriendRequestNotificationResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new GetFriendRequestNotificationResp()
            {
                NotificationCount = await Database.GetFriendRequestNotification(request.Data.CharacterId),
            });
#endif
        }

        protected async UniTaskVoid UpdateUserCount(RequestHandlerData requestHandler, DbRequestMessage<UpdateUserCountReq> request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            await Database.UpdateUserCount(request.Data.UserCount);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid GetSocialCharacter(RequestHandlerData requestHandler, DbRequestMessage<GetSocialCharacterReq> request, RequestProceedResultDelegate<SocialCharacterResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new SocialCharacterResp()
            {
                SocialCharacterData = await GetSocialCharacter(request.Data.CharacterId),
            });
#endif
        }

        protected async UniTaskVoid FindGuilds(RequestHandlerData requestHandler, DbRequestMessage<FindGuildNameReq> request, RequestProceedResultDelegate<GuildsResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new GuildsResp()
            {
                List = await Database.FindGuilds(request.Data.FinderId, request.Data.GuildName, request.Data.Skip, request.Data.Limit)
            });
#endif
        }

        protected async UniTaskVoid CreateGuildRequest(RequestHandlerData requestHandler, DbRequestMessage<CreateGuildRequestReq> request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            await Database.CreateGuildRequest(request.Data.GuildId, request.Data.RequesterId);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid DeleteGuildRequest(RequestHandlerData requestHandler, DbRequestMessage<DeleteGuildRequestReq> request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            await Database.DeleteGuildRequest(request.Data.GuildId, request.Data.RequesterId);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid GetGuildRequests(RequestHandlerData requestHandler, DbRequestMessage<GetGuildRequestsReq> request, RequestProceedResultDelegate<SocialCharactersResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new SocialCharactersResp()
            {
                List = await Database.GetGuildRequests(request.Data.GuildId, request.Data.Skip, request.Data.Limit)
            });
#endif
        }

        protected async UniTaskVoid GetGuildRequestNotification(RequestHandlerData requestHandler, DbRequestMessage<GetGuildRequestNotificationReq> request, RequestProceedResultDelegate<GetGuildRequestNotificationResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            result.InvokeSuccess(new GetGuildRequestNotificationResp()
            {
                NotificationCount = await Database.GetGuildRequestsNotification(request.Data.GuildId),
            });
#endif
        }

        protected async UniTaskVoid UpdateGuildMemberCount(RequestHandlerData requestHandler, DbRequestMessage<UpdateGuildMemberCountReq> request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            await Database.UpdateGuildMemberCount(request.Data.GuildId, request.Data.MaxGuildMember);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        protected async UniTask<bool> ValidateAccessToken(string userId, string accessToken)
        {
            return await Database.ValidateAccessToken(userId, accessToken);
        }

        protected async UniTask<long> FindUsername(string username)
        {
            return await Database.FindUsername(username);
        }

        protected async UniTask<long> FindCharacterName(string characterName)
        {
            return await Database.FindCharacterName(characterName);
        }

        protected async UniTask<long> FindGuildName(string guildName)
        {
            return await Database.FindGuildName(guildName);
        }

        protected async UniTask<long> FindEmail(string email)
        {
            return await Database.FindEmail(email);
        }

        protected async UniTask<List<BuildingSaveData>> GetBuildings(string channel, string mapName)
        {
            // Get buildings from cache
            var buildingsResult = await DatabaseCache.GetBuildings(channel, mapName);
            if (buildingsResult.HasValue)
                return new List<BuildingSaveData>(buildingsResult.Value);
            // Read buildings from database
            List<BuildingSaveData> buildings = await Database.GetBuildings(channel, mapName);
            if (buildings == null)
                buildings = new List<BuildingSaveData>();
            // Store buildings to cache
            await DatabaseCache.SetBuildings(channel, mapName, buildings);
            return buildings;
        }

        protected async UniTask<int> GetGold(string userId)
        {
            return await Database.GetGold(userId);
        }

        protected async UniTask<int> GetCash(string userId)
        {
            return await Database.GetCash(userId);
        }

        protected async UniTask<PlayerCharacterData> GetCharacter(string id)
        {
            // Get character from cache
            var characterResult = await DatabaseCache.GetPlayerCharacter(id);
            if (characterResult.HasValue)
            {
                return characterResult.Value;
            }
            // Read character from database
            PlayerCharacterData character = await Database.GetCharacter(id);
            if (character != null)
            {
                // Store character to cache
                await DatabaseCache.SetPlayerCharacter(character);
            }
            return character;
        }

        protected async UniTask<PlayerCharacterData> GetCharacterWithUserIdValidation(string id, string userId)
        {
            PlayerCharacterData character = await GetCharacter(id);
            if (character != null && character.UserId != userId)
                character = null;
            return character;
        }

        protected async UniTask<SocialCharacterData> GetSocialCharacter(string id)
        {
            // Get character from cache
            var characterResult = await DatabaseCache.GetSocialCharacter(id);
            if (characterResult.HasValue)
                return characterResult.Value;
            // Read character from database
            SocialCharacterData character = SocialCharacterData.Create(await Database.GetCharacter(id, false, false, false, false, false, false, false, false, false, false, false));
            // Store character to cache
            await DatabaseCache.SetSocialCharacter(character);
            return character;
        }

        protected async UniTask<PartyData> GetParty(int id)
        {
            // Get party from cache
            var partyResult = await DatabaseCache.GetParty(id);
            if (partyResult.HasValue)
                return partyResult.Value;
            // Read party from database
            PartyData party = await Database.GetParty(id);
            if (party != null)
            {
                // Store party to cache
                await UniTask.WhenAll(
                    DatabaseCache.SetParty(party),
                    CacheSocialCharacters(party.GetMembers()));
            }
            return party;
        }

        protected async UniTask<GuildData> GetGuild(int id)
        {
            // Get guild from cache
            var guildResult = await DatabaseCache.GetGuild(id);
            if (guildResult.HasValue)
                return guildResult.Value;
            // Read guild from database
            GuildData guild = await Database.GetGuild(id, GuildMemberRoles);
            if (guild != null)
            {
                // Store guild to cache
                await UniTask.WhenAll(
                    DatabaseCache.SetGuild(guild),
                    CacheSocialCharacters(guild.GetMembers()));
            }
            return guild;
        }

        protected async UniTask<List<CharacterItem>> GetStorageItems(StorageType storageType, string storageOwnerId)
        {
            // Get storageItems from cache
            var storageItemsResult = await DatabaseCache.GetStorageItems(storageType, storageOwnerId);
            if (storageItemsResult.HasValue)
                return new List<CharacterItem>(storageItemsResult.Value);
            // Read storageItems from database
            List<CharacterItem> storageItems = await Database.GetStorageItems(storageType, storageOwnerId);
            if (storageItems == null)
                storageItems = new List<CharacterItem>();
            // Store storageItems to cache
            await DatabaseCache.SetStorageItems(storageType, storageOwnerId, storageItems);
            return storageItems;
        }

        protected async UniTask CacheSocialCharacters(SocialCharacterData[] socialCharacters)
        {
            UniTask<bool>[] tasks = new UniTask<bool>[socialCharacters.Length];
            for (int i = 0; i < socialCharacters.Length; ++i)
            {
                tasks[i] = DatabaseCache.SetSocialCharacter(socialCharacters[i]);
            }
            await UniTask.WhenAll(tasks);
        }
#endif
    }
}
