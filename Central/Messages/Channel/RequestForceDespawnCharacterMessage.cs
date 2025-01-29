using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct RequestForceDespawnCharacterMessage : INetSerializable
    {
        public string userId;

        public void Deserialize(NetDataReader reader)
        {
            userId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(userId);
        }
    }
}
