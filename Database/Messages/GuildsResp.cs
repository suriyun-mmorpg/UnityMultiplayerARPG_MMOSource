using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct GuildsResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            List = reader.GetList<GuildListEntry>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutList(List);
        }
    }
}