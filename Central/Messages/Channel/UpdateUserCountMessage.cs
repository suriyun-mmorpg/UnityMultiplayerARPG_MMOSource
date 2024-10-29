using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct UpdateUserCountMessage : INetSerializable
    {
        public int currentUsers;
        public int maxUsers;

        public void Deserialize(NetDataReader reader)
        {
            currentUsers = reader.GetPackedInt();
            maxUsers = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(currentUsers);
            writer.PutPackedInt(maxUsers);
        }
    }
}
