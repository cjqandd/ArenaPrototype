using ArenaPrototype.Combat;
using UnityEngine;

namespace ArenaPrototype.Enemies
{
    [CreateAssetMenu(fileName = "Shield", menuName = "Arena Prototype/Shield Definition")]
    public sealed class ShieldDefinition : ScriptableObject
    {
        [Header("防御")]
        [SerializeField, Min(1f)] private float maxPosture = 85f;
        [SerializeField, Range(1f, 180f)] private float blockArcDegrees = 130f;
        [SerializeField, Min(0f)] private float blockedKnockbackMultiplier = 0.25f;

        [Header("不同攻击的破盾效率")]
        [SerializeField, Min(0f)] private float weaponPostureMultiplier = 0.45f;
        [SerializeField, Min(0f)] private float kickPostureMultiplier = 1.40f;
        [SerializeField, Min(0f)] private float projectilePostureMultiplier = 0.25f;

        public float MaxPosture => maxPosture;
        public float BlockArcDegrees => blockArcDegrees;
        public float BlockedKnockbackMultiplier => blockedKnockbackMultiplier;

        public float GetPostureMultiplier(CombatAttackKind attackKind)
        {
            return attackKind switch
            {
                CombatAttackKind.Kick => kickPostureMultiplier,
                CombatAttackKind.Projectile => projectilePostureMultiplier,
                CombatAttackKind.Weapon => weaponPostureMultiplier,
                _ => 0f
            };
        }
    }
}
