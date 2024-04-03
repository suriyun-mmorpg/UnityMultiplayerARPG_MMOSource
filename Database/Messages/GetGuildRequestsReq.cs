using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct GetGuildRequestsReq
    {
        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(GuildId);
        }
    }
}