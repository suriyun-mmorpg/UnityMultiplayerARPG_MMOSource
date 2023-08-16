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
            ExtraContent = (EExtraContent)reader.GetByte();
            if (HasExtraContent(EExtraContent.StorageItems))
                StorageItems = reader.GetList<CharacterItem>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ChannelId);
            writer.Put(MapName);
            writer.Put(BuildingData);
            writer.Put((byte)ExtraContent);
            if (HasExtraContent(EExtraContent.StorageItems))
                writer.PutList(StorageItems);
        }
    }
}