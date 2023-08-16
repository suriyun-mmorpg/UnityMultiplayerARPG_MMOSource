using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct UpdateCharacterReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            CharacterData = reader.Get(() => new PlayerCharacterData());
            ExtraContent = (EExtraContent)reader.GetByte();
            if (HasExtraContent(EExtraContent.StorageItems))
                StorageItems = reader.GetList<CharacterItem>();
            if (HasExtraContent(EExtraContent.SummonBuffs))
                SummonBuffs = reader.GetList<CharacterBuff>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CharacterData);
            writer.Put((byte)ExtraContent);
            if (HasExtraContent(EExtraContent.StorageItems))
                writer.PutList(StorageItems);
            if (HasExtraContent(EExtraContent.SummonBuffs))
                writer.PutList(SummonBuffs);
        }
    }
}