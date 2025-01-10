using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
#if NET || NETCOREAPP
using DotNetRestClient;
#else
using UnityRestClient;
#endif

namespace MultiplayerARPG.MMO
{
    public partial class RestDatabaseClient : RestClient, IDatabaseClient
    {
        public const string HEADER_REQUEST_TIME = "RequestTimeUtc";
        
        [System.Serializable]
        public struct Config
        {
            public string dbApiUrl;
            public string dbSecretKey;
        }

        public string apiUrl = "http://localhost:5757/api/";
        public string secretKey = "secret";

        void Awake()
        {
            string configFolder = "./Config";
            string configFilePath = configFolder + "/serverConfig.json";
            Dictionary<string, object> jsonConfig = new Dictionary<string, object>();
            Logging.Log(nameof(RestDatabaseClient), "Reading config file from " + configFilePath);
            if (File.Exists(configFilePath))
            {
                // Read config file
                Logging.Log(nameof(RestDatabaseClient), "Found config file");
                string dataAsJson = File.ReadAllText(configFilePath);
                Config newConfig = JsonConvert.DeserializeObject<Config>(dataAsJson);
                if (newConfig.dbApiUrl != null)
                    apiUrl = newConfig.dbApiUrl;
                if (newConfig.dbSecretKey != null)
                    secretKey = newConfig.dbSecretKey;
            }
        }

        private async UniTask<DatabaseApiResult<TResp>> SendRequest<TReq, TResp>(TReq request, string url, string functionName)
        {
            var headers = new Dictionary<string, string>()
            {
                { HEADER_REQUEST_TIME, System.DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
            };
            var resp = new Result<TResp>(await Post(GetUrl(apiUrl, url), GetJsonContent(request), secretKey, BearerAuthHeaderSettings, headers));
            if (resp.IsError())
            {
                Logging.LogError(nameof(RestDatabaseClient), $"Cannot {functionName} status: {resp.ResponseCode}");
                return new DatabaseApiResult<TResp>()
                {
                    IsError = true,
                    Error = resp.Error,
                    Response = resp.Content,
                };
            }
            return new DatabaseApiResult<TResp>()
            {
                Response = resp.Content,
            };
        }

        private async UniTask<DatabaseApiResult> SendRequest<TReq>(TReq request, string url, string functionName)
        {
            var headers = new Dictionary<string, string>()
            {
                { HEADER_REQUEST_TIME, System.DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
            };
            var resp = await Post(GetUrl(apiUrl, url), GetJsonContent(request), secretKey, BearerAuthHeaderSettings, headers);
            if (resp.IsError())
            {
                Logging.LogError(nameof(RestDatabaseClient), $"Cannot {functionName} status: {resp.ResponseCode}");
                return new DatabaseApiResult()
                {
                    IsError = true,
                    Error = resp.Error,
                };
            }
            return new DatabaseApiResult();
        }

        public async UniTask<DatabaseApiResult<ValidateUserLoginResp>> ValidateUserLoginAsync(ValidateUserLoginReq request)
        {
            return await SendRequest<ValidateUserLoginReq, ValidateUserLoginResp>(request, DatabaseApiPath.ValidateUserLogin, nameof(ValidateUserLoginAsync));
        }

        public async UniTask<DatabaseApiResult<ValidateAccessTokenResp>> ValidateAccessTokenAsync(ValidateAccessTokenReq request)
        {
            return await SendRequest<ValidateAccessTokenReq, ValidateAccessTokenResp>(request, DatabaseApiPath.ValidateAccessToken, nameof(ValidateAccessTokenAsync));
        }

        public async UniTask<DatabaseApiResult<GetUserLevelResp>> GetUserLevelAsync(GetUserLevelReq request)
        {
            return await SendRequest<GetUserLevelReq, GetUserLevelResp>(request, DatabaseApiPath.GetUserLevel, nameof(GetUserLevelAsync));
        }

        public async UniTask<DatabaseApiResult<GoldResp>> GetGoldAsync(GetGoldReq request)
        {
            return await SendRequest<GetGoldReq, GoldResp>(request, DatabaseApiPath.GetGold, nameof(GetGoldAsync));
        }

        public async UniTask<DatabaseApiResult<GoldResp>> ChangeGoldAsync(ChangeGoldReq request)
        {
            return await SendRequest<ChangeGoldReq, GoldResp>(request, DatabaseApiPath.ChangeGold, nameof(ChangeGoldAsync));
        }

        public async UniTask<DatabaseApiResult<CashResp>> GetCashAsync(GetCashReq request)
        {
            return await SendRequest<GetCashReq, CashResp>(request, DatabaseApiPath.GetCash, nameof(GetCashAsync));
        }

        public async UniTask<DatabaseApiResult<CashResp>> ChangeCashAsync(ChangeCashReq request)
        {
            return await SendRequest<ChangeCashReq, CashResp>(request, DatabaseApiPath.ChangeCash, nameof(ChangeCashAsync));
        }

        public async UniTask<DatabaseApiResult> UpdateAccessTokenAsync(UpdateAccessTokenReq request)
        {
            return await SendRequest(request, DatabaseApiPath.UpdateAccessToken, nameof(UpdateAccessTokenAsync));
        }

        public async UniTask<DatabaseApiResult> CreateUserLoginAsync(CreateUserLoginReq request)
        {
            return await SendRequest(request, DatabaseApiPath.CreateUserLogin, nameof(CreateUserLoginAsync));
        }

        public async UniTask<DatabaseApiResult<FindUsernameResp>> FindUsernameAsync(FindUsernameReq request)
        {
            return await SendRequest<FindUsernameReq, FindUsernameResp>(request, DatabaseApiPath.FindUsername, nameof(FindUsernameAsync));
        }

        public async UniTask<DatabaseApiResult<CharacterResp>> CreateCharacterAsync(CreateCharacterReq request)
        {
            return await SendRequest<CreateCharacterReq, CharacterResp>(request, DatabaseApiPath.CreateCharacter, nameof(CreateCharacterAsync));
        }

        public async UniTask<DatabaseApiResult<CharacterResp>> GetCharacterAsync(GetCharacterReq request)
        {
            return await SendRequest<GetCharacterReq, CharacterResp>(request, DatabaseApiPath.GetCharacter, nameof(GetCharacterAsync));
        }

        public async UniTask<DatabaseApiResult<CharactersResp>> GetCharactersAsync(GetCharactersReq request)
        {
            return await SendRequest<GetCharactersReq, CharactersResp>(request, DatabaseApiPath.GetCharacters, nameof(GetCharactersAsync));
        }

        public async UniTask<DatabaseApiResult<CharacterResp>> UpdateCharacterAsync(UpdateCharacterReq request)
        {
            return await SendRequest<UpdateCharacterReq, CharacterResp>(request, DatabaseApiPath.UpdateCharacter, nameof(UpdateCharacterAsync));
        }

        public async UniTask<DatabaseApiResult> DeleteCharacterAsync(DeleteCharacterReq request)
        {
            return await SendRequest(request, DatabaseApiPath.DeleteCharacter, nameof(DeleteCharacterAsync));
        }

        public async UniTask<DatabaseApiResult<FindCharacterNameResp>> FindCharacterNameAsync(FindCharacterNameReq request)
        {
            return await SendRequest<FindCharacterNameReq, FindCharacterNameResp>(request, DatabaseApiPath.FindCharacterName, nameof(FindCharacterNameAsync));
        }

        public async UniTask<DatabaseApiResult<SocialCharactersResp>> FindCharactersAsync(FindCharacterNameReq request)
        {
            return await SendRequest<FindCharacterNameReq, SocialCharactersResp>(request, DatabaseApiPath.FindCharacters, nameof(FindCharactersAsync));
        }

        public async UniTask<DatabaseApiResult> CreateFriendAsync(CreateFriendReq request)
        {
            return await SendRequest(request, DatabaseApiPath.CreateFriend, nameof(CreateFriendAsync));
        }

        public async UniTask<DatabaseApiResult> DeleteFriendAsync(DeleteFriendReq request)
        {
            return await SendRequest(request, DatabaseApiPath.DeleteFriend, nameof(DeleteFriendAsync));
        }

        public async UniTask<DatabaseApiResult<SocialCharactersResp>> GetFriendsAsync(GetFriendsReq request)
        {
            return await SendRequest<GetFriendsReq, SocialCharactersResp>(request, DatabaseApiPath.GetFriends, nameof(GetFriendsAsync));
        }

        public async UniTask<DatabaseApiResult<BuildingResp>> CreateBuildingAsync(CreateBuildingReq request)
        {
            return await SendRequest<CreateBuildingReq, BuildingResp>(request, DatabaseApiPath.CreateBuilding, nameof(CreateBuildingAsync));
        }

        public async UniTask<DatabaseApiResult<BuildingResp>> UpdateBuildingAsync(UpdateBuildingReq request)
        {
            return await SendRequest<UpdateBuildingReq, BuildingResp>(request, DatabaseApiPath.UpdateBuilding, nameof(UpdateBuildingAsync));
        }

        public async UniTask<DatabaseApiResult> DeleteBuildingAsync(DeleteBuildingReq request)
        {
            return await SendRequest(request, DatabaseApiPath.DeleteBuilding, nameof(DeleteBuildingAsync));
        }

        public async UniTask<DatabaseApiResult<BuildingsResp>> GetBuildingsAsync(GetBuildingsReq request)
        {
            return await SendRequest<GetBuildingsReq, BuildingsResp>(request, DatabaseApiPath.GetBuildings, nameof(GetBuildingsAsync));
        }

        public async UniTask<DatabaseApiResult<PartyResp>> CreatePartyAsync(CreatePartyReq request)
        {
            return await SendRequest<CreatePartyReq, PartyResp>(request, DatabaseApiPath.CreateParty, nameof(CreatePartyAsync));
        }

        public async UniTask<DatabaseApiResult<PartyResp>> UpdatePartyAsync(UpdatePartyReq request)
        {
            return await SendRequest<UpdatePartyReq, PartyResp>(request, DatabaseApiPath.UpdateParty, nameof(UpdatePartyAsync));
        }

        public async UniTask<DatabaseApiResult<PartyResp>> UpdatePartyLeaderAsync(UpdatePartyLeaderReq request)
        {
            return await SendRequest<UpdatePartyLeaderReq, PartyResp>(request, DatabaseApiPath.UpdatePartyLeader, nameof(UpdatePartyLeaderAsync));
        }

        public async UniTask<DatabaseApiResult> DeletePartyAsync(DeletePartyReq request)
        {
            return await SendRequest(request, DatabaseApiPath.DeleteParty, nameof(DeletePartyAsync));
        }

        public async UniTask<DatabaseApiResult<PartyResp>> UpdateCharacterPartyAsync(UpdateCharacterPartyReq request)
        {
            return await SendRequest<UpdateCharacterPartyReq, PartyResp>(request, DatabaseApiPath.UpdateCharacterParty, nameof(UpdateCharacterPartyAsync));
        }

        public async UniTask<DatabaseApiResult> ClearCharacterPartyAsync(ClearCharacterPartyReq request)
        {
            return await SendRequest(request, DatabaseApiPath.ClearCharacterParty, nameof(ClearCharacterPartyAsync));
        }

        public async UniTask<DatabaseApiResult<PartyResp>> GetPartyAsync(GetPartyReq request)
        {
            return await SendRequest<GetPartyReq, PartyResp>(request, DatabaseApiPath.GetParty, nameof(GetPartyAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> CreateGuildAsync(CreateGuildReq request)
        {
            return await SendRequest<CreateGuildReq, GuildResp>(request, DatabaseApiPath.CreateGuild, nameof(CreateGuildAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildLeaderAsync(UpdateGuildLeaderReq request)
        {
            return await SendRequest<UpdateGuildLeaderReq, GuildResp>(request, DatabaseApiPath.UpdateGuildLeader, nameof(UpdateGuildLeaderAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildMessageAsync(UpdateGuildMessageReq request)
        {
            return await SendRequest<UpdateGuildMessageReq, GuildResp>(request, DatabaseApiPath.UpdateGuildMessage, nameof(UpdateGuildMessageAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildMessage2Async(UpdateGuildMessageReq request)
        {
            return await SendRequest<UpdateGuildMessageReq, GuildResp>(request, DatabaseApiPath.UpdateGuildMessage2, nameof(UpdateGuildMessage2Async));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildOptionsAsync(UpdateGuildOptionsReq request)
        {
            return await SendRequest<UpdateGuildOptionsReq, GuildResp>(request, DatabaseApiPath.UpdateGuildOptions, nameof(UpdateGuildOptionsAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildAutoAcceptRequestsAsync(UpdateGuildAutoAcceptRequestsReq request)
        {
            return await SendRequest<UpdateGuildAutoAcceptRequestsReq, GuildResp>(request, DatabaseApiPath.UpdateGuildAutoAcceptRequests, nameof(UpdateGuildAutoAcceptRequestsAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildRoleAsync(UpdateGuildRoleReq request)
        {
            return await SendRequest<UpdateGuildRoleReq, GuildResp>(request, DatabaseApiPath.UpdateGuildRole, nameof(UpdateGuildRoleAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildMemberRoleAsync(UpdateGuildMemberRoleReq request)
        {
            return await SendRequest<UpdateGuildMemberRoleReq, GuildResp>(request, DatabaseApiPath.UpdateGuildMemberRole, nameof(UpdateGuildMemberRoleAsync));
        }

        public async UniTask<DatabaseApiResult> DeleteGuildAsync(DeleteGuildReq request)
        {
            return await SendRequest(request, DatabaseApiPath.DeleteGuild, nameof(DeleteGuildAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateCharacterGuildAsync(UpdateCharacterGuildReq request)
        {
            return await SendRequest<UpdateCharacterGuildReq, GuildResp>(request, DatabaseApiPath.UpdateCharacterGuild, nameof(UpdateCharacterGuildAsync));
        }

        public async UniTask<DatabaseApiResult> ClearCharacterGuildAsync(ClearCharacterGuildReq request)
        {
            return await SendRequest(request, DatabaseApiPath.ClearCharacterGuild, nameof(ClearCharacterGuildAsync));
        }

        public async UniTask<DatabaseApiResult<FindGuildNameResp>> FindGuildNameAsync(FindGuildNameReq request)
        {
            return await SendRequest<FindGuildNameReq, FindGuildNameResp>(request, DatabaseApiPath.FindGuildName, nameof(FindGuildNameAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> GetGuildAsync(GetGuildReq request)
        {
            return await SendRequest<GetGuildReq, GuildResp>(request, DatabaseApiPath.GetGuild, nameof(GetGuildAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> IncreaseGuildExpAsync(IncreaseGuildExpReq request)
        {
            return await SendRequest<IncreaseGuildExpReq, GuildResp>(request, DatabaseApiPath.IncreaseGuildExp, nameof(IncreaseGuildExpAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> AddGuildSkillAsync(AddGuildSkillReq request)
        {
            return await SendRequest<AddGuildSkillReq, GuildResp>(request, DatabaseApiPath.AddGuildSkill, nameof(AddGuildSkillAsync));
        }

        public async UniTask<DatabaseApiResult<GuildGoldResp>> GetGuildGoldAsync(GetGuildGoldReq request)
        {
            return await SendRequest<GetGuildGoldReq, GuildGoldResp>(request, DatabaseApiPath.GetGuildGold, nameof(GetGuildGoldAsync));
        }

        public async UniTask<DatabaseApiResult<GuildGoldResp>> ChangeGuildGoldAsync(ChangeGuildGoldReq request)
        {
            return await SendRequest<ChangeGuildGoldReq, GuildGoldResp>(request, DatabaseApiPath.ChangeGuildGold, nameof(ChangeGuildGoldAsync));
        }

        public async UniTask<DatabaseApiResult<GetStorageItemsResp>> GetStorageItemsAsync(GetStorageItemsReq request)
        {
            return await SendRequest<GetStorageItemsReq, GetStorageItemsResp>(request, DatabaseApiPath.GetStorageItems, nameof(GetStorageItemsAsync));
        }

        public async UniTask<DatabaseApiResult> UpdateStorageItemsAsync(UpdateStorageItemsReq request)
        {
            return await SendRequest(request, DatabaseApiPath.UpdateStorageItems, nameof(UpdateStorageItemsAsync));
        }

        public async UniTask<DatabaseApiResult> DeleteAllReservedStorageAsync()
        {
            return await SendRequest(EmptyMessage.Value, DatabaseApiPath.DeleteAllReservedStorage, nameof(DeleteAllReservedStorageAsync));
        }

        public async UniTask<DatabaseApiResult<MailListResp>> MailListAsync(MailListReq request)
        {
            return await SendRequest<MailListReq, MailListResp>(request, DatabaseApiPath.MailList, nameof(MailListAsync));
        }

        public async UniTask<DatabaseApiResult<UpdateReadMailStateResp>> UpdateReadMailStateAsync(UpdateReadMailStateReq request)
        {
            return await SendRequest<UpdateReadMailStateReq, UpdateReadMailStateResp>(request, DatabaseApiPath.UpdateReadMailState, nameof(UpdateReadMailStateAsync));
        }

        public async UniTask<DatabaseApiResult<UpdateClaimMailItemsStateResp>> UpdateClaimMailItemsStateAsync(UpdateClaimMailItemsStateReq request)
        {
            return await SendRequest<UpdateClaimMailItemsStateReq, UpdateClaimMailItemsStateResp>(request, DatabaseApiPath.UpdateClaimMailItemsState, nameof(UpdateClaimMailItemsStateAsync));
        }

        public async UniTask<DatabaseApiResult<UpdateDeleteMailStateResp>> UpdateDeleteMailStateAsync(UpdateDeleteMailStateReq request)
        {
            return await SendRequest<UpdateDeleteMailStateReq, UpdateDeleteMailStateResp>(request, DatabaseApiPath.UpdateDeleteMailState, nameof(UpdateDeleteMailStateAsync));
        }

        public async UniTask<DatabaseApiResult<SendMailResp>> SendMailAsync(SendMailReq request)
        {
            return await SendRequest<SendMailReq, SendMailResp>(request, DatabaseApiPath.SendMail, nameof(SendMailAsync));
        }

        public async UniTask<DatabaseApiResult<GetMailResp>> GetMailAsync(GetMailReq request)
        {
            return await SendRequest<GetMailReq, GetMailResp>(request, DatabaseApiPath.GetMail, nameof(GetMailAsync));
        }

        public async UniTask<DatabaseApiResult<GetIdByCharacterNameResp>> GetIdByCharacterNameAsync(GetIdByCharacterNameReq request)
        {
            return await SendRequest<GetIdByCharacterNameReq, GetIdByCharacterNameResp>(request, DatabaseApiPath.GetIdByCharacterName, nameof(GetIdByCharacterNameAsync));
        }

        public async UniTask<DatabaseApiResult<GetUserIdByCharacterNameResp>> GetUserIdByCharacterNameAsync(GetUserIdByCharacterNameReq request)
        {
            return await SendRequest<GetUserIdByCharacterNameReq, GetUserIdByCharacterNameResp>(request, DatabaseApiPath.GetUserIdByCharacterName, nameof(GetUserIdByCharacterNameAsync));
        }

        public async UniTask<DatabaseApiResult<GetMailNotificationResp>> GetMailNotificationAsync(GetMailNotificationReq request)
        {
            return await SendRequest<GetMailNotificationReq, GetMailNotificationResp>(request, DatabaseApiPath.GetMailNotification, nameof(GetMailNotificationAsync));
        }

        public async UniTask<DatabaseApiResult<GetUserUnbanTimeResp>> GetUserUnbanTimeAsync(GetUserUnbanTimeReq request)
        {
            return await SendRequest<GetUserUnbanTimeReq, GetUserUnbanTimeResp>(request, DatabaseApiPath.GetUserUnbanTime, nameof(GetUserUnbanTimeAsync));
        }

        public async UniTask<DatabaseApiResult> SetUserUnbanTimeByCharacterNameAsync(SetUserUnbanTimeByCharacterNameReq request)
        {
            return await SendRequest(request, DatabaseApiPath.SetUserUnbanTimeByCharacterName, nameof(SetUserUnbanTimeByCharacterNameAsync));
        }

        public async UniTask<DatabaseApiResult> SetCharacterUnmuteTimeByNameAsync(SetCharacterUnmuteTimeByNameReq request)
        {
            return await SendRequest(request, DatabaseApiPath.SetCharacterUnmuteTimeByName, nameof(SetCharacterUnmuteTimeByNameAsync));
        }

        public async UniTask<DatabaseApiResult<GetSummonBuffsResp>> GetSummonBuffsAsync(GetSummonBuffsReq request)
        {
            return await SendRequest<GetSummonBuffsReq, GetSummonBuffsResp>(request, DatabaseApiPath.GetSummonBuffs, nameof(GetSummonBuffsAsync));
        }

        public async UniTask<DatabaseApiResult<ValidateEmailVerificationResp>> ValidateEmailVerificationAsync(ValidateEmailVerificationReq request)
        {
            return await SendRequest<ValidateEmailVerificationReq, ValidateEmailVerificationResp>(request, DatabaseApiPath.ValidateEmailVerification, nameof(ValidateEmailVerificationAsync));
        }

        public async UniTask<DatabaseApiResult<FindEmailResp>> FindEmailAsync(FindEmailReq request)
        {
            return await SendRequest<FindEmailReq, FindEmailResp>(request, DatabaseApiPath.FindEmail, nameof(FindEmailAsync));
        }

        public async UniTask<DatabaseApiResult<GetFriendRequestNotificationResp>> GetFriendRequestNotificationAsync(GetFriendRequestNotificationReq request)
        {
            return await SendRequest<GetFriendRequestNotificationReq, GetFriendRequestNotificationResp>(request, DatabaseApiPath.GetFriendRequestNotification, nameof(GetFriendRequestNotificationAsync));
        }

        public async UniTask<DatabaseApiResult> UpdateUserCount(UpdateUserCountReq request)
        {
            return await SendRequest(request, DatabaseApiPath.UpdateUserCount, nameof(UpdateUserCount));
        }

        public async UniTask<DatabaseApiResult<SocialCharacterResp>> GetSocialCharacterAsync(GetSocialCharacterReq request)
        {
            return await SendRequest<GetSocialCharacterReq, SocialCharacterResp>(request, DatabaseApiPath.GetSocialCharacter, nameof(GetSocialCharacterAsync));
        }

        public async UniTask<DatabaseApiResult<GuildsResp>> FindGuildsAsync(FindGuildNameReq request)
        {
            return await SendRequest<FindGuildNameReq, GuildsResp>(request, DatabaseApiPath.FindGuilds, nameof(FindGuildsAsync));
        }

        public async UniTask<DatabaseApiResult> CreateGuildRequestAsync(CreateGuildRequestReq request)
        {
            return await SendRequest(request, DatabaseApiPath.CreateGuildRequest, nameof(CreateGuildRequestAsync));
        }

        public async UniTask<DatabaseApiResult> DeleteGuildRequestAsync(DeleteGuildRequestReq request)
        {
            return await SendRequest(request, DatabaseApiPath.DeleteGuildRequest, nameof(DeleteGuildRequestAsync));
        }

        public async UniTask<DatabaseApiResult<SocialCharactersResp>> GetGuildRequestsAsync(GetGuildRequestsReq request)
        {
            return await SendRequest<GetGuildRequestsReq, SocialCharactersResp>(request, DatabaseApiPath.GetGuildRequests, nameof(GetGuildRequestsAsync));
        }

        public async UniTask<DatabaseApiResult<GetGuildRequestNotificationResp>> GetGuildRequestNotificationAsync(GetGuildRequestNotificationReq request)
        {
            return await SendRequest<GetGuildRequestNotificationReq, GetGuildRequestNotificationResp>(request, DatabaseApiPath.GetGuildRequestNotification, nameof(GetGuildRequestNotificationAsync));
        }

        public async UniTask<DatabaseApiResult> UpdateGuildMemberCountAsync(UpdateGuildMemberCountReq request)
        {
            return await SendRequest(request, DatabaseApiPath.UpdateGuildMemberCount, nameof(UpdateGuildMemberCountAsync));
        }
    }
}