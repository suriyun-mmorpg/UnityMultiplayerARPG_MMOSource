using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct CreateBuildingReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            ChannelId = reader.GetString();
            MapName = reader.GetString();
            BuildingData = reader.Get(() => new BuildingSaveData());
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ChannelId);
            writer.Put(MapName);
            writer.Put(BuildingData);
        }
    }
}