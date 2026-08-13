using System;
using UnityEngine;

namespace ArenaPrototype.Combat
{
    public enum CombatAttackKind
    {
        Weapon,
        Kick,
        Projectile,
        Environment
    }

    public enum CombatContact
    {
        Ignored,
        Body,
        Shield
    }

    public readonly struct CombatHit
    {
        public CombatHit(
            GameObject source,
            float healthDamage,
            float postureDamage,
            Vector3 direction,
            float knockbackSpeed,
            CombatAttackKind attackKind = CombatAttackKind.Weapon)
        {
            Source = source;
            HealthDamage = healthDamage;
            PostureDamage = postureDamage;
            Direction = direction;
            KnockbackSpeed = knockbackSpeed;
            AttackKind = attackKind;
        }

        public GameObject Source { get; }
        public float HealthDamage { get; }
        public float PostureDamage { get; }
        public Vector3 Direction { get; }
        public float KnockbackSpeed { get; }
        public CombatAttackKind AttackKind { get; }
    }

    public readonly struct CombatHitResult
    {
        public CombatHitResult(
            GameObject source,
            GameObject target,
            CombatContact contact,
            CombatAttackKind attackKind,
            float healthDamageApplied,
            float postureDamageApplied,
            bool brokePosture,
            bool defeated)
        {
            Source = source;
            Target = target;
            Contact = contact;
            AttackKind = attackKind;
            HealthDamageApplied = Mathf.Max(0f, healthDamageApplied);
            PostureDamageApplied = Mathf.Max(0f, postureDamageApplied);
            BrokePosture = brokePosture;
            Defeated = defeated;
        }

        public static CombatHitResult Ignored(
            GameObject source,
            GameObject target,
            CombatAttackKind attackKind)
        {
            return new CombatHitResult(
                source,
                target,
                CombatContact.Ignored,
                attackKind,
                0f,
                0f,
                false,
                false);
        }

        public GameObject Source { get; }
        public GameObject Target { get; }
        public CombatContact Contact { get; }
        public CombatAttackKind AttackKind { get; }
        public float HealthDamageApplied { get; }
        public float PostureDamageApplied { get; }
        public bool BrokePosture { get; }
        public bool Defeated { get; }
        public bool Applied => Contact != CombatContact.Ignored;
    }

    public static class CombatResultStream
    {
        public static event Action<CombatHitResult> Resolved;

        public static CombatHitResult Publish(CombatHitResult result)
        {
            if (result.Applied)
            {
                Resolved?.Invoke(result);
            }

            return result;
        }
    }

    public interface ICombatTarget
    {
        CombatHitResult ReceiveHit(CombatHit hit);
    }
}
