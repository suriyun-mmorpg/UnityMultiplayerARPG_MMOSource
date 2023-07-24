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
            if (message == UITextKeys.NONE)
                peerInfo = reader.Get<CentralServerPeerInfo>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            if (message == UITextKeys.NONE)
                writer.Put(peerInfo);
        }
    }
}
