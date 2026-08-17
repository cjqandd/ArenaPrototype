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
        [Header("当前美术套装")]
        [SerializeField] private ArenaArtSet activeArtSet;
        [SerializeField] private ArenaArtSet fallbackArtSet;

        [Header("旧版角色引用（自动迁移后仅作兼容）")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject swordsmanPrefab;
        [SerializeField] private GameObject archerPrefab;
        [SerializeField] private GameObject shieldBearerPrefab;

        public ArenaArtSet ActiveArtSet => activeArtSet;
        public ArenaArtSet FallbackArtSet => fallbackArtSet;
        public bool IsComplete => GetCharacterPrefab(ArenaCharacterRole.Player) != null
            && GetCharacterPrefab(ArenaCharacterRole.Swordsman) != null
            && GetCharacterPrefab(ArenaCharacterRole.Archer) != null
            && GetCharacterPrefab(ArenaCharacterRole.ShieldBearer) != null;

        public GameObject GetCharacterPrefab(ArenaCharacterRole role)
        {
            if (activeArtSet != null)
            {
                GameObject activePrefab = activeArtSet.GetCharacterPrefab(role);
                if (activePrefab != null)
                {
                    return activePrefab;
                }

                return fallbackArtSet != null
                    ? fallbackArtSet.GetCharacterPrefab(role)
                    : null;
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
            EquipmentVisualProfile activeProfile = activeArtSet != null
                ? activeArtSet.GetEquipmentProfile(role)
                : null;
            if (activeProfile != null)
            {
                return activeProfile;
            }

            return fallbackArtSet != null
                ? fallbackArtSet.GetEquipmentProfile(role)
                : null;
        }

        public void ConfigureArtSets(ArenaArtSet newActiveArtSet, ArenaArtSet newFallbackArtSet)
        {
            activeArtSet = newActiveArtSet;
            fallbackArtSet = newFallbackArtSet;
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
