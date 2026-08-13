using UnityEngine;

namespace ArenaPrototype.Combat
{
    public enum RunBoon
    {
        Ferocity,
        Breaker,
        Footwork
    }

    public sealed class PlayerCombatGrowth : MonoBehaviour
    {
        [SerializeField] private PlayerCheer cheer;
        [SerializeField, Min(0f)] private float ferocityDamagePerLevel = 0.15f;
        [SerializeField, Min(0f)] private float breakerPosturePerLevel = 0.25f;
        [SerializeField, Range(0.5f, 1f)] private float footworkCooldownPerLevel = 0.85f;

        private int ferocityLevel;
        private int breakerLevel;
        private int footworkLevel;

        public float HealthDamageMultiplier => CheerHealthMultiplier
            * (1f + ferocityLevel * ferocityDamagePerLevel);
        public float PostureDamageMultiplier => CheerPostureMultiplier
            * (1f + breakerLevel * breakerPosturePerLevel);
        public float DodgeCooldownMultiplier => Mathf.Max(
            0.55f,
            Mathf.Pow(footworkCooldownPerLevel, footworkLevel));

        public void Configure(PlayerCheer newCheer)
        {
            cheer = newCheer;
        }

        public void ApplyBoon(RunBoon boon)
        {
            SetBoonLevel(boon, GetBoonLevel(boon) + 1);
        }

        public int GetBoonLevel(RunBoon boon)
        {
            return boon switch
            {
                RunBoon.Ferocity => ferocityLevel,
                RunBoon.Breaker => breakerLevel,
                RunBoon.Footwork => footworkLevel,
                _ => 0
            };
        }

        public string GetBoonTitle(RunBoon boon)
        {
            return boon switch
            {
                RunBoon.Ferocity => "猛攻",
                RunBoon.Breaker => "破势",
                RunBoon.Footwork => "轻步",
                _ => boon.ToString()
            };
        }

        public string GetBoonDescription(RunBoon boon)
        {
            return boon switch
            {
                RunBoon.Ferocity => $"武器与踢击生命伤害 +{ferocityDamagePerLevel * 100f:0}%",
                RunBoon.Breaker => $"武器与踢击架势伤害 +{breakerPosturePerLevel * 100f:0}%",
                RunBoon.Footwork => $"闪避冷却 × {footworkCooldownPerLevel:0.00}",
                _ => string.Empty
            };
        }

        public string GetActiveBoonsLabel()
        {
            if (ferocityLevel + breakerLevel + footworkLevel == 0)
            {
                return "暂无强化";
            }

            string label = string.Empty;
            AppendBoonLabel(ref label, RunBoon.Ferocity, ferocityLevel);
            AppendBoonLabel(ref label, RunBoon.Breaker, breakerLevel);
            AppendBoonLabel(ref label, RunBoon.Footwork, footworkLevel);
            return label;
        }

        private float CheerHealthMultiplier => cheer == null
            ? 1f
            : cheer.HealthDamageMultiplier;
        private float CheerPostureMultiplier => cheer == null
            ? 1f
            : cheer.PostureDamageMultiplier;

        private void SetBoonLevel(RunBoon boon, int level)
        {
            level = Mathf.Max(0, level);
            switch (boon)
            {
                case RunBoon.Ferocity:
                    ferocityLevel = level;
                    break;
                case RunBoon.Breaker:
                    breakerLevel = level;
                    break;
                case RunBoon.Footwork:
                    footworkLevel = level;
                    break;
            }
        }

        private void AppendBoonLabel(ref string label, RunBoon boon, int level)
        {
            if (level <= 0)
            {
                return;
            }

            if (label.Length > 0)
            {
                label += "　|　";
            }

            label += $"{GetBoonTitle(boon)} {level}级";
        }
    }
}
