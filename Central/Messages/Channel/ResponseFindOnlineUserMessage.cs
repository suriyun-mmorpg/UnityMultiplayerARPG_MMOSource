using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ResponseFindOnlineUserMessage : INetSerializable
    {
        public string userId;
        public bool isFound;

        public void Deserialize(NetDataReader reader)
        {
            userId = reader.GetString();
            isFound = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(userId);
            writer.Put(isFound);
        }
    }
}
