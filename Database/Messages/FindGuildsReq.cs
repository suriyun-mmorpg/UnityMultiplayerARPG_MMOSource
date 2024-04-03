using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct FindGuildsReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            GuildName = reader.GetString();
            FinderCharacterId = reader.GetString();
            Skip = reader.GetPackedInt();
            Take = reader.GetPackedInt();
            FieldOptions = (GuildListFieldOptions)reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildName);
            writer.Put(FinderCharacterId);
            writer.PutPackedInt(Skip);
            writer.PutPackedInt(Take);
            writer.PutPackedInt((int)FieldOptions);
        }
    }
}