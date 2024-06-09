using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct UpdateGuildMemberCountReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetInt();
            MaxGuildMember = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildId);
            writer.Put(MaxGuildMember);
        }
    }
}