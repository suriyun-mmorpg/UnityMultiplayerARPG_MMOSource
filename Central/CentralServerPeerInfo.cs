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

        public void Deserialize(NetDataReader reader)
        {
            peerType = (CentralServerPeerType)reader.GetByte();
            networkAddress = reader.GetString();
            networkPort = reader.GetInt();
            channelId = reader.GetString();
            refId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)peerType);
            writer.Put(networkAddress);
            writer.Put(networkPort);
            writer.Put(channelId);
            writer.Put(refId);
        }
    }
}
