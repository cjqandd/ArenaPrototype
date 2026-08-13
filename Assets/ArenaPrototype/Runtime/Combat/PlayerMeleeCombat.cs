using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ArenaPrototype.Movement;
using System;

namespace ArenaPrototype.Combat
{
    public sealed class PlayerMeleeCombat : MonoBehaviour
    {
        [Header("配置")]
        [SerializeField] private WeaponDefinition weapon;
        [SerializeField] private Transform weaponPivot;
        [SerializeField] private PlayerKickCombat kickCombat;
        [SerializeField] private PlayerDodge dodge;
        [SerializeField] private PlayerCombatGrowth combatGrowth;
        [SerializeField] private LayerMask targetLayers = ~0;

        private readonly HashSet<ICombatTarget> hitTargets = new HashSet<ICombatTarget>();
        private readonly Collider[] overlapResults = new Collider[16];

        private readonly TimedCombatAction action = new TimedCombatAction();

        public bool IsAttacking => action.IsRunning;
        public bool IsInRecovery => action.IsInRecovery;
        public WeaponDefinition CurrentWeapon => weapon;
        public event Action<WeaponDefinition> WeaponChanged;
        public bool IsHitboxActive => weapon != null && action.IsHitboxActive;

        public void Configure(WeaponDefinition newWeapon, Transform newWeaponPivot)
        {
            SetWeapon(newWeapon, newWeaponPivot);
        }

        public void SetWeapon(WeaponDefinition newWeapon, Transform newWeaponPivot)
        {
            ResetAction();
            weapon = newWeapon;
            weaponPivot = newWeaponPivot;
            ResetWeaponVisual();
            WeaponChanged?.Invoke(weapon);
        }

        private void Update()
        {
            if (!IsAttacking
                && (kickCombat == null || !kickCombat.IsKicking)
                && (dodge == null || !dodge.IsDodging)
                && Mouse.current?.leftButton.wasPressedThisFrame == true)
            {
                BeginAttack();
            }

            if (IsAttacking)
            {
                UpdateAttack();
            }
        }

        public void SetKickCombat(PlayerKickCombat newKickCombat)
        {
            kickCombat = newKickCombat;
        }

        public void SetDodge(PlayerDodge newDodge)
        {
            dodge = newDodge;
        }

        public void SetCombatGrowth(PlayerCombatGrowth newCombatGrowth)
        {
            combatGrowth = newCombatGrowth;
        }

        public void ResetAction()
        {
            action.Reset();
            hitTargets.Clear();
            ResetWeaponVisual();
        }

        private void BeginAttack()
        {
            if (weapon == null)
            {
                return;
            }

            action.Begin(
                weapon.WindupDuration,
                weapon.ActiveDuration,
                weapon.RecoveryDuration);
            hitTargets.Clear();
        }

        private void UpdateAttack()
        {
            CombatActionFrame frame = action.Advance(Time.deltaTime);
            if (frame.Phase == CombatActionPhase.Windup)
            {
                SetWeaponAngle(Mathf.Lerp(0f, weapon.WindupAngle, frame.PhaseProgress));
            }
            else if (frame.Phase == CombatActionPhase.Active)
            {
                SetWeaponAngle(Mathf.Lerp(
                    weapon.WindupAngle,
                    weapon.SwingAngle,
                    frame.PhaseProgress));
            }
            else if (frame.Phase == CombatActionPhase.Recovery)
            {
                SetWeaponAngle(Mathf.Lerp(weapon.SwingAngle, 0f, frame.PhaseProgress));
            }
            else if (frame.Completed)
            {
                ResetWeaponVisual();
            }

            if (frame.EnteredActive)
            {
                DetectHits();
            }
        }

        public bool TryCancelRecoveryForDodge()
        {
            if (!action.IsInRecovery)
            {
                return false;
            }

            ResetAction();
            return true;
        }

        private void DetectHits()
        {
            Vector3 center = transform.position + Vector3.up * (weapon.AttackHeight * 0.5f);
            float healthMultiplier = combatGrowth == null
                ? 1f
                : combatGrowth.HealthDamageMultiplier;
            float postureMultiplier = combatGrowth == null
                ? 1f
                : combatGrowth.PostureDamageMultiplier;

            int hitCount = Physics.OverlapSphereNonAlloc(
                center,
                weapon.AttackRange,
                overlapResults,
                targetLayers,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                ICombatTarget target = overlapResults[i].GetComponentInParent<ICombatTarget>();
                if (target == null || hitTargets.Contains(target))
                {
                    continue;
                }

                if (target is Component targetComponent && targetComponent.transform.root == transform.root)
                {
                    continue;
                }

                Vector3 closestPoint = overlapResults[i].ClosestPoint(center);
                Vector3 direction = closestPoint - transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude < 0.01f)
                {
                    direction = transform.forward;
                }

                if (Vector3.Angle(transform.forward, direction) > weapon.AttackArcDegrees * 0.5f)
                {
                    continue;
                }

                hitTargets.Add(target);

                target.ReceiveHit(new CombatHit(
                    gameObject,
                    weapon.HealthDamage * healthMultiplier,
                    weapon.PostureDamage * postureMultiplier,
                    direction.normalized,
                    weapon.KnockbackSpeed,
                    CombatAttackKind.Weapon));
            }
        }

        private void SetWeaponAngle(float angle)
        {
            if (weaponPivot != null)
            {
                weaponPivot.localRotation = Quaternion.Euler(0f, angle, 0f);
            }
        }

        private void ResetWeaponVisual()
        {
            SetWeaponAngle(0f);
        }

        private void OnDrawGizmosSelected()
        {
            if (weapon == null)
            {
                return;
            }

            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.color = new Color(1f, 0.25f, 0.1f, 0.35f);
            const int segments = 20;
            Vector3 previous = Vector3.zero;
            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.Lerp(
                    -weapon.AttackArcDegrees * 0.5f,
                    weapon.AttackArcDegrees * 0.5f,
                    i / (float)segments);
                Vector3 current = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * weapon.AttackRange;
                Gizmos.DrawLine(Vector3.zero, current);
                if (i > 0)
                {
                    Gizmos.DrawLine(previous, current);
                }

                previous = current;
            }
            Gizmos.matrix = previousMatrix;
        }
    }
}
