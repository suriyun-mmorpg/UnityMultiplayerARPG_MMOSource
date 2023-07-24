#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
namespace MultiplayerARPG.MMO
{
    [System.Serializable]
    public struct InstanceMapAllocatingData
    {
        public BaseMapInfo mapInfo;
        public int allocatingAmount;
    }
}
#endif