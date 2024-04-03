using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct ReadFriendsReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            CharacterId = reader.GetString();
            ReadById2 = reader.GetBool();
            State = reader.GetByte();
            Skip = reader.GetPackedInt();
            Limit = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CharacterId);
            writer.Put(ReadById2);
            writer.Put(State);
            writer.PutPackedInt(Skip);
            writer.PutPackedInt(Limit);
        }
    }
}