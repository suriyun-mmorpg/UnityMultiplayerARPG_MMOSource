namespace MultiplayerARPG.MMO
{
    public static partial class MMORequestTypes
    {
        public const ushort AppServerRegister = 0;
        public const ushort AppServerAddress = 1;
        public const ushort UserLogin = 2;
        public const ushort UserRegister = 3;
        public const ushort UserLogout = 4;
        public const ushort Characters = 5;
        public const ushort CreateCharacter = 6;
        public const ushort DeleteCharacter = 7;
        public const ushort SelectCharacter = 8;
        public const ushort SpawnMap = 9;
        public const ushort ValidateAccessToken = 10;
        public const ushort UserCount = 11;
        public const ushort Channels = 12;
        public const ushort ForceDespawnCharacter = 13;
        public const ushort RunMap = 14;
        public const ushort FindOnlineUser = 15;
    }
}
