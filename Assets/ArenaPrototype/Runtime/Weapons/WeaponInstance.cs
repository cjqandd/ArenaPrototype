using UnityEngine;

namespace ArenaPrototype.Weapons
{
    public enum WeaponState
    {
        Grounded,
        Equipped,
        Thrown
    }

    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public sealed class WeaponInstance : MonoBehaviour
    {
        [SerializeField] private Combat.WeaponDefinition definition;
        [SerializeField] private Transform modelRoot;
        [SerializeField] private GameObject pickupIndicator;
        [SerializeField] private Collider interactionCollider;
        [SerializeField] private Rigidbody weaponRigidbody;
        [SerializeField] private WeaponState state = WeaponState.Grounded;

        [Header("装备姿势")]
        [SerializeField] private Vector3 equippedLocalPosition = new Vector3(0.65f, 0f, 0.55f);
        [SerializeField] private Vector3 equippedLocalEuler = new Vector3(0f, 25f, 0f);

        [Header("投掷落地")]
        [SerializeField, Min(0f)] private float settleSpeed = 0.35f;
        [SerializeField, Min(0f)] private float settleDuration = 0.30f;

        private float pickupLockedUntil;
        private float timeBelowSettleSpeed;
        [SerializeField, HideInInspector] private Vector3 groundedModelLocalPosition;
        [SerializeField, HideInInspector] private Quaternion groundedModelLocalRotation = Quaternion.identity;
        [SerializeField, HideInInspector] private Vector3 groundedModelLocalScale = Vector3.one;
        [SerializeField, HideInInspector] private bool hasGroundedModelPose;

        public Combat.WeaponDefinition Definition => definition;
        public WeaponState State => state;
        public bool CanBePickedUp => State == WeaponState.Grounded && Time.time >= pickupLockedUntil;

        public void Configure(
            Combat.WeaponDefinition newDefinition,
            Transform newModelRoot,
            GameObject newPickupIndicator,
            Collider newInteractionCollider,
            Rigidbody newWeaponRigidbody)
        {
            definition = newDefinition;
            modelRoot = newModelRoot;
            pickupIndicator = newPickupIndicator;
            interactionCollider = newInteractionCollider;
            weaponRigidbody = newWeaponRigidbody;
            CaptureGroundedModelPose();
            SetGroundPhysics();
            SetHighlighted(false);
        }

        private void Awake()
        {
            if (interactionCollider == null)
            {
                interactionCollider = GetComponent<Collider>();
            }

            if (weaponRigidbody == null)
            {
                weaponRigidbody = GetComponent<Rigidbody>();
            }

            CaptureGroundedModelPose();
            MigrateLegacyEquippedHierarchy();
            ApplyStatePhysics();
            SetHighlighted(false);
        }

        private void Update()
        {
            if (State != WeaponState.Thrown || weaponRigidbody == null)
            {
                return;
            }

            if (weaponRigidbody.linearVelocity.sqrMagnitude <= settleSpeed * settleSpeed)
            {
                timeBelowSettleSpeed += Time.deltaTime;
                if (timeBelowSettleSpeed >= settleDuration)
                {
                    SetGrounded(0.15f);
                }
            }
            else
            {
                timeBelowSettleSpeed = 0f;
            }
        }

        public void Equip(Transform socket)
        {
            if (socket == null)
            {
                return;
            }

            state = WeaponState.Equipped;
            ApplyStatePhysics();

            // Keep the Rigidbody root in world space. Parenting an interpolated Rigidbody
            // beneath the moving player lets the physics pose overwrite the visual pose.
            transform.SetParent(null, true);
            if (modelRoot != null)
            {
                modelRoot.SetParent(socket, false);
                modelRoot.localPosition = equippedLocalPosition;
                modelRoot.localRotation = Quaternion.Euler(equippedLocalEuler);
                modelRoot.localScale = groundedModelLocalScale;
            }

            SetHighlighted(false);
            Physics.SyncTransforms();
        }

        public void Drop(Vector3 worldPosition, Quaternion worldRotation, float pickupLockDuration)
        {
            transform.SetParent(null, true);
            transform.SetPositionAndRotation(worldPosition, worldRotation);
            AttachModelToPhysicsRoot();
            pickupLockedUntil = Time.time + Mathf.Max(0f, pickupLockDuration);
            SetGrounded(worldPosition.y);
            Physics.SyncTransforms();
        }

        public void Throw(Vector3 velocity, float pickupLockDuration)
        {
            MovePhysicsRootToEquippedModel();
            transform.SetParent(null, true);
            state = WeaponState.Thrown;
            pickupLockedUntil = Time.time + Mathf.Max(0f, pickupLockDuration);
            timeBelowSettleSpeed = 0f;
            SetHighlighted(false);
            ApplyStatePhysics();
            weaponRigidbody.linearVelocity = velocity;
            Physics.SyncTransforms();
        }

        public void SetHighlighted(bool highlighted)
        {
            if (pickupIndicator != null)
            {
                pickupIndicator.SetActive(highlighted && State == WeaponState.Grounded);
            }
        }

        private void SetGrounded(float groundHeight)
        {
            state = WeaponState.Grounded;
            Vector3 position = transform.position;
            position.y = groundHeight;
            transform.position = position;
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            timeBelowSettleSpeed = 0f;
            ApplyStatePhysics();
        }

        private void ApplyStatePhysics()
        {
            if (interactionCollider == null || weaponRigidbody == null)
            {
                return;
            }

            switch (State)
            {
                case WeaponState.Equipped:
                    interactionCollider.enabled = false;
                    weaponRigidbody.interpolation = RigidbodyInterpolation.None;
                    weaponRigidbody.useGravity = false;
                    weaponRigidbody.isKinematic = true;
                    weaponRigidbody.detectCollisions = false;
                    weaponRigidbody.linearVelocity = Vector3.zero;
                    weaponRigidbody.angularVelocity = Vector3.zero;
                    break;
                case WeaponState.Grounded:
                    SetGroundPhysics();
                    break;
                case WeaponState.Thrown:
                    interactionCollider.enabled = true;
                    interactionCollider.isTrigger = false;
                    weaponRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                    weaponRigidbody.useGravity = true;
                    weaponRigidbody.isKinematic = false;
                    weaponRigidbody.detectCollisions = true;
                    break;
            }
        }

        private void SetGroundPhysics()
        {
            if (interactionCollider == null || weaponRigidbody == null)
            {
                return;
            }

            interactionCollider.enabled = true;
            interactionCollider.isTrigger = true;
            weaponRigidbody.interpolation = RigidbodyInterpolation.None;
            weaponRigidbody.useGravity = false;
            weaponRigidbody.isKinematic = true;
            weaponRigidbody.detectCollisions = true;
            weaponRigidbody.linearVelocity = Vector3.zero;
            weaponRigidbody.angularVelocity = Vector3.zero;
        }

        private void CaptureGroundedModelPose()
        {
            if (hasGroundedModelPose || modelRoot == null)
            {
                return;
            }

            bool modelIsOnPhysicsRoot = modelRoot.parent == transform;
            groundedModelLocalPosition = modelIsOnPhysicsRoot
                ? modelRoot.localPosition
                : Vector3.zero;
            groundedModelLocalRotation = modelIsOnPhysicsRoot
                ? modelRoot.localRotation
                : Quaternion.identity;
            groundedModelLocalScale = modelRoot.localScale;
            hasGroundedModelPose = true;
        }

        private void MigrateLegacyEquippedHierarchy()
        {
            if (state != WeaponState.Equipped || transform.parent == null || modelRoot == null)
            {
                return;
            }

            Transform socket = transform.parent;
            transform.SetParent(null, true);
            modelRoot.SetParent(socket, false);
            modelRoot.localPosition = equippedLocalPosition;
            modelRoot.localRotation = Quaternion.Euler(equippedLocalEuler);
            modelRoot.localScale = groundedModelLocalScale;
            Physics.SyncTransforms();
        }

        private void AttachModelToPhysicsRoot()
        {
            if (modelRoot == null)
            {
                return;
            }

            CaptureGroundedModelPose();
            modelRoot.SetParent(transform, false);
            modelRoot.localPosition = groundedModelLocalPosition;
            modelRoot.localRotation = groundedModelLocalRotation;
            modelRoot.localScale = groundedModelLocalScale;
        }

        private void MovePhysicsRootToEquippedModel()
        {
            if (modelRoot == null || modelRoot.parent == transform)
            {
                return;
            }

            Vector3 modelWorldPosition = modelRoot.position;
            Quaternion modelWorldRotation = modelRoot.rotation;
            transform.SetPositionAndRotation(modelWorldPosition, modelWorldRotation);
            AttachModelToPhysicsRoot();
        }
    }
}
