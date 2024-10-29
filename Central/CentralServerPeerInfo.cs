using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct CentralServerPeerInfo : INetSerializable
    {
        public long connectionId;
        public CentralServerPeerType peerType;
        public string networkAddress;
        public int networkPort;
        public string channelId;
        public string refId;
        public int currentUsers;
        public int maxUsers;

        public void Deserialize(NetDataReader reader)
        {
            peerType = (CentralServerPeerType)reader.GetByte();
            networkAddress = reader.GetString();
            networkPort = reader.GetInt();
            channelId = reader.GetString();
            refId = reader.GetString();
            currentUsers = reader.GetPackedInt();
            maxUsers = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)peerType);
            writer.Put(networkAddress);
            writer.Put(networkPort);
            writer.Put(channelId);
            writer.Put(refId);
            writer.PutPackedInt(currentUsers);
            writer.PutPackedInt(maxUsers);
        }
    }
}
