using UnityEngine;

namespace ArenaPrototype.Environment
{
    public readonly struct EnvironmentImpact
    {
        public EnvironmentImpact(
            GameObject source,
            Vector3 surfaceNormal,
            float minimumImpactSpeed,
            float enemyDamage,
            float brokenEnemyDamage,
            float playerDamage,
            float bounceSpeed)
        {
            Source = source;
            SurfaceNormal = surfaceNormal.normalized;
            MinimumImpactSpeed = minimumImpactSpeed;
            EnemyDamage = enemyDamage;
            BrokenEnemyDamage = brokenEnemyDamage;
            PlayerDamage = playerDamage;
            BounceSpeed = bounceSpeed;
        }

        public GameObject Source { get; }
        public Vector3 SurfaceNormal { get; }
        public float MinimumImpactSpeed { get; }
        public float EnemyDamage { get; }
        public float BrokenEnemyDamage { get; }
        public float PlayerDamage { get; }
        public float BounceSpeed { get; }
    }

    public interface IEnvironmentImpactTarget
    {
        bool TryReceiveEnvironmentImpact(EnvironmentImpact impact);
    }
}

