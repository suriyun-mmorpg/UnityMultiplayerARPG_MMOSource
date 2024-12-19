using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct UpdateCharacterReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            State = (TransactionUpdateCharacterState)reader.GetPackedInt();
            CharacterData = reader.Get(() => new PlayerCharacterData());
            SummonBuffs = reader.GetList<CharacterBuff>();
            bool isNull;
            isNull = reader.GetBool();
            if (!isNull)
                StorageItems = reader.GetList<CharacterItem>();
            else
                StorageItems = null;
            isNull = reader.GetBool();
            if (!isNull)
                ProtectedStorageItems = reader.GetList<CharacterItem>();
            else
                ProtectedStorageItems = null;
            DeleteStorageReservation = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt((int)State);
            writer.Put(CharacterData);
            writer.PutList(SummonBuffs);
            bool isNull;
            isNull = StorageItems == null;
            writer.Put(isNull);
            if (!isNull)
                writer.PutList(StorageItems);
            isNull = ProtectedStorageItems == null;
            writer.Put(isNull);
            if (!isNull)
                writer.PutList(ProtectedStorageItems);
            writer.Put(DeleteStorageReservation);
        }
    }
}