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
            DeleteStorageReservation = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUInt((uint)State);
            writer.Put(CharacterData);
            writer.PutList(SummonBuffs);
            writer.Put(DeleteStorageReservation);
        }
    }
}