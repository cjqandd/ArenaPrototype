using UnityEngine;

namespace ArenaPrototype.Combat
{
    public enum WeaponKind
    {
        Sword,
        ChainBlade
    }

    [CreateAssetMenu(fileName = "Weapon", menuName = "Arena Prototype/Weapon Definition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [Header("武器身份")]
        [SerializeField] private string displayName = "Sword";
        [SerializeField] private WeaponKind kind;

        [Header("基础伤害")]
        [SerializeField, Min(0f)] private float healthDamage = 20f;
        [SerializeField, Min(0f)] private float postureDamage = 30f;
        [SerializeField, Min(0f)] private float knockbackSpeed = 7f;

        [Header("攻击范围")]
        [SerializeField, Min(0.1f)] private float attackRange = 2.2f;
        [SerializeField, Range(1f, 360f)] private float attackArcDegrees = 100f;
        [SerializeField, Min(0.1f)] private float attackHeight = 1.8f;

        [Header("攻击时间")]
        [SerializeField, Min(0f)] private float windupDuration = 0.12f;
        [SerializeField, Min(0.01f)] private float activeDuration = 0.10f;
        [SerializeField, Min(0f)] private float recoveryDuration = 0.28f;

        [Header("灰盒挥砍表现")]
        [SerializeField] private float windupAngle = -55f;
        [SerializeField] private float swingAngle = 105f;

        public float HealthDamage => healthDamage;
        public float PostureDamage => postureDamage;
        public float KnockbackSpeed => knockbackSpeed;
        public float AttackRange => attackRange;
        public float AttackArcDegrees => attackArcDegrees;
        public float AttackHeight => attackHeight;
        public float WindupDuration => windupDuration;
        public float ActiveDuration => activeDuration;
        public float RecoveryDuration => recoveryDuration;
        public float WindupAngle => windupAngle;
        public float SwingAngle => swingAngle;
        public float TotalDuration => windupDuration + activeDuration + recoveryDuration;
        public string DisplayName => displayName;
        public WeaponKind Kind => kind;
    }
}
