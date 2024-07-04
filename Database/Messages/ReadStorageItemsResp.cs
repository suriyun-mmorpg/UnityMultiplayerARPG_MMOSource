using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct GetStorageItemsResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            Error = (UITextKeys)reader.GetByte();
            StorageItems = reader.GetList<CharacterItem>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Error);
            writer.PutList(StorageItems);
        }
    }
}