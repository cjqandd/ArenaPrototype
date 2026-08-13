using System.Collections;
using ArenaPrototype.Combat;
using ArenaPrototype.UI;
using ArenaPrototype.Environment;
using ArenaPrototype.Art;
using UnityEngine;

namespace ArenaPrototype.Enemies
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class SwordEnemy : MonoBehaviour, ICombatTarget, IEnvironmentImpactTarget, IWaveResettable
    {
        private enum EnemyState
        {
            Approach,
            Windup,
            Active,
            Recovery,
            Broken,
            Defeated
        }

        [SerializeField] private SwordEnemyDefinition definition;
        [SerializeField] private Transform target;
        [SerializeField] private PlayerHealth targetHealth;
        [SerializeField] private Transform weaponPivot;
        [SerializeField] private CharacterVisual characterVisual;
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private WorldSpaceDebugBars debugBars;
        [SerializeField] private EnemyWaveMember waveMember;
        [SerializeField] private ShieldDefinition shieldDefinition;
        [SerializeField] private Renderer shieldRenderer;
        [SerializeField] private EnemyAttackCoordinator attackCoordinator;

        private CharacterController characterController;
        private Material bodyMaterial;
        private Color baseColor;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private Vector3 knockbackVelocity;
        private float verticalVelocity;
        private float health;
        private float posture;
        private float difficultyMultiplier = 1f;
        private float stateElapsed;
        private bool attackConnected;
        private EnemyState state;
        private Material shieldMaterial;
        private Color shieldBaseColor;
        private float shieldPosture;
        private bool shieldBroken;
        private Renderer[] combatRenderers;

        public void Configure(
            SwordEnemyDefinition newDefinition,
            Transform newTarget,
            PlayerHealth newTargetHealth,
            Transform newWeaponPivot,
            CharacterVisual newCharacterVisual,
            WorldSpaceDebugBars newDebugBars,
            EnemyWaveMember newWaveMember)
        {
            definition = newDefinition;
            target = newTarget;
            targetHealth = newTargetHealth;
            weaponPivot = newWeaponPivot;
            characterVisual = newCharacterVisual;
            bodyRenderer = characterVisual != null ? characterVisual.PrimaryRenderer : null;
            debugBars = newDebugBars;
            waveMember = newWaveMember;
        }

        public void ConfigureShield(
            ShieldDefinition newShieldDefinition,
            Renderer newShieldRenderer)
        {
            shieldDefinition = newShieldDefinition;
            shieldRenderer = newShieldRenderer;
        }

        public void SetAttackCoordinator(EnemyAttackCoordinator newAttackCoordinator)
        {
            attackCoordinator = newAttackCoordinator;
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            combatRenderers = GetComponentsInChildren<Renderer>(true);
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
            health = ScaledMaxHealth;
            posture = ScaledMaxPosture;
            state = EnemyState.Approach;

            if (characterVisual != null)
            {
                baseColor = Color.white;
                characterVisual.SetTint(baseColor);
                characterVisual.ClearEmission();
            }
            else if (bodyRenderer != null)
            {
                bodyMaterial = bodyRenderer.material;
                baseColor = bodyMaterial.color;
            }

            if (shieldRenderer != null)
            {
                shieldMaterial = shieldRenderer.material;
                shieldBaseColor = shieldMaterial.color;
            }

            ResetShield();

            UpdateDebugBars();
        }

        private void Update()
        {
            if (Time.timeScale <= 0f || definition == null || state == EnemyState.Defeated)
            {
                return;
            }

            UpdateKnockbackAndGravity();
            stateElapsed += Time.deltaTime;

            switch (state)
            {
                case EnemyState.Approach:
                    UpdateApproach();
                    break;
                case EnemyState.Windup:
                    UpdateWindup();
                    break;
                case EnemyState.Active:
                    UpdateActive();
                    break;
                case EnemyState.Recovery:
                    UpdateRecovery();
                    break;
                case EnemyState.Broken:
                    UpdateBroken();
                    break;
            }
        }

        private void UpdateApproach()
        {
            if (!CanTargetPlayer())
            {
                return;
            }

            Vector3 direction = FlatDirectionToTarget();
            FaceDirection(direction);
            float distance = FlatDistanceToTarget();

            if (distance <= definition.AttackRange * 0.92f)
            {
                if (attackCoordinator == null || attackCoordinator.TryClaim(gameObject))
                {
                    ChangeState(EnemyState.Windup);
                }
                return;
            }

            characterController.Move(direction * ScaledMoveSpeed * Time.deltaTime);
        }

        private void UpdateWindup()
        {
            if (!CanTargetPlayer())
            {
                ChangeState(EnemyState.Approach);
                return;
            }

            FaceDirection(FlatDirectionToTarget());
            float progress = Mathf.Clamp01(stateElapsed / definition.WindupDuration);
            SetWeaponAngle(Mathf.Lerp(0f, -65f, progress));

            if (stateElapsed >= definition.WindupDuration)
            {
                attackConnected = false;
                ChangeState(EnemyState.Active);
            }
        }

        private void UpdateActive()
        {
            float progress = Mathf.Clamp01(stateElapsed / definition.ActiveDuration);
            SetWeaponAngle(Mathf.Lerp(-65f, 100f, progress));

            if (!attackConnected)
            {
                attackConnected = true;
                TryHitPlayer();
            }

            if (stateElapsed >= definition.ActiveDuration)
            {
                ChangeState(EnemyState.Recovery);
            }
        }

        private void UpdateRecovery()
        {
            float progress = definition.RecoveryDuration <= 0f
                ? 1f
                : Mathf.Clamp01(stateElapsed / definition.RecoveryDuration);
            SetWeaponAngle(Mathf.Lerp(100f, 0f, progress));

            if (stateElapsed >= definition.RecoveryDuration)
            {
                ChangeState(EnemyState.Approach);
            }
        }

        private void UpdateBroken()
        {
            if (stateElapsed >= definition.BrokenDuration)
            {
                posture = ScaledMaxPosture;
                ResetShield();
                UpdateBodyColor();
                UpdateDebugBars();
                ChangeState(EnemyState.Approach);
            }
        }

        private void TryHitPlayer()
        {
            if (!CanTargetPlayer())
            {
                return;
            }

            Vector3 direction = FlatDirectionToTarget();
            if (FlatDistanceToTarget() > definition.AttackRange
                || Vector3.Angle(transform.forward, direction) > definition.AttackArcDegrees * 0.5f)
            {
                return;
            }

            targetHealth.ReceiveHit(new CombatHit(
                gameObject,
                definition.HealthDamage * difficultyMultiplier,
                definition.PostureDamage * difficultyMultiplier,
                direction,
                definition.KnockbackSpeed * difficultyMultiplier,
                CombatAttackKind.Weapon));
        }

        public CombatHitResult ReceiveHit(CombatHit hit)
        {
            if (state == EnemyState.Defeated)
            {
                return CombatHitResult.Ignored(hit.Source, gameObject, hit.AttackKind);
            }

            if (CanBlock(hit))
            {
                return ReceiveShieldHit(hit);
            }

            bool wasBroken = state == EnemyState.Broken;
            float previousHealth = health;
            float previousPosture = posture;
            health = Mathf.Max(0f, health - hit.HealthDamage * (wasBroken ? definition.BrokenDamageMultiplier : 1f));
            bool brokePosture = false;

            if (!wasBroken)
            {
                posture = Mathf.Max(0f, posture - hit.PostureDamage);
                if (posture <= 0f)
                {
                    ChangeState(EnemyState.Broken);
                    brokePosture = true;
                }
            }

            float knockbackMultiplier = state == EnemyState.Broken ? definition.BrokenKnockbackMultiplier : 1f;
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
            if (state == EnemyState.Defeated)
            {
                return false;
            }

            float impactSpeed = Mathf.Max(0f, -Vector3.Dot(knockbackVelocity, impact.SurfaceNormal));
            if (impactSpeed < impact.MinimumImpactSpeed)
            {
                return false;
            }

            float damage = state == EnemyState.Broken
                ? impact.BrokenEnemyDamage
                : impact.EnemyDamage;
            CombatHitResult result = ReceiveHit(new CombatHit(
                impact.Source,
                damage,
                0f,
                impact.SurfaceNormal,
                impact.BounceSpeed,
                CombatAttackKind.Environment));
            return result.Applied;
        }

        private void UpdateKnockbackAndGravity()
        {
            knockbackVelocity = Vector3.MoveTowards(
                knockbackVelocity,
                Vector3.zero,
                definition.KnockbackDamping * Time.deltaTime);

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }
            else
            {
                verticalVelocity += -30f * Time.deltaTime;
            }

            characterController.Move((knockbackVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
        }

        private void FaceDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.01f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                definition.RotationSpeed * Time.deltaTime);
        }

        private Vector3 FlatDirectionToTarget()
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0f;
            return direction.sqrMagnitude < 0.01f ? transform.forward : direction.normalized;
        }

        private float FlatDistanceToTarget()
        {
            Vector3 difference = target.position - transform.position;
            difference.y = 0f;
            return difference.magnitude;
        }

        private bool CanTargetPlayer()
        {
            return target != null && targetHealth != null && !targetHealth.IsDefeated;
        }

        private void ChangeState(EnemyState newState)
        {
            bool heldAttackClaim = state == EnemyState.Windup || state == EnemyState.Active;
            state = newState;
            stateElapsed = 0f;

            if (heldAttackClaim
                && newState != EnemyState.Windup
                && newState != EnemyState.Active)
            {
                attackCoordinator?.Release(gameObject);
            }

            if (newState == EnemyState.Windup)
            {
                SetBodyColor(new Color(1f, 0.45f, 0.05f));
            }
            else
            {
                UpdateBodyColor();
            }

            if (newState == EnemyState.Broken)
            {
                SetWeaponAngle(0f);
                if (shieldDefinition != null)
                {
                    shieldBroken = true;
                    if (shieldRenderer != null)
                    {
                        shieldRenderer.enabled = false;
                    }
                }
            }
        }

        private IEnumerator FlashHit()
        {
            if (characterVisual != null)
            {
                characterVisual.SetEmission(Color.white * 1.6f);
                yield return new WaitForSeconds(0.08f);
                characterVisual.ClearEmission();
                UpdateBodyColor();
                yield break;
            }

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
            ChangeState(EnemyState.Defeated);
            if (characterController != null) characterController.enabled = false;
            SetCombatRenderers(false);
            if (weaponPivot != null) weaponPivot.gameObject.SetActive(false);
            if (debugBars != null) debugBars.gameObject.SetActive(false);

            if (waveMember != null)
            {
                waveMember.MarkDefeated();
                yield break;
            }

            yield return new WaitForSeconds(definition.RespawnDelay);

            ResetForWave(difficultyMultiplier);
        }

        public void ResetForWave(float enemyStatMultiplier)
        {
            attackCoordinator?.Release(gameObject);
            difficultyMultiplier = Mathf.Max(0.01f, enemyStatMultiplier);
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }

            characterController.enabled = false;
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            characterController.enabled = true;
            health = ScaledMaxHealth;
            posture = ScaledMaxPosture;
            knockbackVelocity = Vector3.zero;
            verticalVelocity = 0f;
            if (weaponPivot != null) weaponPivot.gameObject.SetActive(true);
            if (debugBars != null) debugBars.gameObject.SetActive(true);
            SetCombatRenderers(true);
            SetWeaponAngle(0f);
            ResetShield();
            UpdateDebugBars();
            ChangeState(EnemyState.Approach);
        }

        private void OnDisable()
        {
            attackCoordinator?.Release(gameObject);
        }

        private void UpdateBodyColor()
        {
            if (state == EnemyState.Broken)
            {
                SetBodyColor(Color.yellow);
            }
            else if (state == EnemyState.Windup)
            {
                SetBodyColor(new Color(1f, 0.45f, 0.05f));
            }
            else
            {
                SetBodyColor(baseColor);
            }
        }

        private void SetBodyColor(Color color)
        {
            if (characterVisual != null)
            {
                characterVisual.SetTint(color);
            }
            else if (bodyMaterial != null)
            {
                bodyMaterial.color = color;
            }
        }

        private void SetWeaponAngle(float angle)
        {
            if (weaponPivot != null) weaponPivot.localRotation = Quaternion.Euler(0f, angle, 0f);
        }

        private void UpdateDebugBars()
        {
            float postureNormalized = state == EnemyState.Broken
                ? 0f
                : HasIntactShield
                    ? shieldPosture / ScaledShieldPosture
                    : posture / ScaledMaxPosture;
            debugBars?.SetValues(
                health / ScaledMaxHealth,
                postureNormalized,
                state == EnemyState.Broken);
        }

        private bool CanBlock(CombatHit hit)
        {
            if (!HasIntactShield || hit.AttackKind == CombatAttackKind.Environment)
            {
                return false;
            }

            Vector3 directionToAttacker = -hit.Direction;
            directionToAttacker.y = 0f;
            return directionToAttacker.sqrMagnitude <= 0.001f
                || Vector3.Angle(transform.forward, directionToAttacker)
                    <= shieldDefinition.BlockArcDegrees * 0.5f;
        }

        private CombatHitResult ReceiveShieldHit(CombatHit hit)
        {
            float previousShieldPosture = shieldPosture;
            float postureDamage = hit.PostureDamage
                * shieldDefinition.GetPostureMultiplier(hit.AttackKind);
            shieldPosture = Mathf.Max(0f, shieldPosture - postureDamage);
            bool brokeShield = shieldPosture <= 0f;

            knockbackVelocity = hit.Direction
                * hit.KnockbackSpeed
                * shieldDefinition.BlockedKnockbackMultiplier;
            StopCoroutine(nameof(FlashShield));
            StartCoroutine(nameof(FlashShield));

            if (brokeShield)
            {
                shieldBroken = true;
                if (shieldRenderer != null)
                {
                    shieldRenderer.enabled = false;
                }
                ChangeState(EnemyState.Broken);
            }

            UpdateDebugBars();
            return CombatResultStream.Publish(new CombatHitResult(
                hit.Source,
                gameObject,
                CombatContact.Shield,
                hit.AttackKind,
                0f,
                previousShieldPosture - shieldPosture,
                brokeShield,
                false));
        }

        private IEnumerator FlashShield()
        {
            if (shieldMaterial == null || shieldBroken)
            {
                yield break;
            }

            shieldMaterial.color = Color.white;
            yield return new WaitForSeconds(0.08f);
            shieldMaterial.color = shieldBaseColor;
        }

        private void ResetShield()
        {
            shieldBroken = false;
            shieldPosture = ScaledShieldPosture;
            if (shieldRenderer != null)
            {
                shieldRenderer.enabled = shieldDefinition != null;
            }
            if (shieldMaterial != null)
            {
                shieldMaterial.color = shieldBaseColor;
            }
        }

        private void SetCombatRenderers(bool visible)
        {
            if (combatRenderers == null || combatRenderers.Length == 0)
            {
                combatRenderers = GetComponentsInChildren<Renderer>(true);
            }

            foreach (Renderer combatRenderer in combatRenderers)
            {
                if (combatRenderer != null)
                {
                    combatRenderer.enabled = visible;
                }
            }
        }

        private bool HasIntactShield => shieldDefinition != null && !shieldBroken;
        private float ScaledShieldPosture => shieldDefinition == null
            ? 1f
            : shieldDefinition.MaxPosture * difficultyMultiplier;

        private float ScaledMaxHealth => definition.MaxHealth * difficultyMultiplier;
        private float ScaledMaxPosture => definition.MaxPosture * difficultyMultiplier;
        private float ScaledMoveSpeed => definition.MoveSpeed * difficultyMultiplier;
    }
}
