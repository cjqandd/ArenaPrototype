using UnityEngine;

namespace ArenaPrototype.Combat
{
    public enum CombatActionPhase
    {
        Ready,
        Windup,
        Active,
        Recovery
    }

    public readonly struct CombatActionFrame
    {
        public CombatActionFrame(
            CombatActionPhase phase,
            float phaseProgress,
            bool enteredActive,
            bool completed)
        {
            Phase = phase;
            PhaseProgress = Mathf.Clamp01(phaseProgress);
            EnteredActive = enteredActive;
            Completed = completed;
        }

        public CombatActionPhase Phase { get; }
        public float PhaseProgress { get; }
        public bool EnteredActive { get; }
        public bool Completed { get; }
    }

    public sealed class TimedCombatAction
    {
        private float windupDuration;
        private float activeDuration;
        private float recoveryDuration;
        private float elapsed;
        private bool activeTriggered;

        public CombatActionPhase Phase { get; private set; } = CombatActionPhase.Ready;
        public bool IsRunning => Phase != CombatActionPhase.Ready;
        public bool IsInRecovery => Phase == CombatActionPhase.Recovery;
        public bool IsHitboxActive => Phase == CombatActionPhase.Active;

        public void Begin(float windup, float active, float recovery)
        {
            windupDuration = Mathf.Max(0f, windup);
            activeDuration = Mathf.Max(0.0001f, active);
            recoveryDuration = Mathf.Max(0f, recovery);
            elapsed = 0f;
            activeTriggered = false;
            Phase = windupDuration > 0f
                ? CombatActionPhase.Windup
                : CombatActionPhase.Active;
        }

        public CombatActionFrame Advance(float deltaTime)
        {
            if (!IsRunning)
            {
                return new CombatActionFrame(CombatActionPhase.Ready, 0f, false, false);
            }

            float previousElapsed = elapsed;
            float activeEnd = windupDuration + activeDuration;
            float totalDuration = activeEnd + recoveryDuration;
            elapsed = Mathf.Min(totalDuration, elapsed + Mathf.Max(0f, deltaTime));

            bool enteredActive = !activeTriggered
                && elapsed >= windupDuration
                && previousElapsed < activeEnd;
            if (enteredActive)
            {
                activeTriggered = true;
            }

            bool completed = elapsed >= totalDuration;
            if (completed)
            {
                Phase = CombatActionPhase.Ready;
                return new CombatActionFrame(Phase, 1f, enteredActive, true);
            }

            float phaseProgress;
            if (elapsed < windupDuration)
            {
                Phase = CombatActionPhase.Windup;
                phaseProgress = SafeProgress(elapsed, windupDuration);
            }
            else if (elapsed < activeEnd)
            {
                Phase = CombatActionPhase.Active;
                phaseProgress = SafeProgress(elapsed - windupDuration, activeDuration);
            }
            else
            {
                Phase = CombatActionPhase.Recovery;
                phaseProgress = SafeProgress(elapsed - activeEnd, recoveryDuration);
            }

            return new CombatActionFrame(Phase, phaseProgress, enteredActive, false);
        }

        public void Reset()
        {
            elapsed = 0f;
            activeTriggered = false;
            Phase = CombatActionPhase.Ready;
        }

        private static float SafeProgress(float value, float duration)
        {
            return duration <= 0f ? 1f : value / duration;
        }
    }
}
