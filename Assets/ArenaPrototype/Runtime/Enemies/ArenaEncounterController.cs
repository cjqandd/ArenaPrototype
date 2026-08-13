using System;
using System.Collections;
using ArenaPrototype.Combat;
using ArenaPrototype.Weapons;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArenaPrototype.Enemies
{
    public enum ArenaEncounterPhase
    {
        DifficultySelection,
        Preparing,
        Fighting,
        WaveCleared,
        BoonSelection,
        PlayerDefeated,
        Victory
    }

    public enum ArenaDifficulty
    {
        Easy,
        Standard
    }

    public readonly struct ArenaEncounterSnapshot
    {
        public ArenaEncounterSnapshot(
            ArenaEncounterPhase phase,
            int fightNumber,
            int totalFights,
            int remainingEnemies,
            string fightLabel,
            string difficultyLabel,
            bool allowsPlayerActions)
        {
            Phase = phase;
            FightNumber = fightNumber;
            TotalFights = totalFights;
            RemainingEnemies = remainingEnemies;
            FightLabel = fightLabel;
            DifficultyLabel = difficultyLabel;
            AllowsPlayerActions = allowsPlayerActions;
        }

        public ArenaEncounterPhase Phase { get; }
        public int FightNumber { get; }
        public int TotalFights { get; }
        public int RemainingEnemies { get; }
        public string FightLabel { get; }
        public string DifficultyLabel { get; }
        public bool AllowsPlayerActions { get; }
        public bool IsModalSelection => Phase == ArenaEncounterPhase.DifficultySelection
            || Phase == ArenaEncounterPhase.BoonSelection;
    }

    public sealed class ArenaEncounterController : MonoBehaviour
    {
        [Serializable]
        public sealed class Wave
        {
            [SerializeField] private string label = "战斗";
            [SerializeField] private EnemyWaveMember[] enemies;

            public Wave(string newLabel, EnemyWaveMember[] newEnemies)
            {
                label = newLabel;
                enemies = newEnemies;
            }

            public string Label => label;
            public EnemyWaveMember[] Enemies => enemies;
        }

        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerCombatGrowth combatGrowth;
        [SerializeField] private Wave[] waves;
        [SerializeField] private WeaponDropRegistry weaponDropRegistry;
        [SerializeField] private EnemyAttackCoordinator attackCoordinator;
        [SerializeField, Min(0f)] private float openingDelay = 1f;
        [SerializeField, Min(0f)] private float clearedPickupDuration = 2f;
        [SerializeField, Min(0f)] private float retryDelayAfterRespawn = 0.75f;
        [SerializeField, Min(0f)] private float readyDelayAfterBoon = 0.6f;

        private ArenaEncounterPhase phase;
        private int currentWaveIndex;
        private int remainingEnemies;
        private bool difficultyChosen;
        private bool boonChosen;
        private bool gameplayActionsAllowed;
        private bool externallyPaused;
        private float enemyStatMultiplier = 1f;
        private string difficultyLabel = "标准";

        public ArenaEncounterSnapshot Snapshot
        {
            get
            {
                int totalFights = waves?.Length ?? 0;
                int fightNumber = totalFights <= 0
                    ? 0
                    : Mathf.Clamp(currentWaveIndex + 1, 1, totalFights);
                string fightLabel = currentWaveIndex >= 0
                    && currentWaveIndex < totalFights
                    ? waves[currentWaveIndex].Label
                    : string.Empty;
                return new ArenaEncounterSnapshot(
                    phase,
                    fightNumber,
                    totalFights,
                    remainingEnemies,
                    fightLabel,
                    difficultyLabel,
                    AllowsPlayerActions);
            }
        }

        public bool AllowsPlayerActions => gameplayActionsAllowed
            && !externallyPaused
            && (playerHealth == null || !playerHealth.IsDefeated);

        public void Configure(
            PlayerHealth newPlayerHealth,
            PlayerCombatGrowth newCombatGrowth,
            Wave[] newWaves,
            WeaponDropRegistry newWeaponDropRegistry,
            EnemyAttackCoordinator newAttackCoordinator)
        {
            playerHealth = newPlayerHealth;
            combatGrowth = newCombatGrowth;
            waves = newWaves;
            weaponDropRegistry = newWeaponDropRegistry;
            attackCoordinator = newAttackCoordinator;
        }

        private void Awake()
        {
            SetAllEnemiesActive(false);
            phase = ArenaEncounterPhase.DifficultySelection;
            playerHealth?.SetDamageEnabled(false);
            SetGameplayActionsAllowed(false);
        }

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.Defeated += HandlePlayerDefeated;
                playerHealth.Respawned += HandlePlayerRespawned;
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.Defeated -= HandlePlayerDefeated;
                playerHealth.Respawned -= HandlePlayerRespawned;
            }

            UnsubscribeFromCurrentWave();
            attackCoordinator?.ResetClaims();
        }

        private IEnumerator Start()
        {
            SetGameplayActionsAllowed(false);
            if (waves == null || waves.Length == 0)
            {
                phase = ArenaEncounterPhase.Victory;
                yield break;
            }

            while (!difficultyChosen)
            {
                yield return null;
            }

            phase = ArenaEncounterPhase.Preparing;
            if (openingDelay > 0f)
            {
                yield return new WaitForSeconds(openingDelay);
            }

            currentWaveIndex = 0;
            while (currentWaveIndex < waves.Length)
            {
                bool waveCleared = false;
                while (!waveCleared)
                {
                    phase = ArenaEncounterPhase.Preparing;
                    SetGameplayActionsAllowed(false);
                    while (playerHealth != null && playerHealth.IsDefeated)
                    {
                        yield return null;
                    }

                    PrepareCurrentWave();
                    phase = ArenaEncounterPhase.Fighting;
                    playerHealth?.SetDamageEnabled(true);
                    SetGameplayActionsAllowed(true);

                    while (remainingEnemies > 0
                        && (playerHealth == null || !playerHealth.IsDefeated))
                    {
                        yield return null;
                    }

                    if (playerHealth != null && playerHealth.IsDefeated)
                    {
                        phase = ArenaEncounterPhase.PlayerDefeated;
                        SetGameplayActionsAllowed(false);
                        DeactivateCurrentWave();
                        while (playerHealth.IsDefeated)
                        {
                            yield return null;
                        }

                        if (retryDelayAfterRespawn > 0f)
                        {
                            yield return new WaitForSeconds(retryDelayAfterRespawn);
                        }
                        continue;
                    }

                    waveCleared = remainingEnemies <= 0;
                }

                phase = ArenaEncounterPhase.WaveCleared;
                playerHealth?.SetDamageEnabled(false);
                SetGameplayActionsAllowed(true);
                ArcherProjectile.DestroyAllActive();
                if (clearedPickupDuration > 0f)
                {
                    yield return new WaitForSeconds(clearedPickupDuration);
                }
                DeactivateCurrentWave();

                bool hasAnotherFight = currentWaveIndex < waves.Length - 1;
                if (hasAnotherFight)
                {
                    boonChosen = false;
                    phase = ArenaEncounterPhase.BoonSelection;
                    playerHealth?.CancelCurrentActions(false);
                    SetGameplayActionsAllowed(false);
                    while (!boonChosen)
                    {
                        yield return null;
                    }

                    currentWaveIndex++;
                    phase = ArenaEncounterPhase.Preparing;
                    if (readyDelayAfterBoon > 0f)
                    {
                        yield return new WaitForSeconds(readyDelayAfterBoon);
                    }
                }
                else
                {
                    currentWaveIndex++;
                }
            }

            phase = ArenaEncounterPhase.Victory;
            playerHealth?.CancelCurrentActions(false);
            playerHealth?.SetDamageEnabled(false);
            SetGameplayActionsAllowed(false);
        }

        public bool TrySelectDifficulty(ArenaDifficulty difficulty)
        {
            if (difficultyChosen || phase != ArenaEncounterPhase.DifficultySelection)
            {
                return false;
            }

            difficultyLabel = difficulty == ArenaDifficulty.Easy ? "简单" : "标准";
            enemyStatMultiplier = difficulty == ArenaDifficulty.Easy ? 0.7f : 1f;
            difficultyChosen = true;
            phase = ArenaEncounterPhase.Preparing;
            return true;
        }

        public bool TrySelectBoon(RunBoon boon)
        {
            if (phase != ArenaEncounterPhase.BoonSelection || boonChosen)
            {
                return false;
            }

            combatGrowth?.ApplyBoon(boon);
            boonChosen = true;
            return true;
        }

        public void SetPaused(bool paused)
        {
            externallyPaused = paused;
            RefreshPlayerActions();
        }

        public bool TryRestartRun()
        {
            if (phase != ArenaEncounterPhase.Victory)
            {
                return false;
            }

            Time.timeScale = 1f;
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.buildIndex >= 0)
            {
                SceneManager.LoadScene(activeScene.buildIndex);
            }
            else
            {
                SceneManager.LoadScene(activeScene.name);
            }

            return true;
        }

        private void PrepareCurrentWave()
        {
            UnsubscribeFromCurrentWave();
            attackCoordinator?.ResetClaims();
            remainingEnemies = 0;

            foreach (EnemyWaveMember enemy in waves[currentWaveIndex].Enemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                enemy.Defeated += HandleEnemyDefeated;
                enemy.PrepareForWave(enemyStatMultiplier);
                remainingEnemies++;
            }
        }

        private void HandleEnemyDefeated(EnemyWaveMember enemy)
        {
            remainingEnemies = Mathf.Max(0, remainingEnemies - 1);
        }

        private void HandlePlayerDefeated()
        {
            if (phase == ArenaEncounterPhase.Fighting)
            {
                phase = ArenaEncounterPhase.PlayerDefeated;
                playerHealth?.SetDamageEnabled(false);
                SetGameplayActionsAllowed(false);
            }
        }

        private void HandlePlayerRespawned()
        {
            RefreshPlayerActions();
        }

        private void DeactivateCurrentWave()
        {
            if (waves == null || currentWaveIndex < 0 || currentWaveIndex >= waves.Length)
            {
                return;
            }

            foreach (EnemyWaveMember enemy in waves[currentWaveIndex].Enemies)
            {
                if (enemy != null)
                {
                    enemy.gameObject.SetActive(false);
                }
            }

            ArcherProjectile.DestroyAllActive();
            weaponDropRegistry?.CleanupUnequippedDrops();
            attackCoordinator?.ResetClaims();
            UnsubscribeFromCurrentWave();
        }

        private void UnsubscribeFromCurrentWave()
        {
            if (waves == null || currentWaveIndex < 0 || currentWaveIndex >= waves.Length)
            {
                return;
            }

            foreach (EnemyWaveMember enemy in waves[currentWaveIndex].Enemies)
            {
                if (enemy != null)
                {
                    enemy.Defeated -= HandleEnemyDefeated;
                }
            }
        }

        private void SetAllEnemiesActive(bool active)
        {
            if (waves == null)
            {
                return;
            }

            foreach (Wave wave in waves)
            {
                if (wave?.Enemies == null)
                {
                    continue;
                }

                foreach (EnemyWaveMember enemy in wave.Enemies)
                {
                    if (enemy != null)
                    {
                        enemy.gameObject.SetActive(active);
                    }
                }
            }
        }

        private void SetGameplayActionsAllowed(bool allowed)
        {
            gameplayActionsAllowed = allowed;
            RefreshPlayerActions();
        }

        private void RefreshPlayerActions()
        {
            playerHealth?.SetActionsEnabled(AllowsPlayerActions);
        }
    }
}
