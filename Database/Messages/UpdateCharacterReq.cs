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
                PlayerStorageItems = reader.GetList<CharacterItem>();
            else
                PlayerStorageItems = null;
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
            isNull = PlayerStorageItems == null;
            writer.Put(isNull);
            if (!isNull)
                writer.PutList(PlayerStorageItems);
            isNull = ProtectedStorageItems == null;
            writer.Put(isNull);
            if (!isNull)
                writer.PutList(ProtectedStorageItems);
            writer.Put(DeleteStorageReservation);
        }
    }
}