using System;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct DataPatchReloadMessage : INetSerializable
    {
        public string requestId;
        public string databaseId;
        public string releaseId;
        public string payloadHash;

        public bool IsValid
        {
            get
            {
                if (!Guid.TryParse(requestId, out _) || string.IsNullOrWhiteSpace(databaseId) || databaseId.Length > 128 ||
                    releaseId == null || releaseId.Length > 128 || payloadHash == null)
                    return false;
                if (releaseId.Length == 0)
                    return payloadHash.Length == 0;
                if (payloadHash.Length != 64)
                    return false;
                foreach (char value in payloadHash)
                    if (!Uri.IsHexDigit(value))
                        return false;
                return true;
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            requestId = reader.GetString();
            databaseId = reader.GetString();
            releaseId = reader.GetString();
            payloadHash = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(requestId);
            writer.Put(databaseId);
            writer.Put(releaseId);
            writer.Put(payloadHash);
        }
    }
}
