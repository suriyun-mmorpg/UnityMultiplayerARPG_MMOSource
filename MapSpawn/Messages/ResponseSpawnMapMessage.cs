using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ResponseSpawnMapMessage : INetSerializable
    {
        public UITextKeys message;
        public CentralServerPeerInfo peerInfo;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            if (!message.IsError())
                peerInfo = reader.Get<CentralServerPeerInfo>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            if (!message.IsError())
                writer.Put(peerInfo);
        }
    }
}
