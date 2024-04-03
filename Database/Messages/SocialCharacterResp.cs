using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct SocialCharacterResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            SocialCharacterData = reader.Get<SocialCharacterData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(SocialCharacterData);
        }
    }
}