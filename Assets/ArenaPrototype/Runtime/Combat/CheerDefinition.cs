using UnityEngine;

namespace ArenaPrototype.Combat
{
    [CreateAssetMenu(fileName = "Cheer", menuName = "Arena Prototype/Cheer Definition")]
    public sealed class CheerDefinition : ScriptableObject
    {
        [Header("容量与等级")]
        [SerializeField, Min(1f)] private float maximumCheer = 100f;
        [SerializeField, Range(0f, 1f)] private float levelOneThreshold = 0.25f;
        [SerializeField, Range(0f, 1f)] private float levelTwoThreshold = 0.55f;
        [SerializeField, Range(0f, 1f)] private float levelThreeThreshold = 0.85f;

        [Header("攻击身体获得喝彩")]
        [SerializeField, Min(0f)] private float bodyHitBaseGain = 4f;
        [SerializeField, Min(0f)] private float healthDamageGainRate = 0.35f;
        [SerializeField, Min(0f)] private float postureDamageGainRate = 0.08f;
        [SerializeField, Min(0f)] private float postureBreakBonus = 8f;
        [SerializeField, Min(0f)] private float defeatBonus = 10f;

        [Header("攻击盾牌获得喝彩")]
        [SerializeField, Min(0f)] private float shieldHitBaseGain = 3.5f;
        [SerializeField, Min(0f)] private float shieldPostureGainRate = 0.06f;
        [SerializeField, Min(0f)] private float shieldBreakBonus = 6f;
        [SerializeField, Range(0f, 1f)] private float shieldGainAtLevelOne = 0.65f;
        [SerializeField, Range(0f, 1f)] private float shieldGainAtLevelTwo = 0.35f;
        [SerializeField, Range(0f, 1f)] private float shieldGainAtLevelThree = 0.15f;

        [Header("降低喝彩")]
        [SerializeField, Min(0f)] private float hitBaseLoss = 8f;
        [SerializeField, Min(0f)] private float healthDamageLossRate = 0.40f;
        [SerializeField, Min(0f)] private float inactivityGraceDuration = 4f;
        [SerializeField, Min(0f)] private float inactivityLossPerSecond = 8f;

        [Header("喝彩战斗收益")]
        [SerializeField, Min(1f)] private float healthDamageAtLevelOne = 1.05f;
        [SerializeField, Min(1f)] private float healthDamageAtLevelTwo = 1.10f;
        [SerializeField, Min(1f)] private float healthDamageAtLevelThree = 1.18f;
        [SerializeField, Min(1f)] private float postureDamageAtLevelOne = 1.05f;
        [SerializeField, Min(1f)] private float postureDamageAtLevelTwo = 1.10f;
        [SerializeField, Min(1f)] private float postureDamageAtLevelThree = 1.15f;

        public float MaximumCheer => maximumCheer;
        public float InactivityGraceDuration => inactivityGraceDuration;
        public float InactivityLossPerSecond => inactivityLossPerSecond;

        public int GetLevel(float cheer)
        {
            float normalizedCheer = Mathf.Clamp01(cheer / maximumCheer);
            if (normalizedCheer >= levelThreeThreshold) return 3;
            if (normalizedCheer >= levelTwoThreshold) return 2;
            if (normalizedCheer >= levelOneThreshold) return 1;
            return 0;
        }

        private void OnValidate()
        {
            levelOneThreshold = Mathf.Clamp01(levelOneThreshold);
            levelTwoThreshold = Mathf.Clamp(levelTwoThreshold, levelOneThreshold, 1f);
            levelThreeThreshold = Mathf.Clamp(levelThreeThreshold, levelTwoThreshold, 1f);
        }

        public float GetBodyGain(CombatHitResult result)
        {
            float gain = bodyHitBaseGain
                + result.HealthDamageApplied * healthDamageGainRate
                + result.PostureDamageApplied * postureDamageGainRate;
            if (result.BrokePosture) gain += postureBreakBonus;
            if (result.Defeated) gain += defeatBonus;
            return gain;
        }

        public float GetShieldGain(CombatHitResult result, int cheerLevel)
        {
            float levelMultiplier = cheerLevel switch
            {
                1 => shieldGainAtLevelOne,
                2 => shieldGainAtLevelTwo,
                3 => shieldGainAtLevelThree,
                _ => 1f
            };
            float gain = shieldHitBaseGain
                + result.PostureDamageApplied * shieldPostureGainRate;
            if (result.BrokePosture) gain += shieldBreakBonus;
            return gain * levelMultiplier;
        }

        public float GetHitLoss(float healthDamageApplied)
        {
            return hitBaseLoss + healthDamageApplied * healthDamageLossRate;
        }

        public float GetHealthDamageMultiplier(int level)
        {
            return level switch
            {
                1 => healthDamageAtLevelOne,
                2 => healthDamageAtLevelTwo,
                3 => healthDamageAtLevelThree,
                _ => 1f
            };
        }

        public float GetPostureDamageMultiplier(int level)
        {
            return level switch
            {
                1 => postureDamageAtLevelOne,
                2 => postureDamageAtLevelTwo,
                3 => postureDamageAtLevelThree,
                _ => 1f
            };
        }

        public string GetLevelLabel(int level)
        {
            return level switch
            {
                1 => "投入",
                2 => "沸腾",
                3 => "狂热",
                _ => "冷场"
            };
        }
    }
}
