using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArenaPrototype.Environment
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class ArenaSpikeWall : MonoBehaviour
    {
        [SerializeField] private SpikeWallDefinition definition;
        [SerializeField] private Vector3 surfaceNormal = Vector3.back;
        [SerializeField] private Renderer[] spikeRenderers;
        [SerializeField] private Color impactFlashColor = Color.white;

        private readonly Dictionary<int, float> nextAllowedImpactTime = new Dictionary<int, float>();
        private Material feedbackMaterial;
        private Color baseColor;

        public void Configure(
            SpikeWallDefinition newDefinition,
            Vector3 newSurfaceNormal,
            Renderer[] newSpikeRenderers)
        {
            definition = newDefinition;
            surfaceNormal = newSurfaceNormal.normalized;
            spikeRenderers = newSpikeRenderers;
        }

        private void Awake()
        {
            BoxCollider trigger = GetComponent<BoxCollider>();
            trigger.isTrigger = true;

            if (spikeRenderers != null && spikeRenderers.Length > 0 && spikeRenderers[0] != null)
            {
                feedbackMaterial = spikeRenderers[0].material;
                baseColor = feedbackMaterial.color;

                foreach (Renderer spikeRenderer in spikeRenderers)
                {
                    if (spikeRenderer != null)
                    {
                        spikeRenderer.sharedMaterial = feedbackMaterial;
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            TryApplyImpact(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryApplyImpact(other);
        }

        private void TryApplyImpact(Collider other)
        {
            if (definition == null)
            {
                return;
            }

            IEnvironmentImpactTarget target = other.GetComponentInParent<IEnvironmentImpactTarget>();
            if (target is not Component targetComponent)
            {
                return;
            }

            int targetId = targetComponent.GetInstanceID();
            if (nextAllowedImpactTime.TryGetValue(targetId, out float nextTime) && Time.time < nextTime)
            {
                return;
            }

            var impact = new EnvironmentImpact(
                gameObject,
                transform.TransformDirection(surfaceNormal),
                definition.MinimumImpactSpeed,
                definition.EnemyDamage,
                definition.BrokenEnemyDamage,
                definition.PlayerDamage,
                definition.BounceSpeed);

            if (!target.TryReceiveEnvironmentImpact(impact))
            {
                return;
            }

            nextAllowedImpactTime[targetId] = Time.time + definition.RepeatCooldown;
            StopCoroutine(nameof(FlashImpact));
            StartCoroutine(nameof(FlashImpact));
        }

        private IEnumerator FlashImpact()
        {
            if (feedbackMaterial == null)
            {
                yield break;
            }

            feedbackMaterial.color = impactFlashColor;
            yield return new WaitForSeconds(definition.FlashDuration);
            feedbackMaterial.color = baseColor;
        }
    }
}

