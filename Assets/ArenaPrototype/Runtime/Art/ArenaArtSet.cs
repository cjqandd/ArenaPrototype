using UnityEngine;

namespace ArenaPrototype.Art
{
    [CreateAssetMenu(menuName = "Arena Prototype/美术/美术套装", fileName = "ArenaArtSet")]
    public sealed class ArenaArtSet : ScriptableObject
    {
        [SerializeField] private string displayName = "New Art Set";
        [SerializeField] private bool proceduralGraybox;

        [Header("角色身份")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject swordsmanPrefab;
        [SerializeField] private GameObject archerPrefab;
        [SerializeField] private GameObject shieldBearerPrefab;

        [Header("武器与装备")]
        [SerializeField] private EquipmentVisualProfile sword;
        [SerializeField] private EquipmentVisualProfile chainBlade;
        [SerializeField] private EquipmentVisualProfile bow;
        [SerializeField] private EquipmentVisualProfile arrow;
        [SerializeField] private EquipmentVisualProfile shield;

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public bool ProceduralGraybox => proceduralGraybox;

        public GameObject GetCharacterPrefab(ArenaCharacterRole role)
        {
            if (proceduralGraybox)
            {
                return null;
            }

            return role switch
            {
                ArenaCharacterRole.Player => playerPrefab,
                ArenaCharacterRole.Swordsman => swordsmanPrefab,
                ArenaCharacterRole.Archer => archerPrefab,
                ArenaCharacterRole.ShieldBearer => shieldBearerPrefab,
                _ => null
            };
        }

        public EquipmentVisualProfile GetEquipmentProfile(ArenaEquipmentRole role)
        {
            if (proceduralGraybox)
            {
                return null;
            }

            return role switch
            {
                ArenaEquipmentRole.Sword => sword,
                ArenaEquipmentRole.ChainBlade => chainBlade,
                ArenaEquipmentRole.Bow => bow,
                ArenaEquipmentRole.Arrow => arrow,
                ArenaEquipmentRole.Shield => shield,
                _ => null
            };
        }

        public void SetCharacterPrefab(ArenaCharacterRole role, GameObject prefab)
        {
            switch (role)
            {
                case ArenaCharacterRole.Player:
                    playerPrefab = prefab;
                    break;
                case ArenaCharacterRole.Swordsman:
                    swordsmanPrefab = prefab;
                    break;
                case ArenaCharacterRole.Archer:
                    archerPrefab = prefab;
                    break;
                case ArenaCharacterRole.ShieldBearer:
                    shieldBearerPrefab = prefab;
                    break;
            }
        }

        public void SetEquipmentProfile(
            ArenaEquipmentRole role,
            EquipmentVisualProfile profile)
        {
            switch (role)
            {
                case ArenaEquipmentRole.Sword:
                    sword = profile;
                    break;
                case ArenaEquipmentRole.ChainBlade:
                    chainBlade = profile;
                    break;
                case ArenaEquipmentRole.Bow:
                    bow = profile;
                    break;
                case ArenaEquipmentRole.Arrow:
                    arrow = profile;
                    break;
                case ArenaEquipmentRole.Shield:
                    shield = profile;
                    break;
            }
        }

        public void Configure(
            string newDisplayName,
            bool newProceduralGraybox,
            GameObject newPlayerPrefab,
            GameObject newSwordsmanPrefab,
            GameObject newArcherPrefab,
            GameObject newShieldBearerPrefab,
            EquipmentVisualProfile newSword,
            EquipmentVisualProfile newChainBlade,
            EquipmentVisualProfile newBow,
            EquipmentVisualProfile newArrow,
            EquipmentVisualProfile newShield)
        {
            displayName = newDisplayName;
            proceduralGraybox = newProceduralGraybox;
            playerPrefab = newPlayerPrefab;
            swordsmanPrefab = newSwordsmanPrefab;
            archerPrefab = newArcherPrefab;
            shieldBearerPrefab = newShieldBearerPrefab;
            sword = newSword;
            chainBlade = newChainBlade;
            bow = newBow;
            arrow = newArrow;
            shield = newShield;
        }
    }
}
