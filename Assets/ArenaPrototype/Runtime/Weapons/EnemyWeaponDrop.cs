using ArenaPrototype.Combat;
using ArenaPrototype.Enemies;
using ArenaPrototype.Art;
using UnityEngine;

namespace ArenaPrototype.Weapons
{
    public sealed class EnemyWeaponDrop : MonoBehaviour
    {
        [SerializeField] private EnemyWaveMember waveMember;
        [SerializeField] private WeaponDefinition weaponDefinition;
        [SerializeField] private Material weaponMaterial;
        [SerializeField] private Material pickupIndicatorMaterial;
        [SerializeField] private WeaponDropRegistry registry;
        [SerializeField] private ArenaArtCatalog artCatalog;
        [SerializeField, Min(0f)] private float pickupLockDuration = 0.35f;

        public void Configure(
            EnemyWaveMember newWaveMember,
            WeaponDefinition newWeaponDefinition,
            Material newWeaponMaterial,
            Material newPickupIndicatorMaterial,
            WeaponDropRegistry newRegistry,
            ArenaArtCatalog newArtCatalog)
        {
            waveMember = newWaveMember;
            weaponDefinition = newWeaponDefinition;
            weaponMaterial = newWeaponMaterial;
            pickupIndicatorMaterial = newPickupIndicatorMaterial;
            registry = newRegistry;
            artCatalog = newArtCatalog;
        }

        private void OnEnable()
        {
            if (waveMember != null)
            {
                waveMember.Defeated += HandleDefeated;
            }
        }

        private void OnDisable()
        {
            if (waveMember != null)
            {
                waveMember.Defeated -= HandleDefeated;
            }
        }

        private void HandleDefeated(EnemyWaveMember defeatedMember)
        {
            if (weaponDefinition == null || registry == null)
            {
                return;
            }

            Vector3 dropPosition = transform.position;
            dropPosition.y = 0.15f;
            WeaponInstance drop = GrayboxWeaponFactory.Create(
                $"{weaponDefinition.DisplayName}_EnemyDrop",
                dropPosition,
                Quaternion.Euler(0f, transform.eulerAngles.y + 90f, 0f),
                weaponDefinition,
                weaponMaterial,
                pickupIndicatorMaterial,
                artCatalog);
            drop.Drop(dropPosition, drop.transform.rotation, pickupLockDuration);
            registry.Register(drop);
        }
    }
}
