using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct RequestSelectCharacterMessage : INetSerializable
    {
        public string channelId;
        public string characterId;

        public void Deserialize(NetDataReader reader)
        {
            channelId = reader.GetString();
            characterId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(channelId);
            writer.Put(characterId);
        }
    }
}
