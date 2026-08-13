using UnityEngine;

namespace ArenaPrototype.Enemies
{
    [CreateAssetMenu(fileName = "ArcherEnemy", menuName = "Arena Prototype/Archer Enemy Definition")]
    public sealed class ArcherEnemyDefinition : ScriptableObject
    {
        [Header("生存")]
        [SerializeField, Min(1f)] private float maxHealth = 70f;
        [SerializeField, Min(1f)] private float maxPosture = 70f;
        [SerializeField, Min(0f)] private float brokenDuration = 2.5f;
        [SerializeField, Min(1f)] private float brokenDamageMultiplier = 1.5f;
        [SerializeField, Min(1f)] private float brokenKnockbackMultiplier = 1.35f;

        [Header("移动与站位")]
        [SerializeField, Min(0f)] private float moveSpeed = 2.8f;
        [SerializeField, Min(0f)] private float rotationSpeed = 540f;
        [SerializeField, Min(0f)] private float knockbackDamping = 18f;
        [SerializeField, Min(0.1f)] private float retreatRange = 3.2f;
        [SerializeField, Min(0.1f)] private float preferredRange = 5.8f;
        [SerializeField, Min(0.1f)] private float maximumAttackRange = 8f;

        [Header("射击")]
        [SerializeField, Min(0f)] private float healthDamage = 14f;
        [SerializeField, Min(0f)] private float knockbackSpeed = 3.2f;
        [SerializeField, Min(0f)] private float windupDuration = 0.8f;
        [SerializeField, Min(0f)] private float recoveryDuration = 1.15f;
        [SerializeField, Min(0.1f)] private float projectileSpeed = 13f;
        [SerializeField, Min(0.1f)] private float projectileLifetime = 2.5f;
        [SerializeField, Min(0.01f)] private float projectileHitRadius = 0.10f;
        [SerializeField, Min(0f)] private float respawnDelay = 1.5f;

        public float MaxHealth => maxHealth;
        public float MaxPosture => maxPosture;
        public float BrokenDuration => brokenDuration;
        public float BrokenDamageMultiplier => brokenDamageMultiplier;
        public float BrokenKnockbackMultiplier => brokenKnockbackMultiplier;
        public float MoveSpeed => moveSpeed;
        public float RotationSpeed => rotationSpeed;
        public float KnockbackDamping => knockbackDamping;
        public float RetreatRange => retreatRange;
        public float PreferredRange => preferredRange;
        public float MaximumAttackRange => maximumAttackRange;
        public float HealthDamage => healthDamage;
        public float KnockbackSpeed => knockbackSpeed;
        public float WindupDuration => windupDuration;
        public float RecoveryDuration => recoveryDuration;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetime => projectileLifetime;
        public float ProjectileHitRadius => projectileHitRadius;
        public float RespawnDelay => respawnDelay;
    }
}
