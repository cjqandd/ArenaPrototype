using ArenaPrototype.Combat;
using ArenaPrototype.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArenaPrototype.Movement
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerDodge : MonoBehaviour
    {
        [SerializeField] private DodgeDefinition dodge;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerMeleeCombat meleeCombat;
        [SerializeField] private PlayerKickCombat kickCombat;
        [SerializeField] private PlayerCombatGrowth combatGrowth;

        private CharacterController characterController;
        private Vector3 dodgeDirection;
        private float dodgeElapsed;
        private float cooldownRemaining;
        private float cooldownDuration;
        private float previousCurvePosition;

        public bool IsDodging { get; private set; }
        public bool IsInvulnerable { get; private set; }
        public float CooldownNormalized
        {
            get
            {
                if (cooldownDuration <= 0f)
                {
                    return 0f;
                }

                return Mathf.Clamp01(cooldownRemaining / cooldownDuration);
            }
        }

        public bool IsReady => !IsDodging && cooldownRemaining <= 0f;
        public bool AllowTurningDuringDodge => dodge != null && dodge.AllowTurningDuringDodge;
        public bool AllowsRecoveryCancel => dodge != null && dodge.AllowRecoveryCancel;

        public void ResetAction(bool clearCooldown)
        {
            IsDodging = false;
            IsInvulnerable = false;
            dodgeElapsed = 0f;
            previousCurvePosition = 0f;
            if (clearCooldown)
            {
                cooldownRemaining = 0f;
                cooldownDuration = 0f;
            }
        }

        public void SetCombatGrowth(PlayerCombatGrowth newCombatGrowth)
        {
            combatGrowth = newCombatGrowth;
        }

        public void Configure(
            DodgeDefinition newDodge,
            PlayerController newPlayerController,
            PlayerMeleeCombat newMeleeCombat,
            PlayerKickCombat newKickCombat)
        {
            dodge = newDodge;
            playerController = newPlayerController;
            meleeCombat = newMeleeCombat;
            kickCombat = newKickCombat;
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (!IsDodging && cooldownRemaining > 0f)
            {
                cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);
            }

            if (!IsDodging
                && Keyboard.current?.spaceKey.wasPressedThisFrame == true
                && CanStartDodge())
            {
                BeginDodge();
            }

            if (IsDodging)
            {
                UpdateDodge();
            }
        }

        private bool CanStartDodge()
        {
            return dodge != null
                && cooldownRemaining <= 0f
                && !ActionBlocksDodge(meleeCombat?.IsAttacking == true, meleeCombat?.IsInRecovery == true)
                && !ActionBlocksDodge(kickCombat?.IsKicking == true, kickCombat?.IsInRecovery == true);
        }

        private void BeginDodge()
        {
            if (dodge.AllowRecoveryCancel)
            {
                meleeCombat?.TryCancelRecoveryForDodge();
                kickCombat?.TryCancelRecoveryForDodge();
            }

            dodgeDirection = playerController != null
                ? playerController.GetCameraRelativeMoveDirection()
                : Vector3.zero;

            if (dodgeDirection.sqrMagnitude < 0.01f)
            {
                dodgeDirection = transform.forward;
            }

            dodgeDirection.y = 0f;
            dodgeDirection.Normalize();
            dodgeElapsed = 0f;
            previousCurvePosition = EvaluateMovementCurve(0f);
            IsDodging = true;
            IsInvulnerable = false;
        }

        private void UpdateDodge()
        {
            dodgeElapsed = Mathf.Min(dodgeElapsed + Time.deltaTime, dodge.Duration);
            float normalizedTime = Mathf.Clamp01(dodgeElapsed / dodge.Duration);
            float curvePosition = EvaluateMovementCurve(normalizedTime);
            float deltaDistance = Mathf.Max(0f, curvePosition - previousCurvePosition) * dodge.Distance;
            previousCurvePosition = curvePosition;

            characterController.Move(dodgeDirection * deltaDistance);
            IsInvulnerable = dodgeElapsed >= dodge.InvulnerabilityStart
                && dodgeElapsed <= dodge.InvulnerabilityEnd;

            if (dodgeElapsed >= dodge.Duration)
            {
                IsDodging = false;
                IsInvulnerable = false;
                cooldownDuration = dodge.Cooldown * (combatGrowth == null
                    ? 1f
                    : combatGrowth.DodgeCooldownMultiplier);
                cooldownRemaining = cooldownDuration;
            }
        }

        private bool ActionBlocksDodge(bool isRunning, bool isInRecovery)
        {
            return isRunning && (!dodge.AllowRecoveryCancel || !isInRecovery);
        }

        private float EvaluateMovementCurve(float normalizedTime)
        {
            return dodge.MovementCurve == null
                ? normalizedTime
                : dodge.MovementCurve.Evaluate(normalizedTime);
        }
    }
}
