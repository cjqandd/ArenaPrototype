using UnityEngine;
using UnityEngine.InputSystem;
using ArenaPrototype.Movement;

namespace ArenaPrototype.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("移动")]
        [SerializeField, Min(0f)] private float moveSpeed = 6f;
        [SerializeField, Min(0f)] private float acceleration = 30f;
        [SerializeField] private float gravity = -30f;

        [Header("朝向")]
        [SerializeField, Min(0f)] private float rotationSpeed = 1080f;
        [SerializeField] private PlayerDodge dodge;
        [SerializeField, Min(0f)] private float externalImpulseDamping = 18f;

        private CharacterController characterController;
        private Camera gameplayCamera;
        private Vector3 planarVelocity;
        private float verticalVelocity;
        private Vector3 externalVelocity;

        public Vector3 ExternalVelocity => externalVelocity;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            gameplayCamera = Camera.main;
        }

        private void Update()
        {
            if (gameplayCamera == null)
            {
                gameplayCamera = Camera.main;
                if (gameplayCamera == null)
                {
                    return;
                }
            }

            UpdateMovement();
            UpdateFacing();
        }

        private void UpdateMovement()
        {
            if (dodge != null && dodge.IsDodging)
            {
                planarVelocity = Vector3.zero;
                return;
            }

            Vector2 input = ReadMovementInput();

            Vector3 cameraForward = Vector3.ProjectOnPlane(gameplayCamera.transform.forward, Vector3.up).normalized;
            Vector3 cameraRight = Vector3.ProjectOnPlane(gameplayCamera.transform.right, Vector3.up).normalized;
            Vector3 desiredDirection = cameraForward * input.y + cameraRight * input.x;
            desiredDirection = Vector3.ClampMagnitude(desiredDirection, 1f);

            Vector3 desiredVelocity = desiredDirection * moveSpeed;
            planarVelocity = Vector3.MoveTowards(planarVelocity, desiredVelocity, acceleration * Time.deltaTime);

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            Vector3 motion = planarVelocity + Vector3.up * verticalVelocity;
            externalVelocity = Vector3.MoveTowards(
                externalVelocity,
                Vector3.zero,
                externalImpulseDamping * Time.deltaTime);
            motion += externalVelocity;
            characterController.Move(motion * Time.deltaTime);
        }

        private void UpdateFacing()
        {
            if (dodge != null && dodge.IsDodging && !dodge.AllowTurningDuringDodge)
            {
                return;
            }

            if (Mouse.current == null)
            {
                return;
            }

            Ray mouseRay = gameplayCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane movementPlane = new Plane(Vector3.up, transform.position);

            if (!movementPlane.Raycast(mouseRay, out float distance))
            {
                return;
            }

            Vector3 lookDirection = mouseRay.GetPoint(distance) - transform.position;
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude < 0.01f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);
        }

        public void SetDodge(PlayerDodge newDodge)
        {
            dodge = newDodge;
        }

        public Vector3 GetCameraRelativeMoveDirection()
        {
            if (gameplayCamera == null)
            {
                gameplayCamera = Camera.main;
            }

            if (gameplayCamera == null)
            {
                return Vector3.zero;
            }

            Vector2 input = ReadMovementInput();
            Vector3 cameraForward = Vector3.ProjectOnPlane(gameplayCamera.transform.forward, Vector3.up).normalized;
            Vector3 cameraRight = Vector3.ProjectOnPlane(gameplayCamera.transform.right, Vector3.up).normalized;
            return Vector3.ClampMagnitude(cameraForward * input.y + cameraRight * input.x, 1f);
        }

        public void AddExternalImpulse(Vector3 direction, float speed)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.01f)
            {
                externalVelocity = direction.normalized * Mathf.Max(0f, speed);
            }
        }

        public void ResetMotion()
        {
            planarVelocity = Vector3.zero;
            externalVelocity = Vector3.zero;
            verticalVelocity = 0f;
        }

        private static Vector2 ReadMovementInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            float horizontal = 0f;
            float vertical = 0f;

            if (keyboard.aKey.isPressed) horizontal -= 1f;
            if (keyboard.dKey.isPressed) horizontal += 1f;
            if (keyboard.sKey.isPressed) vertical -= 1f;
            if (keyboard.wKey.isPressed) vertical += 1f;

            return Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
        }
    }
}
