using UnityEngine;

namespace ArenaPrototype.CameraSystem
{
    public sealed class ArenaCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 14f, -12f);
        [SerializeField, Min(0f)] private float lookAtHeight = 0.8f;

        private Quaternion fixedRotation;

        public void Configure(Transform newTarget, Vector3 newOffset)
        {
            target = newTarget;
            offset = newOffset;
            RecalculateFixedRotation();
        }

        private void Awake()
        {
            RecalculateFixedRotation();
        }

        private void OnValidate()
        {
            RecalculateFixedRotation();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            transform.SetPositionAndRotation(target.position + offset, fixedRotation);
        }

        private void RecalculateFixedRotation()
        {
            Vector3 lookDirection = Vector3.up * lookAtHeight - offset;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                fixedRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            }
        }
    }
}
