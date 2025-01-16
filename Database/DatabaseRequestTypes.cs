﻿namespace MultiplayerARPG.MMO
{
    public static partial class DatabaseRequestTypes
    {
        public const ushort ValidateUserLogin = 1;
        public const ushort ValidateAccessToken = 2;
        public const ushort GetUserLevel = 3;
        public const ushort GetGold = 4;
        public const ushort ChangeGold = 5;
        public const ushort GetCash = 6;
        public const ushort ChangeCash = 7;
        public const ushort UpdateAccessToken = 8;
        public const ushort CreateUserLogin = 9;
        public const ushort FindUsername = 10;
        public const ushort CreateCharacter = 11;
        public const ushort GetCharacter = 12;
        public const ushort GetCharacters = 13;
        public const ushort UpdateCharacter = 14;
        public const ushort DeleteCharacter = 15;
        public const ushort FindCharacterName = 16;
        public const ushort FindCharacters = 17;
        public const ushort CreateFriend = 18;
        public const ushort DeleteFriend = 19;
        public const ushort GetFriends = 20;
        public const ushort CreateBuilding = 21;
        public const ushort UpdateBuilding = 22;
        public const ushort DeleteBuilding = 23;
        public const ushort GetBuildings = 24;
        public const ushort CreateParty = 25;
        public const ushort UpdateParty = 26;
        public const ushort UpdatePartyLeader = 27;
        public const ushort DeleteParty = 28;
        public const ushort UpdateCharacterParty = 29;
        public const ushort ClearCharacterParty = 30;
        public const ushort GetParty = 31;
        public const ushort CreateGuild = 32;
        public const ushort UpdateGuildLeader = 33;
        public const ushort UpdateGuildMessage = 34;
        public const ushort UpdateGuildRole = 35;
        public const ushort UpdateGuildMemberRole = 36;
        public const ushort DeleteGuild = 37;
        public const ushort UpdateCharacterGuild = 38;
        public const ushort ClearCharacterGuild = 39;
        public const ushort FindGuildName = 40;
        public const ushort GetGuild = 41;
        public const ushort IncreaseGuildExp = 42;
        public const ushort AddGuildSkill = 43;
        public const ushort GetGuildGold = 44;
        public const ushort ChangeGuildGold = 45;
        public const ushort GetStorageItems = 46;
        public const ushort UpdateStorageItems = 47;
        public const ushort DeleteAllReservedStorage = 48;
        public const ushort MailList = 52;
        public const ushort UpdateReadMailState = 53;
        public const ushort UpdateClaimMailItemsState = 54;
        public const ushort UpdateDeleteMailState = 55;
        public const ushort SendMail = 56;
        public const ushort GetMail = 57;
        public const ushort GetIdByCharacterName = 58;
        public const ushort GetUserIdByCharacterName = 59;
        public const ushort UpdateGuildMessage2 = 60;
        public const ushort UpdateGuildScore = 61;
        public const ushort UpdateGuildOptions = 62;
        public const ushort UpdateGuildAutoAcceptRequests = 67;
        public const ushort UpdateGuildRank = 68;
        public const ushort GetMailNotification = 69;
        public const ushort GetUserUnbanTime = 70;
        public const ushort SetUserUnbanTimeByCharacterName = 71;
        public const ushort SetCharacterUnmuteTimeByName = 72;
        public const ushort GetSummonBuffs = 73;
        public const ushort ValidateEmailVerification = 75;
        public const ushort FindEmail = 76;
        public const ushort GetFriendRequestNotification = 77;
        public const ushort UpdateUserCount = 78;
        public const ushort GetSocialCharacter = 79;
        public const ushort FindGuilds = 80;
        public const ushort CreateGuildRequest = 81;
        public const ushort DeleteGuildRequest = 82;
        public const ushort GetGuildRequests = 83;
        public const ushort GetGuildRequestNotification = 84;
        public const ushort UpdateGuildMemberCount = 85;
        public const ushort UpdateStorageAndCharacterItems = 86;
    }
}