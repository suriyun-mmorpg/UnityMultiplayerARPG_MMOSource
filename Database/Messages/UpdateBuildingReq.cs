using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct UpdateBuildingReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            State = (TransactionUpdateBuildingState)reader.GetPackedUInt();
            ChannelId = reader.GetString();
            MapName = reader.GetString();
            BuildingData = reader.Get(() => new BuildingSaveData());
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUInt((uint)State);
            writer.Put(ChannelId);
            writer.Put(MapName);
            writer.Put(BuildingData);
        }
    }
}