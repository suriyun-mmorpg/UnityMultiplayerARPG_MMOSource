using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct RequestAppServerAddressMessage : INetSerializable
    {
        public CentralServerPeerType peerType;
        public string channelId;
        public string refId;

        public void Deserialize(NetDataReader reader)
        {
            peerType = (CentralServerPeerType)reader.GetByte();
            channelId = reader.GetString();
            refId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)peerType);
            writer.Put(channelId);
            writer.Put(refId);
        }
    }
}
