using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct UpdateStorageAndCharacterItemsReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            StorageType = (StorageType)reader.GetByte();
            StorageOwnerId = reader.GetString();
            StorageItems = reader.GetList<CharacterItem>();
            CharacterId = reader.GetString();
            SelectableWeaponSets = reader.GetList<EquipWeapons>();
            EquipItems = reader.GetList<CharacterItem>();
            NonEquipItems = reader.GetList<CharacterItem>();
            DeleteStorageReservation = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)StorageType);
            writer.Put(StorageOwnerId);
            writer.PutList(StorageItems);
            writer.Put(CharacterId);
            writer.PutList(SelectableWeaponSets);
            writer.PutList(EquipItems);
            writer.PutList(NonEquipItems);
            writer.Put(DeleteStorageReservation);
        }
    }
}
