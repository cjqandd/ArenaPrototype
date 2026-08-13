using ArenaPrototype.Enemies;
using UnityEngine;

namespace ArenaPrototype.Combat
{
    public sealed class PlayerCheer : MonoBehaviour
    {
        [SerializeField] private CheerDefinition definition;

        private float cheer;
        private float lastSuccessfulOffenseTime;

        public float Cheer => cheer;
        public float MaximumCheer => definition == null ? 1f : definition.MaximumCheer;
        public float NormalizedCheer => definition == null
            ? 0f
            : Mathf.Clamp01(cheer / definition.MaximumCheer);
        public int Level => definition == null ? 0 : definition.GetLevel(cheer);
        public bool IsDecaying => definition != null
            && cheer > 0f
            && Time.time - lastSuccessfulOffenseTime > definition.InactivityGraceDuration;
        public string LevelLabel => definition == null ? "冷场" : definition.GetLevelLabel(Level);
        public float HealthDamageMultiplier => definition == null
            ? 1f
            : definition.GetHealthDamageMultiplier(Level);
        public float PostureDamageMultiplier => definition == null
            ? 1f
            : definition.GetPostureDamageMultiplier(Level);

        public void Configure(CheerDefinition newDefinition)
        {
            definition = newDefinition;
        }

        private void OnEnable()
        {
            CombatResultStream.Resolved += HandleHitResolved;
        }

        private void OnDisable()
        {
            CombatResultStream.Resolved -= HandleHitResolved;
        }

        private void Update()
        {
            if (definition == null || cheer <= 0f)
            {
                return;
            }

            if (IsDecaying)
            {
                ChangeCheer(-definition.InactivityLossPerSecond * Time.deltaTime);
            }
        }

        private void HandleHitResolved(CombatHitResult result)
        {
            if (definition == null)
            {
                return;
            }

            if (result.Target == gameObject && result.HealthDamageApplied > 0f)
            {
                ChangeCheer(-definition.GetHitLoss(result.HealthDamageApplied));
                return;
            }

            if (result.Source != gameObject
                || result.Target == null
                || result.Target.GetComponent<EnemyWaveMember>() == null)
            {
                return;
            }

            float gain = result.Contact switch
            {
                CombatContact.Body when result.HealthDamageApplied > 0f
                    || result.PostureDamageApplied > 0f => definition.GetBodyGain(result),
                CombatContact.Shield => definition.GetShieldGain(result, Level),
                _ => 0f
            };

            if (gain <= 0f)
            {
                return;
            }

            ChangeCheer(gain);
            lastSuccessfulOffenseTime = Time.time;
        }

        private void ChangeCheer(float delta)
        {
            cheer = Mathf.Clamp(cheer + delta, 0f, definition.MaximumCheer);
        }

    }
}
