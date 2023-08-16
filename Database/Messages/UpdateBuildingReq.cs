using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct UpdateBuildingReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            ChannelId = reader.GetString();
            MapName = reader.GetString();
            BuildingData = reader.Get(() => new BuildingSaveData());
            bool isNull = reader.GetBool();
            if (!isNull)
                StorageItems = reader.GetList<CharacterItem>();
            else
                StorageItems = null;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ChannelId);
            writer.Put(MapName);
            writer.Put(BuildingData);
            bool isNull = StorageItems == null;
            writer.Put(isNull);
            if (!isNull)
                writer.PutList(StorageItems);
        }
    }
}