using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ChannelEntry : INetSerializable
    {
        public string id;
        public string title;
        public int connections;
        public int maxConnections;

        public void Deserialize(NetDataReader reader)
        {
            id = reader.GetString();
            title = reader.GetString();
            connections = reader.GetPackedInt();
            maxConnections = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(id);
            writer.Put(title);
            writer.PutPackedInt(connections);
            writer.PutPackedInt(maxConnections);
        }
    }
}
