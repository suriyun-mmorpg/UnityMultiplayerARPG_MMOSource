using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct FindCharacterNameReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            FinderId = reader.GetString();
            CharacterName = reader.GetString();
            Skip = reader.GetPackedInt();
            Limit = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(FinderId);
            writer.Put(CharacterName);
            writer.PutPackedInt(Skip);
            writer.PutPackedInt(Limit);
        }
    }
}
