using System.Collections;
using ArenaPrototype.UI;
using UnityEngine;
using ArenaPrototype.Environment;

namespace ArenaPrototype.Combat
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class TrainingDummy : MonoBehaviour, ICombatTarget, IEnvironmentImpactTarget
    {
        [Header("训练假人")]
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField, Min(1f)] private float maxPosture = 100f;
        [SerializeField, Min(0f)] private float knockbackDamping = 18f;
        [SerializeField, Min(0f)] private float respawnDelay = 1f;
        [SerializeField] private float gravity = -30f;
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private WorldSpaceDebugBars debugBars;
        [SerializeField, Min(0f)] private float brokenDuration = 2.5f;
        [SerializeField, Min(1f)] private float brokenHealthDamageMultiplier = 1.5f;
        [SerializeField, Min(1f)] private float brokenKnockbackMultiplier = 1.75f;

        private CharacterController characterController;
        private Material bodyMaterial;
        private Color baseColor;
        private Vector3 spawnPosition;
        private Vector3 knockbackVelocity;
        private float verticalVelocity;
        private float health;
        private float posture;
        private bool isDefeated;
        private bool postureBroken;
        private float brokenTimeRemaining;

        public float HealthNormalized => Mathf.Clamp01(health / maxHealth);
        public float PostureNormalized => Mathf.Clamp01(posture / maxPosture);
        public bool IsPostureBroken => postureBroken;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            spawnPosition = transform.position;
            health = maxHealth;
            posture = maxPosture;

            if (bodyRenderer != null)
            {
                bodyMaterial = bodyRenderer.material;
                baseColor = bodyMaterial.color;
            }

            UpdateDebugBars();
        }

        private void Update()
        {
            if (isDefeated)
            {
                return;
            }

            if (postureBroken)
            {
                brokenTimeRemaining -= Time.deltaTime;
                if (brokenTimeRemaining <= 0f)
                {
                    posture = maxPosture;
                    postureBroken = false;
                    UpdateBodyColor();
                    UpdateDebugBars();
                }
            }

            knockbackVelocity = Vector3.MoveTowards(
                knockbackVelocity,
                Vector3.zero,
                knockbackDamping * Time.deltaTime);

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            characterController.Move((knockbackVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
        }

        public CombatHitResult ReceiveHit(CombatHit hit)
        {
            if (isDefeated)
            {
                return CombatHitResult.Ignored(hit.Source, gameObject, hit.AttackKind);
            }

            bool wasAlreadyBroken = postureBroken;
            float healthDamageMultiplier = wasAlreadyBroken ? brokenHealthDamageMultiplier : 1f;
            float previousHealth = health;
            float previousPosture = posture;
            health = Mathf.Max(0f, health - hit.HealthDamage * healthDamageMultiplier);
            bool brokePosture = false;
            if (!postureBroken)
            {
                posture = Mathf.Max(0f, posture - hit.PostureDamage);
                if (posture <= 0f)
                {
                    postureBroken = true;
                    brokenTimeRemaining = brokenDuration;
                    brokePosture = true;
                }
            }

            float knockbackMultiplier = postureBroken ? brokenKnockbackMultiplier : 1f;
            knockbackVelocity = hit.Direction * hit.KnockbackSpeed * knockbackMultiplier;

            StopCoroutine(nameof(FlashHit));
            StartCoroutine(nameof(FlashHit));
            UpdateDebugBars();

            bool defeated = health <= 0f;
            if (defeated)
            {
                StartCoroutine(Respawn());
            }

            return CombatResultStream.Publish(new CombatHitResult(
                hit.Source,
                gameObject,
                CombatContact.Body,
                hit.AttackKind,
                previousHealth - health,
                previousPosture - posture,
                brokePosture,
                defeated));
        }

        public bool TryReceiveEnvironmentImpact(EnvironmentImpact impact)
        {
            if (isDefeated)
            {
                return false;
            }

            float impactSpeed = Mathf.Max(0f, -Vector3.Dot(knockbackVelocity, impact.SurfaceNormal));
            if (impactSpeed < impact.MinimumImpactSpeed)
            {
                return false;
            }

            float damage = postureBroken ? impact.BrokenEnemyDamage : impact.EnemyDamage;
            CombatHitResult result = ReceiveHit(new CombatHit(
                impact.Source,
                damage,
                0f,
                impact.SurfaceNormal,
                impact.BounceSpeed,
                CombatAttackKind.Environment));
            return result.Applied;
        }

        public void Configure(Renderer newBodyRenderer, WorldSpaceDebugBars newDebugBars)
        {
            bodyRenderer = newBodyRenderer;
            debugBars = newDebugBars;
        }

        private IEnumerator FlashHit()
        {
            if (bodyMaterial == null)
            {
                yield break;
            }

            bodyMaterial.color = Color.white;
            yield return new WaitForSeconds(0.08f);
            UpdateBodyColor();
        }

        private IEnumerator Respawn()
        {
            isDefeated = true;
            if (bodyRenderer != null)
            {
                bodyRenderer.enabled = false;
            }

            yield return new WaitForSeconds(respawnDelay);

            characterController.enabled = false;
            transform.position = spawnPosition;
            characterController.enabled = true;

            health = maxHealth;
            posture = maxPosture;
            postureBroken = false;
            brokenTimeRemaining = 0f;
            knockbackVelocity = Vector3.zero;
            verticalVelocity = 0f;
            isDefeated = false;

            if (bodyRenderer != null)
            {
                bodyRenderer.enabled = true;
                UpdateBodyColor();
            }
            UpdateDebugBars();
        }

        private void UpdateBodyColor()
        {
            if (bodyMaterial != null)
            {
                bodyMaterial.color = postureBroken ? Color.yellow : baseColor;
            }
        }

        private void UpdateDebugBars()
        {
            if (debugBars != null)
            {
                debugBars.SetValues(HealthNormalized, PostureNormalized, postureBroken);
            }
        }
    }
}
