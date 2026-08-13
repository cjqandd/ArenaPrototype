using UnityEngine;

namespace ArenaPrototype.Art
{
    public enum ArenaCharacterRole
    {
        Player,
        Swordsman,
        Archer,
        ShieldBearer
    }

    [CreateAssetMenu(menuName = "Arena Prototype/美术目录", fileName = "ArenaArtCatalog")]
    public sealed class ArenaArtCatalog : ScriptableObject
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject swordsmanPrefab;
        [SerializeField] private GameObject archerPrefab;
        [SerializeField] private GameObject shieldBearerPrefab;

        public bool IsComplete => playerPrefab != null
            && swordsmanPrefab != null
            && archerPrefab != null
            && shieldBearerPrefab != null;

        public GameObject GetCharacterPrefab(ArenaCharacterRole role)
        {
            return role switch
            {
                ArenaCharacterRole.Player => playerPrefab,
                ArenaCharacterRole.Swordsman => swordsmanPrefab,
                ArenaCharacterRole.Archer => archerPrefab,
                ArenaCharacterRole.ShieldBearer => shieldBearerPrefab,
                _ => null
            };
        }

        public void Configure(
            GameObject newPlayerPrefab,
            GameObject newSwordsmanPrefab,
            GameObject newArcherPrefab,
            GameObject newShieldBearerPrefab)
        {
            playerPrefab = newPlayerPrefab;
            swordsmanPrefab = newSwordsmanPrefab;
            archerPrefab = newArcherPrefab;
            shieldBearerPrefab = newShieldBearerPrefab;
        }
    }
}
