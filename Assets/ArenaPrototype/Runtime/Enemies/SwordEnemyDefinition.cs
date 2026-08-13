using UnityEngine;

namespace ArenaPrototype.Enemies
{
    [CreateAssetMenu(fileName = "SwordEnemy", menuName = "Arena Prototype/Sword Enemy Definition")]
    public sealed class SwordEnemyDefinition : ScriptableObject
    {
        [Header("生存")]
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField, Min(1f)] private float maxPosture = 100f;
        [SerializeField, Min(0f)] private float brokenDuration = 2.5f;
        [SerializeField, Min(1f)] private float brokenDamageMultiplier = 1.5f;
        [SerializeField, Min(1f)] private float brokenKnockbackMultiplier = 1.35f;

        [Header("移动")]
        [SerializeField, Min(0f)] private float moveSpeed = 3.2f;
        [SerializeField, Min(0f)] private float rotationSpeed = 540f;
        [SerializeField, Min(0f)] private float knockbackDamping = 18f;

        [Header("攻击")]
        [SerializeField, Min(0f)] private float healthDamage = 18f;
        [SerializeField, Min(0f)] private float postureDamage;
        [SerializeField, Min(0f)] private float knockbackSpeed = 4.5f;
        [SerializeField, Min(0.1f)] private float attackRange = 1.8f;
        [SerializeField, Range(1f, 180f)] private float attackArcDegrees = 70f;
        [SerializeField, Min(0f)] private float windupDuration = 0.55f;
        [SerializeField, Min(0.01f)] private float activeDuration = 0.12f;
        [SerializeField, Min(0f)] private float recoveryDuration = 0.65f;
        [SerializeField, Min(0f)] private float respawnDelay = 1.5f;

        public float MaxHealth => maxHealth;
        public float MaxPosture => maxPosture;
        public float BrokenDuration => brokenDuration;
        public float BrokenDamageMultiplier => brokenDamageMultiplier;
        public float BrokenKnockbackMultiplier => brokenKnockbackMultiplier;
        public float MoveSpeed => moveSpeed;
        public float RotationSpeed => rotationSpeed;
        public float KnockbackDamping => knockbackDamping;
        public float HealthDamage => healthDamage;
        public float PostureDamage => postureDamage;
        public float KnockbackSpeed => knockbackSpeed;
        public float AttackRange => attackRange;
        public float AttackArcDegrees => attackArcDegrees;
        public float WindupDuration => windupDuration;
        public float ActiveDuration => activeDuration;
        public float RecoveryDuration => recoveryDuration;
        public float TotalAttackDuration => windupDuration + activeDuration + recoveryDuration;
        public float RespawnDelay => respawnDelay;
    }
}

