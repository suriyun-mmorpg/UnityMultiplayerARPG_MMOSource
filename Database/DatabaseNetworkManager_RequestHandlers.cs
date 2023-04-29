using ConcurrentCollections;
using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System;
using System.Collections.Concurrent;
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

        protected async UniTaskVoid ValidateUserLogin(RequestHandlerData requestHandler, ValidateUserLoginReq request, RequestProceedResultDelegate<ValidateUserLoginResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new ValidateUserLoginResp()
            {
                UserId = Database.ValidateUserLogin(request.Username, request.Password),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ValidateAccessToken(RequestHandlerData requestHandler, ValidateAccessTokenReq request, RequestProceedResultDelegate<ValidateAccessTokenResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new ValidateAccessTokenResp()
            {
                IsPass = await ValidateAccessToken(request.UserId, request.AccessToken),
            });
#endif
        }

        protected async UniTaskVoid GetUserLevel(RequestHandlerData requestHandler, GetUserLevelReq request, RequestProceedResultDelegate<GetUserLevelResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new GetUserLevelResp()
            {
                UserLevel = Database.GetUserLevel(request.UserId),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetGold(RequestHandlerData requestHandler, GetGoldReq request, RequestProceedResultDelegate<GoldResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new GoldResp()
            {
                Gold = await ReadGold(request.UserId)
            });
#endif
        }

        protected async UniTaskVoid ChangeGold(RequestHandlerData requestHandler, ChangeGoldReq request, RequestProceedResultDelegate<GoldResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            int gold = await ReadGold(request.UserId);
            gold += request.ChangeAmount;
            // Cache the data, it will be used later
            await DatabaseCache.SetUserGold(request.UserId, gold);
            // Update data to database
            Database.UpdateGold(request.UserId, gold);
            result.InvokeSuccess(new GoldResp()
            {
                Gold = gold
            });
#endif
        }

        protected async UniTaskVoid GetCash(RequestHandlerData requestHandler, GetCashReq request, RequestProceedResultDelegate<CashResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new CashResp()
            {
                Cash = await ReadCash(request.UserId)
            });
#endif
        }

        protected async UniTaskVoid ChangeCash(RequestHandlerData requestHandler, ChangeCashReq request, RequestProceedResultDelegate<CashResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            int cash = await ReadCash(request.UserId);
            cash += request.ChangeAmount;
            // Cache the data, it will be used later
            await DatabaseCache.SetUserCash(request.UserId, cash);
            // Update data to database
            Database.UpdateCash(request.UserId, cash);
            result.InvokeSuccess(new CashResp()
            {
                Cash = cash
            });
#endif
        }

        protected async UniTaskVoid UpdateAccessToken(RequestHandlerData requestHandler, UpdateAccessTokenReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            // Store access token to the dictionary, it will be used to validate later
            await DatabaseCache.SetUserAccessToken(request.UserId, request.AccessToken);
            // Update data to database
            Database.UpdateAccessToken(request.UserId, request.AccessToken);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid CreateUserLogin(RequestHandlerData requestHandler, CreateUserLoginReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            // Cache username, it will be used to validate later
            await DatabaseCache.AddUsername(request.Username);
            // Insert new user login to database
            Database.CreateUserLogin(request.Username, request.Password, request.Email);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid FindUsername(RequestHandlerData requestHandler, FindUsernameReq request, RequestProceedResultDelegate<FindUsernameResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new FindUsernameResp()
            {
                FoundAmount = await FindUsername(request.Username),
            });
#endif
        }

        protected async UniTaskVoid CreateCharacter(RequestHandlerData requestHandler, CreateCharacterReq request, RequestProceedResultDelegate<CharacterResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            PlayerCharacterData character = request.CharacterData;
            // Insert new character to database
            Database.CreateCharacter(request.UserId, character);
            result.InvokeSuccess(new CharacterResp()
            {
                CharacterData = character
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ReadCharacter(RequestHandlerData requestHandler, ReadCharacterReq request, RequestProceedResultDelegate<CharacterResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new CharacterResp()
            {
                CharacterData = await ReadCharacterWithUserIdValidation(request.CharacterId, request.UserId),
            });
#endif
        }

        protected async UniTaskVoid ReadCharacters(RequestHandlerData requestHandler, ReadCharactersReq request, RequestProceedResultDelegate<CharactersResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            List<PlayerCharacterData> characters = Database.ReadCharacters(request.UserId);
            // Read and cache character (or load from cache)
            long lastUpdate;
            for (int i = 0; i < characters.Count; ++i)
            {
                lastUpdate = characters[i].LastUpdate;
                characters[i] = await ReadCharacter(characters[i].Id);
                characters[i].LastUpdate = lastUpdate;
            }
            result.InvokeSuccess(new CharactersResp()
            {
                List = characters
            });
#endif
        }

        protected async UniTaskVoid UpdateCharacter(RequestHandlerData requestHandler, UpdateCharacterReq request, RequestProceedResultDelegate<CharacterResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            PlayerCharacterData character = request.CharacterData;
            // Update data to database
            Database.UpdateCharacter(character);
            // Cache the data, it will be used later
            await DatabaseCache.SetPlayerCharacter(character);
            result.InvokeSuccess(new CharacterResp()
            {
                CharacterData = character
            });
#endif
        }

        protected async UniTaskVoid DeleteCharacter(RequestHandlerData requestHandler, DeleteCharacterReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            // Remove data from cache
            var playerCharacter = await ReadCharacter(request.CharacterId);
            if (playerCharacter != null)
            {
                await DatabaseCache.RemoveCharacterName(playerCharacter.CharacterName);
                await DatabaseCache.RemovePlayerCharacter(playerCharacter.Id);
                await DatabaseCache.RemoveSocialCharacter(playerCharacter.Id);
            }
            // Delete data from database
            Database.DeleteCharacter(request.UserId, request.CharacterId);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid FindCharacterName(RequestHandlerData requestHandler, FindCharacterNameReq request, RequestProceedResultDelegate<FindCharacterNameResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new FindCharacterNameResp()
            {
                FoundAmount = await FindCharacterName(request.CharacterName),
            });
#endif
        }

        protected async UniTaskVoid FindCharacters(RequestHandlerData requestHandler, FindCharacterNameReq request, RequestProceedResultDelegate<SocialCharactersResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new SocialCharactersResp()
            {
                List = Database.FindCharacters(request.FinderId, request.CharacterName, request.Skip, request.Limit)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid CreateFriend(RequestHandlerData requestHandler, CreateFriendReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            Database.CreateFriend(request.Character1Id, request.Character2Id, request.State);
            result.InvokeSuccess(EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid DeleteFriend(RequestHandlerData requestHandler, DeleteFriendReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            Database.DeleteFriend(request.Character1Id, request.Character2Id);
            result.InvokeSuccess(EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ReadFriends(RequestHandlerData requestHandler, ReadFriendsReq request, RequestProceedResultDelegate<SocialCharactersResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new SocialCharactersResp()
            {
                List = Database.ReadFriends(request.CharacterId, request.ReadById2, request.State, request.Skip, request.Limit),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid CreateBuilding(RequestHandlerData requestHandler, CreateBuildingReq request, RequestProceedResultDelegate<BuildingResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            BuildingSaveData building = request.BuildingData;
            // Insert data to database
            Database.CreateBuilding(request.MapName, building);
            // Cache building data
            await DatabaseCache.SetBuilding(request.MapName, building);
            result.InvokeSuccess(new BuildingResp()
            {
                BuildingData = request.BuildingData
            });
#endif
        }

        protected async UniTaskVoid UpdateBuilding(RequestHandlerData requestHandler, UpdateBuildingReq request, RequestProceedResultDelegate<BuildingResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            BuildingSaveData building = request.BuildingData;
            // Update data to database
            Database.UpdateBuilding(request.MapName, building);
            // Cache building data
            await DatabaseCache.SetBuilding(request.MapName, building);
            result.InvokeSuccess(new BuildingResp()
            {
                BuildingData = request.BuildingData
            });
#endif
        }

        protected async UniTaskVoid DeleteBuilding(RequestHandlerData requestHandler, DeleteBuildingReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            // Remove data from cache
            await DatabaseCache.RemoveBuilding(request.MapName, request.BuildingId);
            // Remove data from database
            Database.DeleteBuilding(request.MapName, request.BuildingId);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid ReadBuildings(RequestHandlerData requestHandler, ReadBuildingsReq request, RequestProceedResultDelegate<BuildingsResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new BuildingsResp()
            {
                List = await ReadBuildings(request.MapName),
            });
#endif
        }

        protected async UniTaskVoid CreateParty(RequestHandlerData requestHandler, CreatePartyReq request, RequestProceedResultDelegate<PartyResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            // Insert to database
            int partyId = Database.CreateParty(request.ShareExp, request.ShareItem, request.LeaderCharacterId);
            PartyData party = new PartyData(partyId, request.ShareExp, request.ShareItem, request.LeaderCharacterId);
            // Cache the data, it will be used later
            await DatabaseCache.SetParty(party);
            result.InvokeSuccess(new PartyResp()
            {
                PartyData = party
            });
#endif
        }

        protected async UniTaskVoid UpdateParty(RequestHandlerData requestHandler, UpdatePartyReq request, RequestProceedResultDelegate<PartyResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            PartyData party = await ReadParty(request.PartyId);
            if (party == null)
            {
                result.InvokeError(new PartyResp()
                {
                    PartyData = null
                });
                return;
            }
            party.Setting(request.ShareExp, request.ShareItem);
            // Update to cache
            await DatabaseCache.SetParty(party);
            // Update to database
            Database.UpdateParty(request.PartyId, request.ShareExp, request.ShareItem);
            result.InvokeSuccess(new PartyResp()
            {
                PartyData = party
            });
#endif
        }

        protected async UniTaskVoid UpdatePartyLeader(RequestHandlerData requestHandler, UpdatePartyLeaderReq request, RequestProceedResultDelegate<PartyResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            PartyData party = await ReadParty(request.PartyId);
            if (party == null)
            {
                result.InvokeError(new PartyResp()
                {
                    PartyData = null
                });
                return;
            }
            party.SetLeader(request.LeaderCharacterId);
            // Update to cache
            await DatabaseCache.SetParty(party);
            // Update to database
            Database.UpdatePartyLeader(request.PartyId, request.LeaderCharacterId);
            result.InvokeSuccess(new PartyResp()
            {
                PartyData = party
            });
#endif
        }

        protected async UniTaskVoid DeleteParty(RequestHandlerData requestHandler, DeletePartyReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            // Remove data from cache
            await DatabaseCache.RemoveParty(request.PartyId);
            // Remove data from database
            Database.DeleteParty(request.PartyId);
            result.InvokeSuccess(EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateCharacterParty(RequestHandlerData requestHandler, UpdateCharacterPartyReq request, RequestProceedResultDelegate<PartyResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            PartyData party = await ReadParty(request.PartyId);
            if (party == null)
            {
                result.InvokeError(new PartyResp()
                {
                    PartyData = null
                });
                return;
            }
            SocialCharacterData character = request.SocialCharacterData;
            party.AddMember(character);
            // Update to cache
            await DatabaseCache.SetParty(party);
            await DatabaseCache.SetPlayerCharacterPartyId(character.id, party.id);
            await DatabaseCache.SetSocialCharacterPartyId(character.id, party.id);
            // Update to database
            Database.UpdateCharacterParty(character.id, request.PartyId);
            result.InvokeSuccess(new PartyResp()
            {
                PartyData = party
            });
#endif
        }

        protected async UniTaskVoid ClearCharacterParty(RequestHandlerData requestHandler, ClearCharacterPartyReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            PlayerCharacterData character = await ReadCharacter(request.CharacterId);
            if (character == null)
            {
                result.InvokeSuccess(EmptyMessage.Value);
                return;
            }
            PartyData party = await ReadParty(character.PartyId);
            if (party == null)
            {
                result.InvokeSuccess(EmptyMessage.Value);
                return;
            }
            party.RemoveMember(request.CharacterId);
            // Update to cache
            await DatabaseCache.SetParty(party);
            await DatabaseCache.SetPlayerCharacterPartyId(request.CharacterId, 0);
            await DatabaseCache.SetSocialCharacterPartyId(request.CharacterId, 0);
            // Update to database
            Database.UpdateCharacterParty(request.CharacterId, 0);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid ReadParty(RequestHandlerData requestHandler, ReadPartyReq request, RequestProceedResultDelegate<PartyResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new PartyResp()
            {
                PartyData = await ReadParty(request.PartyId)
            });
#endif
        }

        protected async UniTaskVoid CreateGuild(RequestHandlerData requestHandler, CreateGuildReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            // Insert to database
            int guildId = Database.CreateGuild(request.GuildName, request.LeaderCharacterId);
            GuildData guild = new GuildData(guildId, request.GuildName, request.LeaderCharacterId, GuildMemberRoles);
            // Cache the data, it will be used later
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateGuildLeader(RequestHandlerData requestHandler, UpdateGuildLeaderReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            GuildData guild = await ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            guild.SetLeader(request.LeaderCharacterId);
            // Update to cache
            await DatabaseCache.SetGuild(guild);
            // Update to database
            Database.UpdateGuildLeader(request.GuildId, request.LeaderCharacterId);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid UpdateGuildMessage(RequestHandlerData requestHandler, UpdateGuildMessageReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            GuildData guild = await ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            guild.guildMessage = request.GuildMessage;
            // Update to cache
            await DatabaseCache.SetGuild(guild);
            // Update to database
            Database.UpdateGuildMessage(request.GuildId, request.GuildMessage);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid UpdateGuildMessage2(RequestHandlerData requestHandler, UpdateGuildMessageReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            GuildData guild = await ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            guild.guildMessage2 = request.GuildMessage;
            // Update to cache
            await DatabaseCache.SetGuild(guild);
            // Update to database
            Database.UpdateGuildMessage2(request.GuildId, request.GuildMessage);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid UpdateGuildScore(RequestHandlerData requestHandler, UpdateGuildScoreReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            GuildData guild = await ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            guild.score = request.Score;
            // Update to cache
            await DatabaseCache.SetGuild(guild);
            // Update to database
            Database.UpdateGuildScore(request.GuildId, request.Score);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid UpdateGuildOptions(RequestHandlerData requestHandler, UpdateGuildOptionsReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            GuildData guild = await ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            guild.options = request.Options;
            // Update to cache
            await DatabaseCache.SetGuild(guild);
            // Update to database
            Database.UpdateGuildOptions(request.GuildId, request.Options);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid UpdateGuildAutoAcceptRequests(RequestHandlerData requestHandler, UpdateGuildAutoAcceptRequestsReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            GuildData guild = await ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            guild.autoAcceptRequests = request.AutoAcceptRequests;
            // Update to cache
            await DatabaseCache.SetGuild(guild);
            // Update to database
            Database.UpdateGuildAutoAcceptRequests(request.GuildId, request.AutoAcceptRequests);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid UpdateGuildRank(RequestHandlerData requestHandler, UpdateGuildRankReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            GuildData guild = await ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            guild.score = request.Rank;
            // Update to cache
            await DatabaseCache.SetGuild(guild);
            // Update to database
            Database.UpdateGuildRank(request.GuildId, request.Rank);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid UpdateGuildRole(RequestHandlerData requestHandler, UpdateGuildRoleReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            GuildData guild = await ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            guild.SetRole(request.GuildRole, request.GuildRoleData);
            // Update to cache
            await DatabaseCache.SetGuild(guild);
            // Update to database
            Database.UpdateGuildRole(request.GuildId, request.GuildRole, request.GuildRoleData);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid UpdateGuildMemberRole(RequestHandlerData requestHandler, UpdateGuildMemberRoleReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            GuildData guild = await ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            guild.SetMemberRole(request.MemberCharacterId, request.GuildRole);
            // Update to cache
            await DatabaseCache.SetGuild(guild);
            // Update to database
            Database.UpdateGuildMemberRole(request.MemberCharacterId, request.GuildRole);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid DeleteGuild(RequestHandlerData requestHandler, DeleteGuildReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            // Remove data from cache
            var guild = await ReadGuild(request.GuildId);
            if (guild != null)
            {
                await DatabaseCache.RemoveGuildName(guild.guildName);
                await DatabaseCache.RemoveGuild(guild.id);
            }
            // Remove data from database
            Database.DeleteGuild(request.GuildId);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid UpdateCharacterGuild(RequestHandlerData requestHandler, UpdateCharacterGuildReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            GuildData guild = await ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            SocialCharacterData character = request.SocialCharacterData;
            guild.AddMember(character, request.GuildRole);
            // Update to cache
            await DatabaseCache.SetGuild(guild);
            await DatabaseCache.SetPlayerCharacterGuildIdAndRole(character.id, guild.id, request.GuildRole);
            await DatabaseCache.SetSocialCharacterGuildIdAndRole(character.id, guild.id, request.GuildRole);
            // Update to database
            Database.UpdateCharacterGuild(character.id, request.GuildId, request.GuildRole);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        protected async UniTaskVoid ClearCharacterGuild(RequestHandlerData requestHandler, ClearCharacterGuildReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            PlayerCharacterData character = await ReadCharacter(request.CharacterId);
            if (character == null)
            {
                result.InvokeSuccess(EmptyMessage.Value);
                return;
            }
            GuildData guild = await ReadGuild(character.GuildId);
            if (guild == null)
            {
                result.InvokeSuccess(EmptyMessage.Value);
                return;
            }
            // Update to cache
            guild.RemoveMember(request.CharacterId);
            // Update to cache
            await DatabaseCache.SetGuild(guild);
            await DatabaseCache.SetPlayerCharacterGuildIdAndRole(request.CharacterId, 0, 0);
            await DatabaseCache.SetSocialCharacterGuildIdAndRole(request.CharacterId, 0, 0);
            // Update to database
            Database.UpdateCharacterGuild(request.CharacterId, 0, 0);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid FindGuildName(RequestHandlerData requestHandler, FindGuildNameReq request, RequestProceedResultDelegate<FindGuildNameResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new FindGuildNameResp()
            {
                FoundAmount = await FindGuildName(request.GuildName),
            });
#endif
        }

        protected async UniTaskVoid ReadGuild(RequestHandlerData requestHandler, ReadGuildReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = await ReadGuild(request.GuildId)
            });
#endif
        }

        protected async UniTaskVoid IncreaseGuildExp(RequestHandlerData requestHandler, IncreaseGuildExpReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            // TODO: May validate guild by character
            GuildData guild = await ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            guild.IncreaseGuildExp(GuildExpTree, request.Exp);
            // Update to cache
            await DatabaseCache.SetGuild(guild);
            // Update to database
            Database.UpdateGuildLevel(request.GuildId, guild.level, guild.exp, guild.skillPoint);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = await ReadGuild(request.GuildId)
            });
#endif
        }

        protected async UniTaskVoid AddGuildSkill(RequestHandlerData requestHandler, AddGuildSkillReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            // TODO: May validate guild by character
            GuildData guild = await ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            guild.AddSkillLevel(request.SkillId);
            // Update to cache
            await DatabaseCache.SetGuild(guild);
            // Update to database
            Database.UpdateGuildSkillLevel(request.GuildId, request.SkillId, guild.GetSkillLevel(request.SkillId), guild.skillPoint);
            result.InvokeSuccess(new GuildResp()
            {
                GuildData = await ReadGuild(request.GuildId)
            });
#endif
        }

        protected async UniTaskVoid GetGuildGold(RequestHandlerData requestHandler, GetGuildGoldReq request, RequestProceedResultDelegate<GuildGoldResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            GuildData guild = await ReadGuild(request.GuildId);
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

        protected async UniTaskVoid ChangeGuildGold(RequestHandlerData requestHandler, ChangeGuildGoldReq request, RequestProceedResultDelegate<GuildGoldResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            GuildData guild = await ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.InvokeError(new GuildGoldResp()
                {
                    GuildGold = 0
                });
                return;
            }
            guild.gold += request.ChangeAmount;
            // Update to cache
            await DatabaseCache.SetGuild(guild);
            // Update to database
            Database.UpdateGuildGold(request.GuildId, guild.gold);
            result.InvokeSuccess(new GuildGoldResp()
            {
                GuildGold = guild.gold
            });
#endif
        }

        protected async UniTaskVoid ReadStorageItems(RequestHandlerData requestHandler, ReadStorageItemsReq request, RequestProceedResultDelegate<ReadStorageItemsResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            StorageId storageId = new StorageId(request.StorageType, request.StorageOwnerId);
            if (request.ReadForUpdate)
            {
                long time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                var updatedTime = await DatabaseCache.GetUpdatingStorage(storageId);
                if (updatedTime.HasValue && time - updatedTime.Value < 500)
                {
                    // Not allow to update yet
                    result.InvokeError(new ReadStorageItemsResp());
                }
                await DatabaseCache.SetUpdatingStorage(storageId, time);
            }
            result.InvokeSuccess(new ReadStorageItemsResp()
            {
                StorageCharacterItems = await ReadStorageItems(storageId),
            });
#endif
        }

        protected async UniTaskVoid UpdateStorageItems(RequestHandlerData requestHandler, UpdateStorageItemsReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)

            StorageId storageId = new StorageId(request.StorageType, request.StorageOwnerId);
            long time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var updatedTime = await DatabaseCache.GetUpdatingStorage(storageId);
            if (updatedTime.HasValue && time - updatedTime.Value >= 500)
            {
                // Timeout
                result.InvokeError(EmptyMessage.Value);
            }
            if (request.CharacterData != null)
            {
                PlayerCharacterData character = request.CharacterData;
                // Cache the data, it will be used later
                await DatabaseCache.SetPlayerCharacter(character);
                // Update data to database
                Database.UpdateCharacter(character);
            }
            // Cache the data, it will be used later
            await DatabaseCache.SetStorageItems(storageId, request.StorageItems);
            await DatabaseCache.RemoveUpdatingStorage(storageId);
            // Update data to database
            Database.UpdateStorageItems(request.StorageType, request.StorageOwnerId, request.StorageItems);
            result.InvokeSuccess(EmptyMessage.Value);
#endif
        }

        protected async UniTaskVoid MailList(RequestHandlerData requestHandler, MailListReq request, RequestProceedResultDelegate<MailListResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new MailListResp()
            {
                List = Database.MailList(request.UserId, request.OnlyNewMails)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateReadMailState(RequestHandlerData requestHandler, UpdateReadMailStateReq request, RequestProceedResultDelegate<UpdateReadMailStateResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            long updated = Database.UpdateReadMailState(request.MailId, request.UserId);
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
                Mail = Database.GetMail(request.MailId, request.UserId)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateClaimMailItemsState(RequestHandlerData requestHandler, UpdateClaimMailItemsStateReq request, RequestProceedResultDelegate<UpdateClaimMailItemsStateResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            long updated = Database.UpdateClaimMailItemsState(request.MailId, request.UserId);
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
                Mail = Database.GetMail(request.MailId, request.UserId)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateDeleteMailState(RequestHandlerData requestHandler, UpdateDeleteMailStateReq request, RequestProceedResultDelegate<UpdateDeleteMailStateResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            long updated = Database.UpdateDeleteMailState(request.MailId, request.UserId);
            if (updated <= 0)
            {
                result.InvokeError(new UpdateDeleteMailStateResp()
                {
                    Error = UITextKeys.UI_ERROR_MAIL_DELETE_NOT_ALLOWED
                });
                return;
            }
            result.InvokeSuccess(new UpdateDeleteMailStateResp());
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid SendMail(RequestHandlerData requestHandler, SendMailReq request, RequestProceedResultDelegate<SendMailResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            Mail mail = request.Mail;
            if (string.IsNullOrEmpty(mail.ReceiverId))
            {
                result.InvokeError(new SendMailResp()
                {
                    Error = UITextKeys.UI_ERROR_MAIL_SEND_NO_RECEIVER
                });
                return;
            }
            long created = Database.CreateMail(mail);
            if (created <= 0)
            {
                result.InvokeError(new SendMailResp()
                {
                    Error = UITextKeys.UI_ERROR_MAIL_SEND_NOT_ALLOWED
                });
                return;
            }
            result.InvokeSuccess(new SendMailResp());
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetMail(RequestHandlerData requestHandler, GetMailReq request, RequestProceedResultDelegate<GetMailResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new GetMailResp()
            {
                Mail = Database.GetMail(request.MailId, request.UserId),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetMailNotification(RequestHandlerData requestHandler, GetMailNotificationReq request, RequestProceedResultDelegate<GetMailNotificationResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new GetMailNotificationResp()
            {
                NotificationCount = Database.GetMailNotification(request.UserId),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetIdByCharacterName(RequestHandlerData requestHandler, GetIdByCharacterNameReq request, RequestProceedResultDelegate<GetIdByCharacterNameResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new GetIdByCharacterNameResp()
            {
                Id = Database.GetIdByCharacterName(request.CharacterName),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetUserIdByCharacterName(RequestHandlerData requestHandler, GetUserIdByCharacterNameReq request, RequestProceedResultDelegate<GetUserIdByCharacterNameResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new GetUserIdByCharacterNameResp()
            {
                UserId = Database.GetUserIdByCharacterName(request.CharacterName),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetUserUnbanTime(RequestHandlerData requestHandler, GetUserUnbanTimeReq request, RequestProceedResultDelegate<GetUserUnbanTimeResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            long unbanTime = Database.GetUserUnbanTime(request.UserId);
            result.InvokeSuccess(new GetUserUnbanTimeResp()
            {
                UnbanTime = unbanTime,
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid SetUserUnbanTimeByCharacterName(RequestHandlerData requestHandler, SetUserUnbanTimeByCharacterNameReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            Database.SetUserUnbanTimeByCharacterName(request.CharacterName, request.UnbanTime);
            result.InvokeSuccess(EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid SetCharacterUnmuteTimeByName(RequestHandlerData requestHandler, SetCharacterUnmuteTimeByNameReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            Database.SetCharacterUnmuteTimeByName(request.CharacterName, request.UnmuteTime);
            result.InvokeSuccess(EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetSummonBuffs(RequestHandlerData requestHandler, GetSummonBuffsReq request, RequestProceedResultDelegate<GetSummonBuffsResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new GetSummonBuffsResp()
            {
                SummonBuffs = Database.GetSummonBuffs(request.CharacterId),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid SetSummonBuffs(RequestHandlerData requestHandler, SetSummonBuffsReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            Database.SetSummonBuffs(request.CharacterId, request.SummonBuffs);
            result.InvokeSuccess(EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid FindEmail(RequestHandlerData requestHandler, FindEmailReq request, RequestProceedResultDelegate<FindEmailResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new FindEmailResp()
            {
                FoundAmount = await FindEmail(request.Email),
            });
#endif
        }

        protected async UniTaskVoid ValidateEmailVerification(RequestHandlerData requestHandler, ValidateEmailVerificationReq request, RequestProceedResultDelegate<ValidateEmailVerificationResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new ValidateEmailVerificationResp()
            {
                IsPass = Database.ValidateEmailVerification(request.UserId),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetFriendRequestNotification(RequestHandlerData requestHandler, GetFriendRequestNotificationReq request, RequestProceedResultDelegate<GetFriendRequestNotificationResp> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            result.InvokeSuccess(new GetFriendRequestNotificationResp()
            {
                NotificationCount = Database.GetFriendRequestNotification(request.CharacterId),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateUserCount(RequestHandlerData requestHandler, UpdateUserCountReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            Database.UpdateUserCount(request.UserCount);
            result.InvokeSuccess(EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)

        protected async UniTask<bool> ValidateAccessToken(string userId, string accessToken)
        {
            if (!DisableCacheReading)
            {
                // Already cached access token, so validate access token from cache
                var accessTokenResult = await DatabaseCache.GetUserAccessToken(userId);
                return accessTokenResult.HasValue && accessToken.Equals(accessTokenResult.Value);
            }
            // Doesn't cached yet, so try validate from database
            if (Database.ValidateAccessToken(userId, accessToken))
            {
                // Pass, store access token to the dictionary
                await DatabaseCache.SetUserAccessToken(userId, accessToken);
                return true;
            }
            return false;
        }

        protected async UniTask<long> FindUsername(string username)
        {
            long foundAmount;
            if (!DisableCacheReading && await DatabaseCache.ContainsUsername(username))
            {
                // Already cached username, so validate username from cache
                foundAmount = 1;
            }
            else
            {
                // Doesn't cached yet, so try validate from database
                foundAmount = Database.FindUsername(username);
                // Cache username, it will be used to validate later
                if (foundAmount > 0)
                    await DatabaseCache.AddUsername(username);
            }
            return foundAmount;
        }

        protected async UniTask<long> FindCharacterName(string characterName)
        {
            long foundAmount;
            if (!DisableCacheReading && await DatabaseCache.ContainsCharacterName(characterName))
            {
                // Already cached character name, so validate character name from cache
                foundAmount = 1;
            }
            else
            {
                // Doesn't cached yet, so try validate from database
                foundAmount = Database.FindCharacterName(characterName);
                // Cache character name, it will be used to validate later
                if (foundAmount > 0)
                    await DatabaseCache.AddCharacterName(characterName);
            }
            return foundAmount;
        }

        protected async UniTask<long> FindGuildName(string guildName)
        {
            long foundAmount;
            if (!DisableCacheReading && await DatabaseCache.ContainsGuildName(guildName))
            {
                // Already cached username, so validate username from cache
                foundAmount = 1;
            }
            else
            {
                // Doesn't cached yet, so try validate from database
                foundAmount = Database.FindGuildName(guildName);
                // Cache guild name, it will be used to validate later
                if (foundAmount > 0)
                    await DatabaseCache.AddGuildName(guildName);
            }
            return foundAmount;
        }

        protected async UniTask<long> FindEmail(string email)
        {
            long foundAmount;
            if (!DisableCacheReading && await DatabaseCache.ContainsEmail(email))
            {
                // Already cached username, so validate username from cache
                foundAmount = 1;
            }
            else
            {
                // Doesn't cached yet, so try validate from database
                foundAmount = Database.FindEmail(email);
                // Cache username, it will be used to validate later
                if (foundAmount > 0)
                    await DatabaseCache.AddEmail(email);
            }
            return foundAmount;
        }

        protected async UniTask<List<BuildingSaveData>> ReadBuildings(string mapName)
        {
            if (!DisableCacheReading)
            {
                // Get buildings from cache
                var buildingsResult = await DatabaseCache.GetBuildings(mapName);
                if (buildingsResult.HasValue)
                    return new List<BuildingSaveData>(buildingsResult.Value);
            }
            // Read buildings from database
            List<BuildingSaveData> buildings = Database.ReadBuildings(mapName);
            // Store buildings to cache
            await DatabaseCache.SetBuildings(mapName, buildings);
            return buildings;
        }

        protected async UniTask<int> ReadGold(string userId)
        {
            if (!DisableCacheReading)
            {
                // Get gold from cache
                var goldResult = await DatabaseCache.GetUserGold(userId);
                if (goldResult.HasValue)
                    return goldResult.Value;
            }
            // Read gold from database
            int gold = Database.GetGold(userId);
            // Store gold to cache
            await DatabaseCache.SetUserGold(userId, gold);
            return gold;
        }

        protected async UniTask<int> ReadCash(string userId)
        {
            if (!DisableCacheReading)
            {
                // Get cash from cache
                var cashResult = await DatabaseCache.GetUserCash(userId);
                if (cashResult.HasValue)
                    return cashResult.Value;
            }
            // Read cash from database
            int cash = Database.GetCash(userId);
            // Store cash to cache
            await DatabaseCache.SetUserCash(userId, cash);
            return cash;
        }

        protected async UniTask<PlayerCharacterData> ReadCharacter(string id)
        {
            if (!DisableCacheReading)
            {
                // Get character from cache
                var characterResult = await DatabaseCache.GetPlayerCharacter(id);
                if (characterResult.HasValue)
                    return characterResult.Value;
            }
            // Read character from database
            PlayerCharacterData character = Database.ReadCharacter(id);
            if (character != null)
            {
                // Store character to cache
                await DatabaseCache.SetPlayerCharacter(character);
            }
            return character;
        }

        protected async UniTask<PlayerCharacterData> ReadCharacterWithUserIdValidation(string id, string userId)
        {
            PlayerCharacterData character = await ReadCharacter(id);
            if (character != null && character.UserId != userId)
                character = null;
            return character;
        }

        protected async UniTask<SocialCharacterData> ReadSocialCharacter(string id)
        {
            if (!DisableCacheReading)
            {
                // Get character from cache
                var characterResult = await DatabaseCache.GetSocialCharacter(id);
                if (characterResult.HasValue)
                    return characterResult.Value;
            }
            // Read character from database
            SocialCharacterData character = SocialCharacterData.Create(Database.ReadCharacter(id, false, false, false, false, false, false, false, false, false, false, false));
            // Store character to cache
            await DatabaseCache.SetSocialCharacter(character);
            return character;
        }

        protected async UniTask<PartyData> ReadParty(int id)
        {
            if (!DisableCacheReading)
            {
                // Get party from cache
                var partyResult = await DatabaseCache.GetParty(id);
                if (partyResult.HasValue)
                    return partyResult.Value;
            }
            // Read party from database
            PartyData party = Database.ReadParty(id);
            if (party != null)
            {
                // Store party to cache
                await DatabaseCache.SetParty(party);
                await CacheSocialCharacters(party.GetMembers());
            }
            return party;
        }

        protected async UniTask<GuildData> ReadGuild(int id)
        {
            if (!DisableCacheReading)
            {
                // Get guild from cache
                var guildResult = await DatabaseCache.GetGuild(id);
                if (guildResult.HasValue)
                    return guildResult.Value;
            }
            // Read guild from database
            GuildData guild = Database.ReadGuild(id, GuildMemberRoles);
            if (guild != null)
            {
                // Store guild to cache
                await DatabaseCache.SetGuild(guild);
                await CacheSocialCharacters(guild.GetMembers());
            }
            return guild;
        }

        protected async UniTask<List<CharacterItem>> ReadStorageItems(StorageId storageId)
        {
            if (!DisableCacheReading)
            {
                // Get storageItems from cache
                var storageItemsResult = await DatabaseCache.GetStorageItems(storageId);
                if (storageItemsResult.HasValue)
                    return new List<CharacterItem>(storageItemsResult.Value);
            }
            // Read storageItems from database
            List<CharacterItem> storageItems = Database.ReadStorageItems(storageId.storageType, storageId.storageOwnerId);
            // Store storageItems to cache
            await DatabaseCache.SetStorageItems(storageId, storageItems);
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
