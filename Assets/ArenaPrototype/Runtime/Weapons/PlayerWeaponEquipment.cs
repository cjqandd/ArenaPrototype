using ArenaPrototype.Combat;
using ArenaPrototype.Movement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArenaPrototype.Weapons
{
    public sealed class PlayerWeaponEquipment : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private Transform weaponSocket;
        [SerializeField] private PlayerMeleeCombat meleeCombat;
        [SerializeField] private PlayerKickCombat kickCombat;
        [SerializeField] private PlayerDodge dodge;
        [SerializeField] private WeaponInstance equippedWeapon;

        [Header("拾取选择")]
        [SerializeField, Min(0.1f)] private float pickupRadius = 1.7f;
        [SerializeField, Range(1f, 180f)] private float maximumPickupAngle = 85f;
        [SerializeField, Min(0f)] private float distanceWeight = 0.65f;
        [SerializeField, Min(0f)] private float facingWeight = 0.35f;

        [Header("交换")]
        [SerializeField, Min(0f)] private float droppedPickupLockDuration = 0.55f;

        private readonly Collider[] nearbyColliders = new Collider[32];
        private WeaponInstance pickupCandidate;

        public WeaponInstance EquippedWeapon => equippedWeapon;
        public WeaponInstance PickupCandidate => pickupCandidate;

        public void Configure(
            Transform newWeaponSocket,
            PlayerMeleeCombat newMeleeCombat,
            PlayerKickCombat newKickCombat,
            PlayerDodge newDodge)
        {
            weaponSocket = newWeaponSocket;
            meleeCombat = newMeleeCombat;
            kickCombat = newKickCombat;
            dodge = newDodge;
        }

        public void EquipInitial(WeaponInstance weapon)
        {
            if (weapon == null)
            {
                return;
            }

            equippedWeapon = weapon;
            weapon.Equip(weaponSocket);
            meleeCombat?.SetWeapon(weapon.Definition, weaponSocket);
        }

        private void Update()
        {
            UpdatePickupCandidate();

            if (pickupCandidate != null
                && Keyboard.current?.eKey.wasPressedThisFrame == true
                && CanSwapWeapon())
            {
                SwapWithCandidate();
            }
        }

        private void OnDisable()
        {
            SetPickupCandidate(null);
        }

        private bool CanSwapWeapon()
        {
            return (meleeCombat == null || !meleeCombat.IsAttacking)
                && (kickCombat == null || !kickCombat.IsKicking)
                && (dodge == null || !dodge.IsDodging);
        }

        private void UpdatePickupCandidate()
        {
            WeaponInstance bestCandidate = null;
            float bestScore = float.PositiveInfinity;

            int colliderCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                pickupRadius,
                nearbyColliders,
                ~0,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < colliderCount; i++)
            {
                WeaponInstance weapon = nearbyColliders[i].GetComponentInParent<WeaponInstance>();
                if (weapon == null || !weapon.CanBePickedUp)
                {
                    continue;
                }

                Vector3 toWeapon = weapon.transform.position - transform.position;
                toWeapon.y = 0f;
                float distance = toWeapon.magnitude;
                if (distance > pickupRadius)
                {
                    continue;
                }

                float angle = distance <= 0.01f ? 0f : Vector3.Angle(transform.forward, toWeapon);
                if (angle > maximumPickupAngle)
                {
                    continue;
                }

                float normalizedDistance = distance / pickupRadius;
                float normalizedAngle = angle / maximumPickupAngle;
                float score = normalizedDistance * distanceWeight + normalizedAngle * facingWeight;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestCandidate = weapon;
                }
            }

            SetPickupCandidate(bestCandidate);
        }

        private void SetPickupCandidate(WeaponInstance newCandidate)
        {
            if (pickupCandidate == newCandidate)
            {
                return;
            }

            pickupCandidate?.SetHighlighted(false);
            pickupCandidate = newCandidate;
            pickupCandidate?.SetHighlighted(true);
        }

        private void SwapWithCandidate()
        {
            WeaponInstance pickedWeapon = pickupCandidate;
            WeaponInstance previousWeapon = equippedWeapon;
            Vector3 replacementPosition = pickedWeapon.transform.position;
            Quaternion replacementRotation = pickedWeapon.transform.rotation;
            SetPickupCandidate(null);

            equippedWeapon = pickedWeapon;
            pickedWeapon.Equip(weaponSocket);
            meleeCombat?.SetWeapon(pickedWeapon.Definition, weaponSocket);

            if (previousWeapon != null)
            {
                previousWeapon.Drop(
                    replacementPosition,
                    replacementRotation,
                    droppedPickupLockDuration);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        }
    }
}
