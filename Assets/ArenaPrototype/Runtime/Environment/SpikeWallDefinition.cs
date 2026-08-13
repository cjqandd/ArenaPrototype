using UnityEngine;

namespace ArenaPrototype.Environment
{
    [CreateAssetMenu(fileName = "SpikeWall", menuName = "Arena Prototype/Spike Wall Definition")]
    public sealed class SpikeWallDefinition : ScriptableObject
    {
        [Header("伤害")]
        [SerializeField, Min(0f)] private float enemyDamage = 45f;
        [SerializeField, Min(0f)] private float brokenEnemyDamage = 1000f;
        [SerializeField, Min(0f)] private float playerDamage = 25f;

        [Header("触发")]
        [SerializeField, Min(0f)] private float minimumImpactSpeed = 2.5f;
        [SerializeField, Min(0f)] private float bounceSpeed = 3f;
        [SerializeField, Min(0f)] private float repeatCooldown = 0.75f;
        [SerializeField, Min(0f)] private float flashDuration = 0.10f;

        public float EnemyDamage => enemyDamage;
        public float BrokenEnemyDamage => brokenEnemyDamage;
        public float PlayerDamage => playerDamage;
        public float MinimumImpactSpeed => minimumImpactSpeed;
        public float BounceSpeed => bounceSpeed;
        public float RepeatCooldown => repeatCooldown;
        public float FlashDuration => flashDuration;
    }
}

