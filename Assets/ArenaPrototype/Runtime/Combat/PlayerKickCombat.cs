using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ArenaPrototype.Movement;

namespace ArenaPrototype.Combat
{
    public sealed class PlayerKickCombat : MonoBehaviour
    {
        [SerializeField] private KickDefinition kick;
        [SerializeField] private PlayerMeleeCombat meleeCombat;
        [SerializeField] private Transform kickVisual;
        [SerializeField] private PlayerDodge dodge;
        [SerializeField] private PlayerCombatGrowth combatGrowth;
        [SerializeField] private LayerMask targetLayers = ~0;

        private readonly HashSet<ICombatTarget> hitTargets = new HashSet<ICombatTarget>();
        private readonly Collider[] overlapResults = new Collider[16];
        private readonly TimedCombatAction action = new TimedCombatAction();

        public bool IsKicking => action.IsRunning;
        public bool IsInRecovery => action.IsInRecovery;
        public bool IsHitboxActive => kick != null && action.IsHitboxActive;

        public void Configure(KickDefinition newKick, PlayerMeleeCombat newMeleeCombat, Transform newKickVisual)
        {
            kick = newKick;
            meleeCombat = newMeleeCombat;
            kickVisual = newKickVisual;
            ResetKickVisual();
        }

        private void Update()
        {
            if (!IsKicking
                && meleeCombat != null
                && !meleeCombat.IsAttacking
                && (dodge == null || !dodge.IsDodging)
                && Mouse.current?.rightButton.wasPressedThisFrame == true)
            {
                BeginKick();
            }

            if (IsKicking)
            {
                UpdateKick();
            }
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
            ResetKickVisual();
        }

        private void BeginKick()
        {
            if (kick == null)
            {
                return;
            }

            action.Begin(kick.WindupDuration, kick.ActiveDuration, kick.RecoveryDuration);
            hitTargets.Clear();
        }

        private void UpdateKick()
        {
            CombatActionFrame frame = action.Advance(Time.deltaTime);
            if (frame.Phase == CombatActionPhase.Windup)
            {
                SetKickVisual(Mathf.Lerp(0f, -0.2f, frame.PhaseProgress));
            }
            else if (frame.Phase == CombatActionPhase.Active)
            {
                SetKickVisual(Mathf.Lerp(-0.2f, 0.9f, frame.PhaseProgress));
            }
            else if (frame.Phase == CombatActionPhase.Recovery)
            {
                SetKickVisual(Mathf.Lerp(0.9f, 0f, frame.PhaseProgress));
            }
            else if (frame.Completed)
            {
                ResetKickVisual();
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
            Vector3 center = transform.position + Vector3.up * (kick.Height * 0.5f);
            float healthMultiplier = combatGrowth == null
                ? 1f
                : combatGrowth.HealthDamageMultiplier;
            float postureMultiplier = combatGrowth == null
                ? 1f
                : combatGrowth.PostureDamageMultiplier;
            int hitCount = Physics.OverlapSphereNonAlloc(
                center,
                kick.Range,
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

                if (Vector3.Angle(transform.forward, direction) > kick.ArcDegrees * 0.5f)
                {
                    continue;
                }

                hitTargets.Add(target);
                target.ReceiveHit(new CombatHit(
                    gameObject,
                    kick.HealthDamage * healthMultiplier,
                    kick.PostureDamage * postureMultiplier,
                    direction.normalized,
                    kick.KnockbackSpeed,
                    CombatAttackKind.Kick));
            }
        }

        private void SetKickVisual(float forwardOffset)
        {
            if (kickVisual != null)
            {
                Vector3 position = kickVisual.localPosition;
                position.z = forwardOffset;
                kickVisual.localPosition = position;
            }
        }

        private void ResetKickVisual()
        {
            SetKickVisual(0f);
        }

    }
}
