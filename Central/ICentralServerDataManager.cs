using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public interface ICentralServerDataManager
    {
        string GenerateCharacterId();
        string GenerateMapSpawnRequestId();
        bool CanCreateCharacter(int dataId, int entityId, int factionId, IList<CharacterDataBoolean> publicBools, IList<CharacterDataInt32> publicInts, IList<CharacterDataFloat32> publicFloats);
        void SetNewPlayerCharacterData(PlayerCharacterData playerCharacterData, string characterName, int dataId, int entityId, int factionId, IList<CharacterDataBoolean> publicBools, IList<CharacterDataInt32> publicInts, IList<CharacterDataFloat32> publicFloats);
    }
}
