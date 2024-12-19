using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct UpdateCharacterReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            State = (TransactionUpdateCharacterState)reader.GetPackedUInt();
            CharacterData = reader.Get(() => new PlayerCharacterData());
            SummonBuffs = reader.GetList<CharacterBuff>();
            if (State.Has(TransactionUpdateCharacterState.PlayerStorageItems))
                PlayerStorageItems = reader.GetList<CharacterItem>();
            else
                PlayerStorageItems = null;
            if (State.Has(TransactionUpdateCharacterState.ProtectedStorageItems))
                ProtectedStorageItems = reader.GetList<CharacterItem>();
            else
                ProtectedStorageItems = null;
            DeleteStorageReservation = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUInt((uint)State);
            writer.Put(CharacterData);
            writer.PutList(SummonBuffs);
            if (State.Has(TransactionUpdateCharacterState.PlayerStorageItems))
                writer.PutList(PlayerStorageItems);
            if (State.Has(TransactionUpdateCharacterState.ProtectedStorageItems))
                writer.PutList(ProtectedStorageItems);
            writer.Put(DeleteStorageReservation);
        }
    }
}