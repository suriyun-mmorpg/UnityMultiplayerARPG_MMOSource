#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
namespace MultiplayerARPG.MMO
{
    [System.Serializable]
    public struct SpawnAllocateMapData
    {
        public BaseMapInfo mapInfo;
        public int allocateAmount;
    }
}
#endif