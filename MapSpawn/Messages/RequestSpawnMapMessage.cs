﻿using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public struct RequestSpawnMapMessage : INetSerializable
    {
        public string channelId;
        public string mapName;
        public string instanceId;
        public Vector3 instanceWarpPosition;
        public bool instanceWarpOverrideRotation;
        public Vector3 instanceWarpRotation;

        public void Deserialize(NetDataReader reader)
        {
            channelId = reader.GetString();
            mapName = reader.GetString();
            instanceId = reader.GetString();
            if (!string.IsNullOrEmpty(instanceId))
            {
                instanceWarpPosition = reader.GetVector3();
                instanceWarpOverrideRotation = reader.GetBool();
                instanceWarpRotation = reader.GetVector3();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(channelId);
            writer.Put(mapName);
            writer.Put(instanceId);
            if (!string.IsNullOrEmpty(instanceId))
            {
                writer.PutVector3(instanceWarpPosition);
                writer.Put(instanceWarpOverrideRotation);
                writer.PutVector3(instanceWarpRotation);
            }
        }
    }
}
