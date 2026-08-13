using UnityEngine;

namespace ArenaPrototype.Movement
{
    [CreateAssetMenu(fileName = "Dodge", menuName = "Arena Prototype/Dodge Definition")]
    public sealed class DodgeDefinition : ScriptableObject
    {
        [Header("位移")]
        [SerializeField, Min(0.1f)] private float distance = 3.2f;
        [SerializeField, Min(0.01f)] private float duration = 0.24f;
        [SerializeField] private AnimationCurve movementCurve = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 2.5f),
            new Keyframe(1f, 1f, 0.2f, 0f));

        [Header("无敌与冷却")]
        [SerializeField, Min(0f)] private float invulnerabilityStart = 0.04f;
        [SerializeField, Min(0f)] private float invulnerabilityEnd = 0.18f;
        [SerializeField, Min(0f)] private float cooldown = 0.65f;

        [Header("操作规则")]
        [SerializeField] private bool allowTurningDuringDodge;
        [SerializeField] private bool allowRecoveryCancel = true;

        public float Distance => distance;
        public float Duration => duration;
        public AnimationCurve MovementCurve => movementCurve;
        public float InvulnerabilityStart => Mathf.Min(invulnerabilityStart, duration);
        public float InvulnerabilityEnd => Mathf.Clamp(invulnerabilityEnd, InvulnerabilityStart, duration);
        public float Cooldown => cooldown;
        public bool AllowTurningDuringDodge => allowTurningDuringDodge;
        public bool AllowRecoveryCancel => allowRecoveryCancel;
    }
}
