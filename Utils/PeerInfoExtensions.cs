namespace MultiplayerARPG.MMO
{
    public static class PeerInfoExtensions
    {

        public static string GetPeerInfoKey(this CentralServerPeerInfo peerInfo)
        {
            return GetPeerInfoKey(peerInfo.channelId, peerInfo.refId);
        }

        public static string GetPeerInfoKey(this RequestAppServerAddressMessage msg)
        {
            return GetPeerInfoKey(msg.channelId, msg.refId);
        }

        public static string GetPeerInfoKey(string channelId, string refId)
        {
            return $"{channelId}_{refId}";
        }
    }
}
