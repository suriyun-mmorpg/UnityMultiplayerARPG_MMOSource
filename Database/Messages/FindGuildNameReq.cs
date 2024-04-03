using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct FindGuildNameReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            FinderId = reader.GetString();
            GuildName = reader.GetString();
            Skip = reader.GetPackedInt();
            Limit = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(FinderId);
            writer.Put(GuildName);
            writer.PutPackedInt(Skip);
            writer.PutPackedInt(Limit);
        }
    }
}