using System.Linq;
using LiteNetLibManager;
using Cysharp.Threading.Tasks;
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
using UnityEngine;
#endif

namespace MultiplayerARPG.MMO
{
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
    [DefaultExecutionOrder(DefaultExecutionOrders.DATABASE_NETWORK_MANAGER)]
#endif
    public partial class DatabaseNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        [SerializeField]
#endif
        private BaseDatabase database = null;
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        [SerializeField]
#endif
        private BaseDatabase[] databaseOptions = new BaseDatabase[0];

        public BaseDatabase Database
        {
            get
            {
                return database == null ? databaseOptions.FirstOrDefault() : database;
            }
            set
            {
                database = value;
            }
        }

        public IDatabaseCache DatabaseCache
        {
            get; set;
        }

        public bool ProceedingBeforeQuit { get; private set; } = false;
        public bool ReadyToQuit { get; private set; } = false;

        public void SetDatabaseByOptionIndex(int index)
        {
            if (databaseOptions != null &&
                databaseOptions.Length > 0 &&
                index >= 0 &&
                index < databaseOptions.Length)
                database = databaseOptions[index];
        }

#if NET || NETCOREAPP
        public DatabaseNetworkManager() : base()
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
            useWebSocket = false;
            maxConnections = int.MaxValue;
        }

        public async void ProceedBeforeQuit()
        {
            if (ProceedingBeforeQuit)
                return;
            ProceedingBeforeQuit = true;
            // Delay 30 secs before quit
            int seconds = 30;
            do
            {
                Logging.Log($"[DatabaseNetworkManager] {seconds} seconds before quit.");
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
                await UniTask.Delay(1000);
#elif NET || NETCOREAPP
                await Task.Delay(1000);
#endif
                seconds--;
            } while (seconds > 0);
            await UniTask.Yield();
            ReadyToQuit = true;
            // Request to quit again
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            Application.Quit();
#endif
        }

        public override async void OnStartServer()
        {
            base.OnStartServer();
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            Database.Initialize();
#endif
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
            await Database.DoMigration();
#endif
        }

        protected override void RegisterMessages()
        {
            base.RegisterMessages();
            EnableRequestResponse(MMOMessageTypes.Request, MMOMessageTypes.Response);
            RegisterRequestToServer<ValidateUserLoginReq, ValidateUserLoginResp>(DatabaseRequestTypes.ValidateUserLogin, ValidateUserLogin);
            RegisterRequestToServer<ValidateAccessTokenReq, ValidateAccessTokenResp>(DatabaseRequestTypes.ValidateAccessToken, ValidateAccessToken);
            RegisterRequestToServer<GetUserLevelReq, GetUserLevelResp>(DatabaseRequestTypes.GetUserLevel, GetUserLevel);
            RegisterRequestToServer<GetGoldReq, GoldResp>(DatabaseRequestTypes.GetGold, GetGold);
            RegisterRequestToServer<ChangeGoldReq, GoldResp>(DatabaseRequestTypes.ChangeGold, ChangeGold);
            RegisterRequestToServer<GetCashReq, CashResp>(DatabaseRequestTypes.GetCash, GetCash);
            RegisterRequestToServer<ChangeCashReq, CashResp>(DatabaseRequestTypes.ChangeCash, ChangeCash);
            RegisterRequestToServer<UpdateAccessTokenReq, EmptyMessage>(DatabaseRequestTypes.UpdateAccessToken, UpdateAccessToken);
            RegisterRequestToServer<CreateUserLoginReq, EmptyMessage>(DatabaseRequestTypes.CreateUserLogin, CreateUserLogin);
            RegisterRequestToServer<FindUsernameReq, FindUsernameResp>(DatabaseRequestTypes.FindUsername, FindUsername);
            RegisterRequestToServer<CreateCharacterReq, CharacterResp>(DatabaseRequestTypes.CreateCharacter, CreateCharacter);
            RegisterRequestToServer<GetCharacterReq, CharacterResp>(DatabaseRequestTypes.GetCharacter, GetCharacter);
            RegisterRequestToServer<GetCharactersReq, CharactersResp>(DatabaseRequestTypes.GetCharacters, GetCharacters);
            RegisterRequestToServer<UpdateCharacterReq, CharacterResp>(DatabaseRequestTypes.UpdateCharacter, UpdateCharacter);
            RegisterRequestToServer<DeleteCharacterReq, EmptyMessage>(DatabaseRequestTypes.DeleteCharacter, DeleteCharacter);
            RegisterRequestToServer<FindCharacterNameReq, FindCharacterNameResp>(DatabaseRequestTypes.FindCharacterName, FindCharacterName);
            RegisterRequestToServer<FindCharacterNameReq, SocialCharactersResp>(DatabaseRequestTypes.FindCharacters, FindCharacters);
            RegisterRequestToServer<CreateFriendReq, EmptyMessage>(DatabaseRequestTypes.CreateFriend, CreateFriend);
            RegisterRequestToServer<DeleteFriendReq, EmptyMessage>(DatabaseRequestTypes.DeleteFriend, DeleteFriend);
            RegisterRequestToServer<GetFriendsReq, SocialCharactersResp>(DatabaseRequestTypes.GetFriends, GetFriends);
            RegisterRequestToServer<GetFriendRequestNotificationReq, GetFriendRequestNotificationResp>(DatabaseRequestTypes.GetFriendRequestNotification, GetFriendRequestNotification);
            RegisterRequestToServer<CreateBuildingReq, BuildingResp>(DatabaseRequestTypes.CreateBuilding, CreateBuilding);
            RegisterRequestToServer<UpdateBuildingReq, BuildingResp>(DatabaseRequestTypes.UpdateBuilding, UpdateBuilding);
            RegisterRequestToServer<DeleteBuildingReq, EmptyMessage>(DatabaseRequestTypes.DeleteBuilding, DeleteBuilding);
            RegisterRequestToServer<GetBuildingsReq, BuildingsResp>(DatabaseRequestTypes.GetBuildings, GetBuildings);
            RegisterRequestToServer<CreatePartyReq, PartyResp>(DatabaseRequestTypes.CreateParty, CreateParty);
            RegisterRequestToServer<UpdatePartyReq, PartyResp>(DatabaseRequestTypes.UpdateParty, UpdateParty);
            RegisterRequestToServer<UpdatePartyLeaderReq, PartyResp>(DatabaseRequestTypes.UpdatePartyLeader, UpdatePartyLeader);
            RegisterRequestToServer<DeletePartyReq, EmptyMessage>(DatabaseRequestTypes.DeleteParty, DeleteParty);
            RegisterRequestToServer<UpdateCharacterPartyReq, PartyResp>(DatabaseRequestTypes.UpdateCharacterParty, UpdateCharacterParty);
            RegisterRequestToServer<ClearCharacterPartyReq, EmptyMessage>(DatabaseRequestTypes.ClearCharacterParty, ClearCharacterParty);
            RegisterRequestToServer<GetPartyReq, PartyResp>(DatabaseRequestTypes.GetParty, GetParty);
            RegisterRequestToServer<CreateGuildReq, GuildResp>(DatabaseRequestTypes.CreateGuild, CreateGuild);
            RegisterRequestToServer<UpdateGuildLeaderReq, GuildResp>(DatabaseRequestTypes.UpdateGuildLeader, UpdateGuildLeader);
            RegisterRequestToServer<UpdateGuildMessageReq, GuildResp>(DatabaseRequestTypes.UpdateGuildMessage, UpdateGuildMessage);
            RegisterRequestToServer<UpdateGuildMessageReq, GuildResp>(DatabaseRequestTypes.UpdateGuildMessage2, UpdateGuildMessage2);
            RegisterRequestToServer<UpdateGuildScoreReq, GuildResp>(DatabaseRequestTypes.UpdateGuildScore, UpdateGuildScore);
            RegisterRequestToServer<UpdateGuildOptionsReq, GuildResp>(DatabaseRequestTypes.UpdateGuildOptions, UpdateGuildOptions);
            RegisterRequestToServer<UpdateGuildAutoAcceptRequestsReq, GuildResp>(DatabaseRequestTypes.UpdateGuildAutoAcceptRequests, UpdateGuildAutoAcceptRequests);
            RegisterRequestToServer<UpdateGuildRankReq, GuildResp>(DatabaseRequestTypes.UpdateGuildRank, UpdateGuildRank);
            RegisterRequestToServer<UpdateGuildRoleReq, GuildResp>(DatabaseRequestTypes.UpdateGuildRole, UpdateGuildRole);
            RegisterRequestToServer<UpdateGuildMemberRoleReq, GuildResp>(DatabaseRequestTypes.UpdateGuildMemberRole, UpdateGuildMemberRole);
            RegisterRequestToServer<DeleteGuildReq, EmptyMessage>(DatabaseRequestTypes.DeleteGuild, DeleteGuild);
            RegisterRequestToServer<UpdateCharacterGuildReq, GuildResp>(DatabaseRequestTypes.UpdateCharacterGuild, UpdateCharacterGuild);
            RegisterRequestToServer<ClearCharacterGuildReq, EmptyMessage>(DatabaseRequestTypes.ClearCharacterGuild, ClearCharacterGuild);
            RegisterRequestToServer<FindGuildNameReq, FindGuildNameResp>(DatabaseRequestTypes.FindGuildName, FindGuildName);
            RegisterRequestToServer<GetGuildReq, GuildResp>(DatabaseRequestTypes.GetGuild, GetGuild);
            RegisterRequestToServer<IncreaseGuildExpReq, GuildResp>(DatabaseRequestTypes.IncreaseGuildExp, IncreaseGuildExp);
            RegisterRequestToServer<AddGuildSkillReq, GuildResp>(DatabaseRequestTypes.AddGuildSkill, AddGuildSkill);
            RegisterRequestToServer<GetGuildGoldReq, GuildGoldResp>(DatabaseRequestTypes.GetGuildGold, GetGuildGold);
            RegisterRequestToServer<ChangeGuildGoldReq, GuildGoldResp>(DatabaseRequestTypes.ChangeGuildGold, ChangeGuildGold);
            RegisterRequestToServer<GetStorageItemsReq, GetStorageItemsResp>(DatabaseRequestTypes.GetStorageItems, GetStorageItems);
            RegisterRequestToServer<UpdateStorageItemsReq, EmptyMessage>(DatabaseRequestTypes.UpdateStorageItems, UpdateStorageItems);
            RegisterRequestToServer<EmptyMessage, EmptyMessage>(DatabaseRequestTypes.DeleteAllReservedStorage, DeleteAllReservedStorage);
            RegisterRequestToServer<MailListReq, MailListResp>(DatabaseRequestTypes.MailList, MailList);
            RegisterRequestToServer<UpdateReadMailStateReq, UpdateReadMailStateResp>(DatabaseRequestTypes.UpdateReadMailState, UpdateReadMailState);
            RegisterRequestToServer<UpdateClaimMailItemsStateReq, UpdateClaimMailItemsStateResp>(DatabaseRequestTypes.UpdateClaimMailItemsState, UpdateClaimMailItemsState);
            RegisterRequestToServer<UpdateDeleteMailStateReq, UpdateDeleteMailStateResp>(DatabaseRequestTypes.UpdateDeleteMailState, UpdateDeleteMailState);
            RegisterRequestToServer<SendMailReq, SendMailResp>(DatabaseRequestTypes.SendMail, SendMail);
            RegisterRequestToServer<GetMailReq, GetMailResp>(DatabaseRequestTypes.GetMail, GetMail);
            RegisterRequestToServer<GetMailNotificationReq, GetMailNotificationResp>(DatabaseRequestTypes.GetMailNotification, GetMailNotification);
            RegisterRequestToServer<GetIdByCharacterNameReq, GetIdByCharacterNameResp>(DatabaseRequestTypes.GetIdByCharacterName, GetIdByCharacterName);
            RegisterRequestToServer<GetUserIdByCharacterNameReq, GetUserIdByCharacterNameResp>(DatabaseRequestTypes.GetUserIdByCharacterName, GetUserIdByCharacterName);
            RegisterRequestToServer<GetUserUnbanTimeReq, GetUserUnbanTimeResp>(DatabaseRequestTypes.GetUserUnbanTime, GetUserUnbanTime);
            RegisterRequestToServer<SetUserUnbanTimeByCharacterNameReq, EmptyMessage>(DatabaseRequestTypes.SetUserUnbanTimeByCharacterName, SetUserUnbanTimeByCharacterName);
            RegisterRequestToServer<SetCharacterUnmuteTimeByNameReq, EmptyMessage>(DatabaseRequestTypes.SetCharacterUnmuteTimeByName, SetCharacterUnmuteTimeByName);
            RegisterRequestToServer<GetSummonBuffsReq, GetSummonBuffsResp>(DatabaseRequestTypes.GetSummonBuffs, GetSummonBuffs);
            RegisterRequestToServer<FindEmailReq, FindEmailResp>(DatabaseRequestTypes.FindEmail, FindEmail);
            RegisterRequestToServer<ValidateEmailVerificationReq, ValidateEmailVerificationResp>(DatabaseRequestTypes.ValidateEmailVerification, ValidateEmailVerification);
            RegisterRequestToServer<UpdateUserCountReq, EmptyMessage>(DatabaseRequestTypes.UpdateUserCount, UpdateUserCount);
            RegisterRequestToServer<GetSocialCharacterReq, SocialCharacterResp>(DatabaseRequestTypes.GetSocialCharacter, GetSocialCharacter);
            RegisterRequestToServer<FindGuildNameReq, GuildsResp>(DatabaseRequestTypes.FindGuilds, FindGuilds);
            RegisterRequestToServer<CreateGuildRequestReq, EmptyMessage>(DatabaseRequestTypes.CreateGuildRequest, CreateGuildRequest);
            RegisterRequestToServer<DeleteGuildRequestReq, EmptyMessage>(DatabaseRequestTypes.DeleteGuildRequest, DeleteGuildRequest);
            RegisterRequestToServer<GetGuildRequestsReq, SocialCharactersResp>(DatabaseRequestTypes.GetGuildRequests, GetGuildRequests);
            RegisterRequestToServer<GetGuildRequestNotificationReq, GetGuildRequestNotificationResp>(DatabaseRequestTypes.GetGuildRequestNotification, GetGuildRequestNotification);
            RegisterRequestToServer<UpdateGuildMemberCountReq, EmptyMessage>(DatabaseRequestTypes.UpdateGuildMemberCount, UpdateGuildMemberCount);
            this.InvokeInstanceDevExtMethods("RegisterMessages");
        }
    }
}