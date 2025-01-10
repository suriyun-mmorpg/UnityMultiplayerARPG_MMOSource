using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct DbRequestMessage<T> : INetSerializable 
        where T : struct, INetSerializable
    {
        public long RequestTimeUtc { get; set; }
        public T Data { get; set; }
        
        public void Deserialize(NetDataReader reader)
        {
            RequestTimeUtc = reader.GetPackedLong();
            Data = reader.Get<T>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedLong(RequestTimeUtc);
            writer.Put(Data);
        }
    }
}