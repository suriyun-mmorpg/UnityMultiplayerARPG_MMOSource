using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct PartyResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            bool notNull = reader.GetBool();
            if (notNull)
                PartyData = reader.Get(() => new PartyData());
        }

        public void Serialize(NetDataWriter writer)
        {
            bool notNull = PartyData != null;
            writer.Put(notNull);
            if (notNull)
                writer.Put(PartyData);
        }
    }
}