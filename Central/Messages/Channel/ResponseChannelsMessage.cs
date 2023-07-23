using System.Collections.Generic;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct ResponseChannelsMessage : INetSerializable
    {
        public UITextKeys message;
        public List<ChannelEntry> channels;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            channels = reader.GetList<ChannelEntry>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            writer.PutList(channels);
        }
    }
}
