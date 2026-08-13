using UnityEngine;

namespace ArenaPrototype.Art
{
    [DefaultExecutionOrder(50)]
    public sealed class CharacterLocomotionAnimator : MonoBehaviour
    {
        private static readonly int SpeedId = Animator.StringToHash("Speed");

        [SerializeField] private CharacterVisual characterVisual;
        [SerializeField] private CharacterController movementSource;
        [SerializeField, Min(0f)] private float damping = 0.10f;

        public void Configure(CharacterVisual newCharacterVisual)
        {
            characterVisual = newCharacterVisual;
        }

        private void Awake()
        {
            if (characterVisual == null)
            {
                characterVisual = GetComponent<CharacterVisual>();
            }

            if (movementSource == null)
            {
                movementSource = GetComponentInParent<CharacterController>();
            }
        }

        private void LateUpdate()
        {
            Animator animator = characterVisual != null ? characterVisual.Animator : null;
            if (animator == null || movementSource == null)
            {
                return;
            }

            Vector3 velocity = movementSource.velocity;
            velocity.y = 0f;
            animator.SetFloat(SpeedId, velocity.magnitude, damping, Time.deltaTime);
        }
    }
}
