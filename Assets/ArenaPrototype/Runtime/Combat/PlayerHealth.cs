using System.Collections;
using System;
using ArenaPrototype.Movement;
using ArenaPrototype.Player;
using ArenaPrototype.UI;
using ArenaPrototype.Environment;
using ArenaPrototype.Weapons;
using ArenaPrototype.Art;
using UnityEngine;

namespace ArenaPrototype.Combat
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerHealth : MonoBehaviour, ICombatTarget, IEnvironmentImpactTarget
    {
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField, Min(0f)] private float respawnDelay = 1.2f;
        [SerializeField] private CharacterVisual characterVisual;
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerMeleeCombat meleeCombat;
        [SerializeField] private PlayerKickCombat kickCombat;
        [SerializeField] private PlayerDodge dodge;
        [SerializeField] private PlayerHealthDebugBar debugBar;
        [SerializeField] private PlayerWeaponEquipment weaponEquipment;

        private CharacterController characterController;
        private Material bodyMaterial;
        private Color baseColor;
        private Vector3 spawnPosition;
        private float health;
        private Renderer[] renderersToHide;
        private bool damageEnabled = true;

        public bool IsDefeated { get; private set; }
        public float HealthNormalized => Mathf.Clamp01(health / maxHealth);
        public float CurrentHealth => health;
        public float MaximumHealth => maxHealth;
        public event Action Defeated;
        public event Action Respawned;

        public void Configure(
            CharacterVisual newCharacterVisual,
            PlayerController newPlayerController,
            PlayerMeleeCombat newMeleeCombat,
            PlayerKickCombat newKickCombat,
            PlayerDodge newDodge,
            PlayerHealthDebugBar newDebugBar)
        {
            characterVisual = newCharacterVisual;
            bodyRenderer = characterVisual != null ? characterVisual.PrimaryRenderer : null;
            playerController = newPlayerController;
            meleeCombat = newMeleeCombat;
            kickCombat = newKickCombat;
            dodge = newDodge;
            debugBar = newDebugBar;
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            renderersToHide = GetComponentsInChildren<Renderer>(true);
            if (weaponEquipment == null)
            {
                weaponEquipment = GetComponent<PlayerWeaponEquipment>();
            }
            spawnPosition = transform.position;
            health = maxHealth;

            if (characterVisual != null)
            {
                characterVisual.SetTint(Color.white);
                characterVisual.ClearEmission();
            }
            else if (bodyRenderer != null)
            {
                bodyMaterial = bodyRenderer.material;
                baseColor = bodyMaterial.color;
            }

            UpdateDebugBar();
        }

        public CombatHitResult ReceiveHit(CombatHit hit)
        {
            if (!damageEnabled || IsDefeated || (dodge != null && dodge.IsInvulnerable))
            {
                return CombatHitResult.Ignored(hit.Source, gameObject, hit.AttackKind);
            }

            float previousHealth = health;
            health = Mathf.Max(0f, health - hit.HealthDamage);
            playerController?.AddExternalImpulse(hit.Direction, hit.KnockbackSpeed);
            StopCoroutine(nameof(FlashHit));
            StartCoroutine(nameof(FlashHit));
            UpdateDebugBar();

            bool defeated = health <= 0f;
            var result = new CombatHitResult(
                hit.Source,
                gameObject,
                CombatContact.Body,
                hit.AttackKind,
                previousHealth - health,
                0f,
                false,
                defeated);

            if (defeated)
            {
                StartCoroutine(Respawn());
            }

            return CombatResultStream.Publish(result);
        }

        public bool TryReceiveEnvironmentImpact(EnvironmentImpact impact)
        {
            if (IsDefeated || (dodge != null && dodge.IsInvulnerable) || playerController == null)
            {
                return false;
            }

            float impactSpeed = Mathf.Max(
                0f,
                -Vector3.Dot(playerController.ExternalVelocity, impact.SurfaceNormal));
            if (impactSpeed < impact.MinimumImpactSpeed)
            {
                return false;
            }

            CombatHitResult result = ReceiveHit(new CombatHit(
                impact.Source,
                impact.PlayerDamage,
                0f,
                impact.SurfaceNormal,
                impact.BounceSpeed,
                CombatAttackKind.Environment));
            return result.Applied;
        }

        private IEnumerator FlashHit()
        {
            if (characterVisual != null)
            {
                characterVisual.SetEmission(Color.white * 1.6f);
                yield return new WaitForSeconds(0.10f);
                characterVisual.ClearEmission();
                yield break;
            }

            if (bodyMaterial == null)
            {
                yield break;
            }

            bodyMaterial.color = Color.white;
            yield return new WaitForSeconds(0.10f);
            bodyMaterial.color = baseColor;
        }

        private IEnumerator Respawn()
        {
            IsDefeated = true;
            Defeated?.Invoke();
            meleeCombat?.ResetAction();
            kickCombat?.ResetAction();
            dodge?.ResetAction(true);
            SetActionsEnabled(false);
            renderersToHide = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer targetRenderer in renderersToHide)
            {
                targetRenderer.enabled = false;
            }

            yield return new WaitForSeconds(respawnDelay);

            characterController.enabled = false;
            transform.position = spawnPosition;
            characterController.enabled = true;
            playerController?.ResetMotion();

            health = maxHealth;
            IsDefeated = false;
            foreach (Renderer targetRenderer in renderersToHide)
            {
                targetRenderer.enabled = true;
            }
            if (characterVisual != null)
            {
                characterVisual.SetTint(Color.white);
                characterVisual.ClearEmission();
            }
            else if (bodyMaterial != null)
            {
                bodyMaterial.color = baseColor;
            }

            SetActionsEnabled(true);
            UpdateDebugBar();
            Respawned?.Invoke();
        }

        public void SetActionsEnabled(bool enabled)
        {
            if (weaponEquipment == null)
            {
                weaponEquipment = GetComponent<PlayerWeaponEquipment>();
            }

            if (playerController != null) playerController.enabled = enabled;
            if (meleeCombat != null) meleeCombat.enabled = enabled;
            if (kickCombat != null) kickCombat.enabled = enabled;
            if (dodge != null) dodge.enabled = enabled;
            if (weaponEquipment != null) weaponEquipment.enabled = enabled;
        }

        public void SetDamageEnabled(bool enabled)
        {
            damageEnabled = enabled;
        }

        public void CancelCurrentActions(bool clearDodgeCooldown)
        {
            meleeCombat?.ResetAction();
            kickCombat?.ResetAction();
            dodge?.ResetAction(clearDodgeCooldown);
            playerController?.ResetMotion();
        }

        private void UpdateDebugBar()
        {
            debugBar?.SetValue(HealthNormalized);
        }
    }
}
