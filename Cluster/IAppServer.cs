namespace MultiplayerARPG.MMO
{
    public interface IAppServer
    {
        string ClusterServerAddress { get; }
        int ClusterServerPort { get; }
        string AppAddress { get; }
        int AppPort { get; }
        string ChannelId { get; }
        string RefId { get; }
        CentralServerPeerType PeerType { get; }
    }
}
