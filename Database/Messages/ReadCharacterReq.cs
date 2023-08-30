using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct ReadCharacterReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            UserId = reader.GetString();
            CharacterId = reader.GetString();
            ForceClearCache = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserId);
            writer.Put(CharacterId);
            writer.Put(ForceClearCache);
        }
    }
}