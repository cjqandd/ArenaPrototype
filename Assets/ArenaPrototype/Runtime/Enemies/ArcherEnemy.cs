using System.Collections;
using ArenaPrototype.Combat;
using ArenaPrototype.Environment;
using ArenaPrototype.UI;
using ArenaPrototype.Art;
using UnityEngine;

namespace ArenaPrototype.Enemies
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class ArcherEnemy : MonoBehaviour, ICombatTarget, IEnvironmentImpactTarget, IWaveResettable
    {
        private enum EnemyState
        {
            Positioning,
            Windup,
            Recovery,
            Broken,
            Defeated
        }

        [SerializeField] private ArcherEnemyDefinition definition;
        [SerializeField] private Transform target;
        [SerializeField] private PlayerHealth targetHealth;
        [SerializeField] private Transform muzzle;
        [SerializeField] private CharacterVisual characterVisual;
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private WorldSpaceDebugBars debugBars;
        [SerializeField] private LineRenderer aimTelegraph;
        [SerializeField] private Material projectileMaterial;
        [SerializeField] private EquipmentVisualProfile projectileVisualProfile;
        [SerializeField] private EnemyWaveMember waveMember;
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
        private EnemyState state;
        private Renderer[] combatRenderers;

        public void Configure(
            ArcherEnemyDefinition newDefinition,
            Transform newTarget,
            PlayerHealth newTargetHealth,
            Transform newMuzzle,
            CharacterVisual newCharacterVisual,
            WorldSpaceDebugBars newDebugBars,
            LineRenderer newAimTelegraph,
            Material newProjectileMaterial,
            EquipmentVisualProfile newProjectileVisualProfile,
            EnemyWaveMember newWaveMember)
        {
            definition = newDefinition;
            target = newTarget;
            targetHealth = newTargetHealth;
            muzzle = newMuzzle;
            characterVisual = newCharacterVisual;
            bodyRenderer = characterVisual != null ? characterVisual.PrimaryRenderer : null;
            debugBars = newDebugBars;
            aimTelegraph = newAimTelegraph;
            projectileMaterial = newProjectileMaterial;
            projectileVisualProfile = newProjectileVisualProfile;
            waveMember = newWaveMember;
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
            state = EnemyState.Positioning;

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

            SetAimTelegraph(false);
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
                case EnemyState.Positioning:
                    UpdatePositioning();
                    break;
                case EnemyState.Windup:
                    UpdateWindup();
                    break;
                case EnemyState.Recovery:
                    UpdateRecovery();
                    break;
                case EnemyState.Broken:
                    UpdateBroken();
                    break;
            }
        }

        private void UpdatePositioning()
        {
            if (!CanTargetPlayer())
            {
                return;
            }

            Vector3 direction = FlatDirectionToTarget();
            float distance = FlatDistanceToTarget();
            FaceDirection(direction);

            if (distance < definition.RetreatRange)
            {
                characterController.Move(-direction * ScaledMoveSpeed * Time.deltaTime);
                return;
            }

            if (distance > definition.MaximumAttackRange)
            {
                characterController.Move(direction * ScaledMoveSpeed * Time.deltaTime);
                return;
            }

            if (distance > definition.PreferredRange + 0.6f)
            {
                characterController.Move(direction * ScaledMoveSpeed * Time.deltaTime);
            }

            if (attackCoordinator == null || attackCoordinator.TryClaim(gameObject))
            {
                ChangeState(EnemyState.Windup);
            }
        }

        private void UpdateWindup()
        {
            if (!CanTargetPlayer())
            {
                ChangeState(EnemyState.Positioning);
                return;
            }

            FaceDirection(FlatDirectionToTarget());
            UpdateAimTelegraph();

            if (stateElapsed >= definition.WindupDuration)
            {
                FireProjectile();
                ChangeState(EnemyState.Recovery);
            }
        }

        private void UpdateRecovery()
        {
            if (stateElapsed >= definition.RecoveryDuration)
            {
                ChangeState(EnemyState.Positioning);
            }
        }

        private void UpdateBroken()
        {
            if (stateElapsed >= definition.BrokenDuration)
            {
                posture = ScaledMaxPosture;
                UpdateBodyColor();
                UpdateDebugBars();
                ChangeState(EnemyState.Positioning);
            }
        }

        private void FireProjectile()
        {
            if (!CanTargetPlayer() || muzzle == null)
            {
                return;
            }

            Vector3 targetPoint = target.position + Vector3.up;
            Vector3 shotDirection = (targetPoint - muzzle.position).normalized;
            ArcherProjectile.Spawn(
                muzzle.position,
                shotDirection,
                projectileMaterial,
                projectileVisualProfile,
                gameObject,
                definition.ProjectileSpeed * difficultyMultiplier,
                definition.ProjectileLifetime,
                definition.ProjectileHitRadius,
                definition.HealthDamage * difficultyMultiplier,
                definition.KnockbackSpeed * difficultyMultiplier);
        }

        public CombatHitResult ReceiveHit(CombatHit hit)
        {
            if (state == EnemyState.Defeated)
            {
                return CombatHitResult.Ignored(hit.Source, gameObject, hit.AttackKind);
            }

            bool wasBroken = state == EnemyState.Broken;
            float previousHealth = health;
            float previousPosture = posture;
            health = Mathf.Max(
                0f,
                health - hit.HealthDamage * (wasBroken ? definition.BrokenDamageMultiplier : 1f));
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

            float knockbackMultiplier = state == EnemyState.Broken
                ? definition.BrokenKnockbackMultiplier
                : 1f;
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

            float impactSpeed = Mathf.Max(
                0f,
                -Vector3.Dot(knockbackVelocity, impact.SurfaceNormal));
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

            characterController.Move(
                (knockbackVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
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
            bool heldAttackClaim = state == EnemyState.Windup;
            state = newState;
            stateElapsed = 0f;
            if (heldAttackClaim && newState != EnemyState.Windup)
            {
                attackCoordinator?.Release(gameObject);
            }
            SetAimTelegraph(newState == EnemyState.Windup);
            UpdateBodyColor();
        }

        private void UpdateAimTelegraph()
        {
            if (aimTelegraph == null || muzzle == null || target == null)
            {
                return;
            }

            aimTelegraph.SetPosition(0, muzzle.position);
            aimTelegraph.SetPosition(1, target.position + Vector3.up);
        }

        private void SetAimTelegraph(bool visible)
        {
            if (aimTelegraph != null)
            {
                aimTelegraph.enabled = visible;
                if (visible)
                {
                    UpdateAimTelegraph();
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
            SetAimTelegraph(false);
            SetCombatRenderers(false);
            if (muzzle != null && muzzle.parent != null) muzzle.parent.gameObject.SetActive(false);
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
            if (muzzle != null && muzzle.parent != null) muzzle.parent.gameObject.SetActive(true);
            if (debugBars != null) debugBars.gameObject.SetActive(true);
            SetCombatRenderers(true);
            SetAimTelegraph(false);
            UpdateDebugBars();
            ChangeState(EnemyState.Positioning);
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

        private void UpdateDebugBars()
        {
            debugBars?.SetValues(
                health / ScaledMaxHealth,
                posture / ScaledMaxPosture,
                state == EnemyState.Broken);
        }

        private void SetCombatRenderers(bool visible)
        {
            if (combatRenderers == null || combatRenderers.Length == 0)
            {
                combatRenderers = GetComponentsInChildren<Renderer>(true);
            }

            foreach (Renderer combatRenderer in combatRenderers)
            {
                if (combatRenderer != null && combatRenderer != aimTelegraph)
                {
                    combatRenderer.enabled = visible;
                }
            }
        }

        private float ScaledMaxHealth => definition.MaxHealth * difficultyMultiplier;
        private float ScaledMaxPosture => definition.MaxPosture * difficultyMultiplier;
        private float ScaledMoveSpeed => definition.MoveSpeed * difficultyMultiplier;
    }
}
