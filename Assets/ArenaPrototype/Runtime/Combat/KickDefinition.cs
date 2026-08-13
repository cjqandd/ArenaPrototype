using UnityEngine;

namespace ArenaPrototype.Combat
{
    [CreateAssetMenu(fileName = "Kick", menuName = "Arena Prototype/Kick Definition")]
    public sealed class KickDefinition : ScriptableObject
    {
        [Header("伤害与控制")]
        [SerializeField, Min(0f)] private float healthDamage = 5f;
        [SerializeField, Min(0f)] private float postureDamage = 45f;
        [SerializeField, Min(0f)] private float knockbackSpeed = 5f;

        [Header("踢击范围")]
        [SerializeField, Min(0.1f)] private float range = 1.6f;
        [SerializeField, Range(1f, 180f)] private float arcDegrees = 55f;
        [SerializeField, Min(0.1f)] private float height = 1.4f;

        [Header("踢击时间")]
        [SerializeField, Min(0f)] private float windupDuration = 0.10f;
        [SerializeField, Min(0.01f)] private float activeDuration = 0.08f;
        [SerializeField, Min(0f)] private float recoveryDuration = 0.32f;

        public float HealthDamage => healthDamage;
        public float PostureDamage => postureDamage;
        public float KnockbackSpeed => knockbackSpeed;
        public float Range => range;
        public float ArcDegrees => arcDegrees;
        public float Height => height;
        public float WindupDuration => windupDuration;
        public float ActiveDuration => activeDuration;
        public float RecoveryDuration => recoveryDuration;
        public float TotalDuration => windupDuration + activeDuration + recoveryDuration;
    }
}

