﻿using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public partial struct GetPartyReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            PartyId = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(PartyId);
        }
    }
}