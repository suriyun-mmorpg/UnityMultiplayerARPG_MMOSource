using Cysharp.Threading.Tasks;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial class DatabaseNetworkManager : IDatabaseClient
    {
        private async UniTask<DatabaseApiResult<TResp>> SendRequest<TReq, TResp>(TReq request, ushort requestType, string functionName)
            where TReq : struct, INetSerializable
            where TResp : struct, INetSerializable
        {
            var resp = await Client.SendRequestAsync<DbRequestMessage<TReq>, TResp>(requestType, new DbRequestMessage<TReq>()
            {
                RequestTimeUtc = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Data = request,
            });
            if (!resp.IsSuccess)
            {
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {functionName} status: {resp.ResponseCode}");
                return new DatabaseApiResult<TResp>()
                {
                    IsError = true,
                    Response = resp.Response,
                };
            }
            return new DatabaseApiResult<TResp>()
            {
                Response = resp.Response,
            };
        }

        private async UniTask<DatabaseApiResult> SendRequest<TReq>(TReq request, ushort requestType, string functionName)
            where TReq : struct, INetSerializable
        {
            var resp = await Client.SendRequestAsync<DbRequestMessage<TReq>, EmptyMessage>(requestType, new DbRequestMessage<TReq>()
            {
                RequestTimeUtc = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Data = request,
            });
            if (!resp.IsSuccess)
            {
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {functionName} status: {resp.ResponseCode}");
                return new DatabaseApiResult()
                {
                    IsError = true,
                };
            }
            return new DatabaseApiResult();
        }

        public async UniTask<DatabaseApiResult<ValidateUserLoginResp>> ValidateUserLoginAsync(ValidateUserLoginReq request)
        {
            return await SendRequest<ValidateUserLoginReq, ValidateUserLoginResp>(request, DatabaseRequestTypes.ValidateUserLogin, nameof(ValidateUserLoginAsync));
        }

        public async UniTask<DatabaseApiResult<ValidateAccessTokenResp>> ValidateAccessTokenAsync(ValidateAccessTokenReq request)
        {
            return await SendRequest<ValidateAccessTokenReq, ValidateAccessTokenResp>(request, DatabaseRequestTypes.ValidateAccessToken, nameof(ValidateAccessTokenAsync));
        }

        public async UniTask<DatabaseApiResult<GetUserLevelResp>> GetUserLevelAsync(GetUserLevelReq request)
        {
            return await SendRequest<GetUserLevelReq, GetUserLevelResp>(request, DatabaseRequestTypes.GetUserLevel, nameof(GetUserLevelAsync));
        }

        public async UniTask<DatabaseApiResult<GoldResp>> GetGoldAsync(GetGoldReq request)
        {
            return await SendRequest<GetGoldReq, GoldResp>(request, DatabaseRequestTypes.GetGold, nameof(GetGoldAsync));
        }

        public async UniTask<DatabaseApiResult<GoldResp>> ChangeGoldAsync(ChangeGoldReq request)
        {
            return await SendRequest<ChangeGoldReq, GoldResp>(request, DatabaseRequestTypes.ChangeGold, nameof(ChangeGoldAsync));
        }

        public async UniTask<DatabaseApiResult<CashResp>> GetCashAsync(GetCashReq request)
        {
            return await SendRequest<GetCashReq, CashResp>(request, DatabaseRequestTypes.GetCash, nameof(GetCashAsync));
        }

        public async UniTask<DatabaseApiResult<CashResp>> ChangeCashAsync(ChangeCashReq request)
        {
            return await SendRequest<ChangeCashReq, CashResp>(request, DatabaseRequestTypes.ChangeCash, nameof(ChangeCashAsync));
        }

        public async UniTask<DatabaseApiResult> UpdateAccessTokenAsync(UpdateAccessTokenReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.UpdateAccessToken, nameof(UpdateAccessTokenAsync));
        }

        public async UniTask<DatabaseApiResult> CreateUserLoginAsync(CreateUserLoginReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.CreateUserLogin, nameof(CreateUserLoginAsync));
        }

        public async UniTask<DatabaseApiResult<FindUsernameResp>> FindUsernameAsync(FindUsernameReq request)
        {
            return await SendRequest<FindUsernameReq, FindUsernameResp>(request, DatabaseRequestTypes.FindUsername, nameof(FindUsernameAsync));
        }

        public async UniTask<DatabaseApiResult<CharacterResp>> CreateCharacterAsync(CreateCharacterReq request)
        {
            return await SendRequest<CreateCharacterReq, CharacterResp>(request, DatabaseRequestTypes.CreateCharacter, nameof(CreateCharacterAsync));
        }

        public async UniTask<DatabaseApiResult<CharacterResp>> GetCharacterAsync(GetCharacterReq request)
        {
            return await SendRequest<GetCharacterReq, CharacterResp>(request, DatabaseRequestTypes.GetCharacter, nameof(GetCharacterAsync));
        }

        public async UniTask<DatabaseApiResult<CharactersResp>> GetCharactersAsync(GetCharactersReq request)
        {
            return await SendRequest<GetCharactersReq, CharactersResp>(request, DatabaseRequestTypes.GetCharacters, nameof(GetCharactersAsync));
        }

        public async UniTask<DatabaseApiResult<CharacterResp>> UpdateCharacterAsync(UpdateCharacterReq request)
        {
            return await SendRequest<UpdateCharacterReq, CharacterResp>(request, DatabaseRequestTypes.UpdateCharacter, nameof(UpdateCharacterAsync));
        }

        public async UniTask<DatabaseApiResult> DeleteCharacterAsync(DeleteCharacterReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.DeleteCharacter, nameof(DeleteCharacterAsync));
        }

        public async UniTask<DatabaseApiResult<FindCharacterNameResp>> FindCharacterNameAsync(FindCharacterNameReq request)
        {
            return await SendRequest<FindCharacterNameReq, FindCharacterNameResp>(request, DatabaseRequestTypes.FindCharacterName, nameof(FindCharacterNameAsync));
        }

        public async UniTask<DatabaseApiResult<SocialCharactersResp>> FindCharactersAsync(FindCharacterNameReq request)
        {
            return await SendRequest<FindCharacterNameReq, SocialCharactersResp>(request, DatabaseRequestTypes.FindCharacters, nameof(FindCharactersAsync));
        }

        public async UniTask<DatabaseApiResult> CreateFriendAsync(CreateFriendReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.CreateFriend, nameof(CreateFriendAsync));
        }

        public async UniTask<DatabaseApiResult> DeleteFriendAsync(DeleteFriendReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.DeleteFriend, nameof(DeleteFriendAsync));
        }

        public async UniTask<DatabaseApiResult<SocialCharactersResp>> GetFriendsAsync(GetFriendsReq request)
        {
            return await SendRequest<GetFriendsReq, SocialCharactersResp>(request, DatabaseRequestTypes.GetFriends, nameof(GetFriendsAsync));
        }

        public async UniTask<DatabaseApiResult<BuildingResp>> CreateBuildingAsync(CreateBuildingReq request)
        {
            return await SendRequest<CreateBuildingReq, BuildingResp>(request, DatabaseRequestTypes.CreateBuilding, nameof(CreateBuildingAsync));
        }

        public async UniTask<DatabaseApiResult<BuildingResp>> UpdateBuildingAsync(UpdateBuildingReq request)
        {
            return await SendRequest<UpdateBuildingReq, BuildingResp>(request, DatabaseRequestTypes.UpdateBuilding, nameof(UpdateBuildingAsync));
        }

        public async UniTask<DatabaseApiResult> DeleteBuildingAsync(DeleteBuildingReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.DeleteBuilding, nameof(DeleteBuildingAsync));
        }

        public async UniTask<DatabaseApiResult<BuildingsResp>> GetBuildingsAsync(GetBuildingsReq request)
        {
            return await SendRequest<GetBuildingsReq, BuildingsResp>(request, DatabaseRequestTypes.GetBuildings, nameof(GetBuildingsAsync));
        }

        public async UniTask<DatabaseApiResult<PartyResp>> CreatePartyAsync(CreatePartyReq request)
        {
            return await SendRequest<CreatePartyReq, PartyResp>(request, DatabaseRequestTypes.CreateParty, nameof(CreatePartyAsync));
        }

        public async UniTask<DatabaseApiResult<PartyResp>> UpdatePartyAsync(UpdatePartyReq request)
        {
            return await SendRequest<UpdatePartyReq, PartyResp>(request, DatabaseRequestTypes.UpdateParty, nameof(UpdatePartyAsync));
        }

        public async UniTask<DatabaseApiResult<PartyResp>> UpdatePartyLeaderAsync(UpdatePartyLeaderReq request)
        {
            return await SendRequest<UpdatePartyLeaderReq, PartyResp>(request, DatabaseRequestTypes.UpdatePartyLeader, nameof(UpdatePartyLeaderAsync));
        }

        public async UniTask<DatabaseApiResult> DeletePartyAsync(DeletePartyReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.DeleteParty, nameof(DeletePartyAsync));
        }

        public async UniTask<DatabaseApiResult<PartyResp>> UpdateCharacterPartyAsync(UpdateCharacterPartyReq request)
        {
            return await SendRequest<UpdateCharacterPartyReq, PartyResp>(request, DatabaseRequestTypes.UpdateCharacterParty, nameof(UpdateCharacterPartyAsync));
        }

        public async UniTask<DatabaseApiResult> ClearCharacterPartyAsync(ClearCharacterPartyReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.ClearCharacterParty, nameof(ClearCharacterPartyAsync));
        }

        public async UniTask<DatabaseApiResult<PartyResp>> GetPartyAsync(GetPartyReq request)
        {
            return await SendRequest<GetPartyReq, PartyResp>(request, DatabaseRequestTypes.GetParty, nameof(GetPartyAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> CreateGuildAsync(CreateGuildReq request)
        {
            return await SendRequest<CreateGuildReq, GuildResp>(request, DatabaseRequestTypes.CreateGuild, nameof(CreateGuildAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildLeaderAsync(UpdateGuildLeaderReq request)
        {
            return await SendRequest<UpdateGuildLeaderReq, GuildResp>(request, DatabaseRequestTypes.UpdateGuildLeader, nameof(UpdateGuildLeaderAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildMessageAsync(UpdateGuildMessageReq request)
        {
            return await SendRequest<UpdateGuildMessageReq, GuildResp>(request, DatabaseRequestTypes.UpdateGuildMessage, nameof(UpdateGuildMessageAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildMessage2Async(UpdateGuildMessageReq request)
        {
            return await SendRequest<UpdateGuildMessageReq, GuildResp>(request, DatabaseRequestTypes.UpdateGuildMessage2, nameof(UpdateGuildMessage2Async));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildOptionsAsync(UpdateGuildOptionsReq request)
        {
            return await SendRequest<UpdateGuildOptionsReq, GuildResp>(request, DatabaseRequestTypes.UpdateGuildOptions, nameof(UpdateGuildOptionsAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildAutoAcceptRequestsAsync(UpdateGuildAutoAcceptRequestsReq request)
        {
            return await SendRequest<UpdateGuildAutoAcceptRequestsReq, GuildResp>(request, DatabaseRequestTypes.UpdateGuildAutoAcceptRequests, nameof(UpdateGuildAutoAcceptRequestsAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildRoleAsync(UpdateGuildRoleReq request)
        {
            return await SendRequest<UpdateGuildRoleReq, GuildResp>(request, DatabaseRequestTypes.UpdateGuildRole, nameof(UpdateGuildRoleAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildMemberRoleAsync(UpdateGuildMemberRoleReq request)
        {
            return await SendRequest<UpdateGuildMemberRoleReq, GuildResp>(request, DatabaseRequestTypes.UpdateGuildMemberRole, nameof(UpdateGuildMemberRoleAsync));
        }

        public async UniTask<DatabaseApiResult> DeleteGuildAsync(DeleteGuildReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.DeleteGuild, nameof(DeleteGuildAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateCharacterGuildAsync(UpdateCharacterGuildReq request)
        {
            return await SendRequest<UpdateCharacterGuildReq, GuildResp>(request, DatabaseRequestTypes.UpdateCharacterGuild, nameof(UpdateCharacterGuildAsync));
        }

        public async UniTask<DatabaseApiResult> ClearCharacterGuildAsync(ClearCharacterGuildReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.ClearCharacterGuild, nameof(ClearCharacterGuildAsync));
        }

        public async UniTask<DatabaseApiResult<FindGuildNameResp>> FindGuildNameAsync(FindGuildNameReq request)
        {
            return await SendRequest<FindGuildNameReq, FindGuildNameResp>(request, DatabaseRequestTypes.FindGuildName, nameof(FindGuildNameAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> GetGuildAsync(GetGuildReq request)
        {
            return await SendRequest<GetGuildReq, GuildResp>(request, DatabaseRequestTypes.GetGuild, nameof(GetGuildAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> IncreaseGuildExpAsync(IncreaseGuildExpReq request)
        {
            return await SendRequest<IncreaseGuildExpReq, GuildResp>(request, DatabaseRequestTypes.IncreaseGuildExp, nameof(IncreaseGuildExpAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> AddGuildSkillAsync(AddGuildSkillReq request)
        {
            return await SendRequest<AddGuildSkillReq, GuildResp>(request, DatabaseRequestTypes.AddGuildSkill, nameof(AddGuildSkillAsync));
        }

        public async UniTask<DatabaseApiResult<GuildGoldResp>> GetGuildGoldAsync(GetGuildGoldReq request)
        {
            return await SendRequest<GetGuildGoldReq, GuildGoldResp>(request, DatabaseRequestTypes.GetGuildGold, nameof(GetGuildGoldAsync));
        }

        public async UniTask<DatabaseApiResult<GuildGoldResp>> ChangeGuildGoldAsync(ChangeGuildGoldReq request)
        {
            return await SendRequest<ChangeGuildGoldReq, GuildGoldResp>(request, DatabaseRequestTypes.ChangeGuildGold, nameof(ChangeGuildGoldAsync));
        }

        public async UniTask<DatabaseApiResult<GetStorageItemsResp>> GetStorageItemsAsync(GetStorageItemsReq request)
        {
            return await SendRequest<GetStorageItemsReq, GetStorageItemsResp>(request, DatabaseRequestTypes.GetStorageItems, nameof(GetStorageItemsAsync));
        }

        public async UniTask<DatabaseApiResult> UpdateStorageItemsAsync(UpdateStorageItemsReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.UpdateStorageItems, nameof(UpdateStorageItemsAsync));
        }

        public async UniTask<DatabaseApiResult> UpdateStorageAndCharacterItemsAsync(UpdateStorageAndCharacterItemsReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.UpdateStorageAndCharacterItems, nameof(UpdateStorageAndCharacterItemsAsync));
        }

        public async UniTask<DatabaseApiResult> DeleteAllReservedStorageAsync()
        {
            return await SendRequest(EmptyMessage.Value, DatabaseRequestTypes.DeleteAllReservedStorage, nameof(DeleteAllReservedStorageAsync));
        }

        public async UniTask<DatabaseApiResult<MailListResp>> MailListAsync(MailListReq request)
        {
            return await SendRequest<MailListReq, MailListResp>(request, DatabaseRequestTypes.MailList, nameof(MailListAsync));
        }

        public async UniTask<DatabaseApiResult<UpdateReadMailStateResp>> UpdateReadMailStateAsync(UpdateReadMailStateReq request)
        {
            return await SendRequest<UpdateReadMailStateReq, UpdateReadMailStateResp>(request, DatabaseRequestTypes.UpdateReadMailState, nameof(UpdateReadMailStateAsync));
        }

        public async UniTask<DatabaseApiResult<UpdateClaimMailItemsStateResp>> UpdateClaimMailItemsStateAsync(UpdateClaimMailItemsStateReq request)
        {
            return await SendRequest<UpdateClaimMailItemsStateReq, UpdateClaimMailItemsStateResp>(request, DatabaseRequestTypes.UpdateClaimMailItemsState, nameof(UpdateClaimMailItemsStateAsync));
        }

        public async UniTask<DatabaseApiResult<UpdateDeleteMailStateResp>> UpdateDeleteMailStateAsync(UpdateDeleteMailStateReq request)
        {
            return await SendRequest<UpdateDeleteMailStateReq, UpdateDeleteMailStateResp>(request, DatabaseRequestTypes.UpdateDeleteMailState, nameof(UpdateDeleteMailStateAsync));
        }

        public async UniTask<DatabaseApiResult<SendMailResp>> SendMailAsync(SendMailReq request)
        {
            return await SendRequest<SendMailReq, SendMailResp>(request, DatabaseRequestTypes.SendMail, nameof(SendMailAsync));
        }

        public async UniTask<DatabaseApiResult<GetMailResp>> GetMailAsync(GetMailReq request)
        {
            return await SendRequest<GetMailReq, GetMailResp>(request, DatabaseRequestTypes.GetMail, nameof(GetMailAsync));
        }

        public async UniTask<DatabaseApiResult<GetIdByCharacterNameResp>> GetIdByCharacterNameAsync(GetIdByCharacterNameReq request)
        {
            return await SendRequest<GetIdByCharacterNameReq, GetIdByCharacterNameResp>(request, DatabaseRequestTypes.GetIdByCharacterName, nameof(GetIdByCharacterNameAsync));
        }

        public async UniTask<DatabaseApiResult<GetUserIdByCharacterNameResp>> GetUserIdByCharacterNameAsync(GetUserIdByCharacterNameReq request)
        {
            return await SendRequest<GetUserIdByCharacterNameReq, GetUserIdByCharacterNameResp>(request, DatabaseRequestTypes.GetUserIdByCharacterName, nameof(GetUserIdByCharacterNameAsync));
        }

        public async UniTask<DatabaseApiResult<GetMailNotificationResp>> GetMailNotificationAsync(GetMailNotificationReq request)
        {
            return await SendRequest<GetMailNotificationReq, GetMailNotificationResp>(request, DatabaseRequestTypes.GetMailNotification, nameof(GetMailNotificationAsync));
        }

        public async UniTask<DatabaseApiResult<GetUserUnbanTimeResp>> GetUserUnbanTimeAsync(GetUserUnbanTimeReq request)
        {
            return await SendRequest<GetUserUnbanTimeReq, GetUserUnbanTimeResp>(request, DatabaseRequestTypes.GetUserUnbanTime, nameof(GetUserUnbanTimeAsync));
        }

        public async UniTask<DatabaseApiResult> SetUserUnbanTimeByCharacterNameAsync(SetUserUnbanTimeByCharacterNameReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.SetUserUnbanTimeByCharacterName, nameof(SetUserUnbanTimeByCharacterNameAsync));
        }

        public async UniTask<DatabaseApiResult> SetCharacterUnmuteTimeByNameAsync(SetCharacterUnmuteTimeByNameReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.SetCharacterUnmuteTimeByName, nameof(SetCharacterUnmuteTimeByNameAsync));
        }

        public async UniTask<DatabaseApiResult<GetSummonBuffsResp>> GetSummonBuffsAsync(GetSummonBuffsReq request)
        {
            return await SendRequest<GetSummonBuffsReq, GetSummonBuffsResp>(request, DatabaseRequestTypes.GetSummonBuffs, nameof(GetSummonBuffsAsync));
        }

        public async UniTask<DatabaseApiResult<ValidateEmailVerificationResp>> ValidateEmailVerificationAsync(ValidateEmailVerificationReq request)
        {
            return await SendRequest<ValidateEmailVerificationReq, ValidateEmailVerificationResp>(request, DatabaseRequestTypes.ValidateEmailVerification, nameof(ValidateEmailVerificationAsync));
        }

        public async UniTask<DatabaseApiResult<FindEmailResp>> FindEmailAsync(FindEmailReq request)
        {
            return await SendRequest<FindEmailReq, FindEmailResp>(request, DatabaseRequestTypes.FindEmail, nameof(FindEmailAsync));
        }

        public async UniTask<DatabaseApiResult<GetFriendRequestNotificationResp>> GetFriendRequestNotificationAsync(GetFriendRequestNotificationReq request)
        {
            return await SendRequest<GetFriendRequestNotificationReq, GetFriendRequestNotificationResp>(request, DatabaseRequestTypes.GetFriendRequestNotification, nameof(GetFriendRequestNotificationAsync));
        }

        public async UniTask<DatabaseApiResult> UpdateUserCount(UpdateUserCountReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.UpdateUserCount, nameof(UpdateUserCount));
        }

        public async UniTask<DatabaseApiResult<SocialCharacterResp>> GetSocialCharacterAsync(GetSocialCharacterReq request)
        {
            return await SendRequest<GetSocialCharacterReq, SocialCharacterResp>(request, DatabaseRequestTypes.GetSocialCharacter, nameof(GetSocialCharacterAsync));
        }

        public async UniTask<DatabaseApiResult<GuildsResp>> FindGuildsAsync(FindGuildNameReq request)
        {
            return await SendRequest<FindGuildNameReq, GuildsResp>(request, DatabaseRequestTypes.FindGuilds, nameof(FindGuildsAsync));
        }

        public async UniTask<DatabaseApiResult> CreateGuildRequestAsync(CreateGuildRequestReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.CreateGuildRequest, nameof(CreateGuildRequestAsync));
        }

        public async UniTask<DatabaseApiResult> DeleteGuildRequestAsync(DeleteGuildRequestReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.DeleteGuildRequest, nameof(DeleteGuildRequestAsync));
        }

        public async UniTask<DatabaseApiResult<SocialCharactersResp>> GetGuildRequestsAsync(GetGuildRequestsReq request)
        {
            return await SendRequest<GetGuildRequestsReq, SocialCharactersResp>(request, DatabaseRequestTypes.GetGuildRequests, nameof(GetGuildRequestsAsync));
        }

        public async UniTask<DatabaseApiResult<GetGuildRequestNotificationResp>> GetGuildRequestNotificationAsync(GetGuildRequestNotificationReq request)
        {
            return await SendRequest<GetGuildRequestNotificationReq, GetGuildRequestNotificationResp>(request, DatabaseRequestTypes.GetGuildRequestNotification, nameof(GetGuildRequestNotificationAsync));
        }

        public async UniTask<DatabaseApiResult> UpdateGuildMemberCountAsync(UpdateGuildMemberCountReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.UpdateGuildMemberCount, nameof(UpdateGuildMemberCountAsync));
        }

        public async UniTask<DatabaseApiResult> RemoveGuildCacheAsync(GetGuildReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.RemoveGuildCache, nameof(RemoveGuildCacheAsync));
        }

        public async UniTask<DatabaseApiResult> RemovePartyCacheAsync(GetPartyReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.RemovePartyCache, nameof(RemovePartyCacheAsync));
        }
    }
}
