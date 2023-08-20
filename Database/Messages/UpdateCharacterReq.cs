using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct UpdateCharacterReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            CharacterData = reader.Get(() => new PlayerCharacterData());
            SummonBuffs = reader.GetList<CharacterBuff>();
            bool isNull = reader.GetBool();
            if (!isNull)
                StorageItems = reader.GetList<CharacterItem>();
            else
                StorageItems = null;
            DeleteStorageReservation = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CharacterData);
            writer.PutList(SummonBuffs);
            bool isNull = StorageItems == null;
            writer.Put(isNull);
            if (!isNull)
                writer.PutList(StorageItems);
            writer.Put(DeleteStorageReservation);
        }
    }
}