using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct RequestSpawnMapMessage : INetSerializable
    {
        public string channelId;
        public string mapName;
        public string instanceId;
        public Vec3 instanceWarpPosition;
        public bool instanceWarpOverrideRotation;
        public Vec3 instanceWarpRotation;

        public void Deserialize(NetDataReader reader)
        {
            channelId = reader.GetString();
            mapName = reader.GetString();
            instanceId = reader.GetString();
            if (!string.IsNullOrEmpty(instanceId))
            {
                instanceWarpPosition = reader.GetValue<Vec3>();
                instanceWarpOverrideRotation = reader.GetBool();
                instanceWarpRotation = reader.GetValue<Vec3>();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(channelId);
            writer.Put(mapName);
            writer.Put(instanceId);
            if (!string.IsNullOrEmpty(instanceId))
            {
                writer.Put(instanceWarpPosition);
                writer.Put(instanceWarpOverrideRotation);
                writer.Put(instanceWarpRotation);
            }
        }
    }
}
