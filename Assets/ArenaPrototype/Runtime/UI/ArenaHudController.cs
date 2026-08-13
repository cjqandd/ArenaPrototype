using ArenaPrototype.Combat;
using ArenaPrototype.Enemies;
using ArenaPrototype.Movement;
using ArenaPrototype.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArenaPrototype.UI
{
    public sealed class ArenaHudController : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerCheer playerCheer;
        [SerializeField] private PlayerCombatGrowth combatGrowth;
        [SerializeField] private PlayerMeleeCombat meleeCombat;
        [SerializeField] private PlayerDodge playerDodge;
        [SerializeField] private PlayerWeaponEquipment weaponEquipment;
        [SerializeField] private ArenaEncounterController encounter;

        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle sectionStyle;
        private GUIStyle centeredStyle;
        private GUIStyle smallStyle;
        private Font chineseFont;
        private bool isPaused;
        private bool modalTimePaused;
        private bool showHelp;
        private string feedbackText = string.Empty;
        private Color feedbackColor = Color.white;
        private float feedbackUntil;
        private bool playerHitFeedback;

        public void Configure(
            PlayerHealth newPlayerHealth,
            PlayerCheer newPlayerCheer,
            PlayerCombatGrowth newCombatGrowth,
            PlayerMeleeCombat newMeleeCombat,
            PlayerDodge newPlayerDodge,
            PlayerWeaponEquipment newWeaponEquipment,
            ArenaEncounterController newEncounter)
        {
            playerHealth = newPlayerHealth;
            playerCheer = newPlayerCheer;
            combatGrowth = newCombatGrowth;
            meleeCombat = newMeleeCombat;
            playerDodge = newPlayerDodge;
            weaponEquipment = newWeaponEquipment;
            encounter = newEncounter;
        }

        private void OnEnable()
        {
            CombatResultStream.Resolved += HandleCombatResult;
        }

        private void OnDisable()
        {
            CombatResultStream.Resolved -= HandleCombatResult;
            if (isPaused || modalTimePaused)
            {
                Time.timeScale = 1f;
                encounter?.SetPaused(false);
                isPaused = false;
                modalTimePaused = false;
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || encounter == null)
            {
                return;
            }

            ArenaEncounterSnapshot snapshot = encounter.Snapshot;
            SyncModalTimePause(snapshot);
            if (snapshot.Phase == ArenaEncounterPhase.DifficultySelection)
            {
                if (keyboard.digit1Key.wasPressedThisFrame
                    || keyboard.numpad1Key.wasPressedThisFrame)
                {
                    encounter.TrySelectDifficulty(ArenaDifficulty.Easy);
                }
                else if (keyboard.digit2Key.wasPressedThisFrame
                    || keyboard.numpad2Key.wasPressedThisFrame)
                {
                    encounter.TrySelectDifficulty(ArenaDifficulty.Standard);
                }
            }
            else if (snapshot.Phase == ArenaEncounterPhase.BoonSelection)
            {
                if (keyboard.digit1Key.wasPressedThisFrame
                    || keyboard.numpad1Key.wasPressedThisFrame)
                {
                    encounter.TrySelectBoon(RunBoon.Ferocity);
                }
                else if (keyboard.digit2Key.wasPressedThisFrame
                    || keyboard.numpad2Key.wasPressedThisFrame)
                {
                    encounter.TrySelectBoon(RunBoon.Breaker);
                }
                else if (keyboard.digit3Key.wasPressedThisFrame
                    || keyboard.numpad3Key.wasPressedThisFrame)
                {
                    encounter.TrySelectBoon(RunBoon.Footwork);
                }
            }
            else if (snapshot.Phase == ArenaEncounterPhase.Victory
                && keyboard.rKey.wasPressedThisFrame)
            {
                encounter.TryRestartRun();
            }

            if (keyboard.f1Key.wasPressedThisFrame)
            {
                showHelp = !showHelp;
            }

            bool canPause = !snapshot.IsModalSelection
                && snapshot.Phase != ArenaEncounterPhase.Victory;
            if (canPause && keyboard.escapeKey.wasPressedThisFrame)
            {
                SetPaused(!isPaused);
            }
        }

        private void OnGUI()
        {
            if (encounter == null)
            {
                return;
            }

            EnsureStyles();
            ArenaEncounterSnapshot snapshot = encounter.Snapshot;
            if (snapshot.Phase == ArenaEncounterPhase.DifficultySelection)
            {
                DrawDifficultySelection();
                DrawHelpHint();
                return;
            }

            DrawTopStatus(snapshot);
            DrawPlayerHealth();
            DrawCheer();
            DrawEquipmentAndDodge();
            DrawPickupPrompt();
            DrawCombatFeedback();

            if (snapshot.Phase == ArenaEncounterPhase.BoonSelection)
            {
                DrawBoonSelection();
            }
            else if (snapshot.Phase == ArenaEncounterPhase.Victory)
            {
                DrawVictory();
            }

            if (isPaused)
            {
                DrawPauseOverlay();
            }
            if (showHelp)
            {
                DrawHelpOverlay();
            }
            else
            {
                DrawHelpHint();
            }
        }

        private void DrawTopStatus(ArenaEncounterSnapshot snapshot)
        {
            Rect topBar = new Rect(0f, 0f, Screen.width, 78f);
            Fill(topBar, new Color(0.025f, 0.03f, 0.04f, 0.88f));

            string title = snapshot.Phase == ArenaEncounterPhase.Victory
                ? "胜利"
                : $"第 {snapshot.FightNumber} / {snapshot.TotalFights} 场";
            string status = snapshot.Phase switch
            {
                ArenaEncounterPhase.Preparing => "准备战斗",
                ArenaEncounterPhase.Fighting => $"剩余敌人：{snapshot.RemainingEnemies}",
                ArenaEncounterPhase.WaveCleared => "清场完成　可拾取武器",
                ArenaEncounterPhase.BoonSelection => "选择强化",
                ArenaEncounterPhase.PlayerDefeated => "战败　正在重试本场",
                ArenaEncounterPhase.Victory => "三场战斗全部完成",
                _ => string.Empty
            };

            GUI.Label(new Rect(0f, 10f, Screen.width, 34f), title, titleStyle);
            string context = snapshot.Phase == ArenaEncounterPhase.Victory
                ? $"{snapshot.DifficultyLabel}　|　{status}"
                : $"{snapshot.DifficultyLabel}　|　{GetFightName(snapshot.FightLabel)}　|　{status}";
            GUI.Label(
                new Rect(0f, 43f, Screen.width, 24f),
                context,
                subtitleStyle);
        }

        private void DrawPlayerHealth()
        {
            GetBottomPanels(out Rect panel, out _, out _);
            float width = panel.width - 28f;
            Fill(panel, new Color(0.025f, 0.03f, 0.04f, 0.88f));
            GUI.Label(new Rect(panel.x + 14f, panel.y + 8f, width, 20f), "生命", sectionStyle);
            float normalized = playerHealth == null ? 0f : playerHealth.HealthNormalized;
            DrawBar(
                new Rect(panel.x + 14f, panel.y + 34f, width, 20f),
                normalized,
                new Color(0.10f, 0.82f, 0.25f));
            if (playerHealth != null)
            {
                GUI.Label(
                    new Rect(panel.x + 14f, panel.y + 34f, width, 20f),
                    $"{playerHealth.CurrentHealth:0} / {playerHealth.MaximumHealth:0}",
                    centeredStyle);
            }
        }

        private void DrawCheer()
        {
            GetBottomPanels(out _, out Rect panel, out _);
            float width = panel.width - 28f;
            Fill(panel, new Color(0.025f, 0.03f, 0.04f, 0.88f));
            int level = playerCheer == null ? 0 : playerCheer.Level;
            string decay = playerCheer?.IsDecaying == true ? "　衰减中" : string.Empty;
            string label = playerCheer == null
                ? "喝彩 0级"
                : $"喝彩 {level}级　{playerCheer.LevelLabel}{decay}";
            GUI.Label(new Rect(panel.x + 14f, panel.y + 8f, width, 20f), label, sectionStyle);
            Color cheerColor = level switch
            {
                1 => new Color(1f, 0.72f, 0.10f),
                2 => new Color(1f, 0.42f, 0.05f),
                3 => new Color(1f, 0.10f, 0.04f),
                _ => new Color(0.55f, 0.55f, 0.58f)
            };
            DrawBar(
                new Rect(panel.x + 14f, panel.y + 34f, width, 20f),
                playerCheer == null ? 0f : playerCheer.NormalizedCheer,
                cheerColor);
            if (playerCheer != null)
            {
                GUI.Label(
                    new Rect(panel.x + 14f, panel.y + 34f, width, 20f),
                    $"{playerCheer.Cheer:0} / {playerCheer.MaximumCheer:0}　　伤害 ×{playerCheer.HealthDamageMultiplier:0.00}",
                    centeredStyle);
            }
        }

        private void DrawEquipmentAndDodge()
        {
            GetBottomPanels(out _, out _, out Rect panel);
            float width = panel.width - 28f;
            Fill(panel, new Color(0.025f, 0.03f, 0.04f, 0.88f));
            string weaponName = meleeCombat?.CurrentWeapon == null
                ? "未装备"
                : GetWeaponName(meleeCombat.CurrentWeapon);
            GUI.Label(
                new Rect(panel.x + 14f, panel.y + 8f, width, 20f),
                $"武器：{weaponName}",
                sectionStyle);

            float dodgeReady = playerDodge == null
                ? 0f
                : playerDodge.IsReady ? 1f : 1f - playerDodge.CooldownNormalized;
            DrawBar(
                new Rect(panel.x + 14f, panel.y + 34f, width, 20f),
                dodgeReady,
                playerDodge?.IsInvulnerable == true
                    ? Color.white
                    : new Color(0.12f, 0.82f, 1f));
            GUI.Label(
                new Rect(panel.x + 14f, panel.y + 34f, width, 20f),
                playerDodge?.IsReady == true ? "闪避就绪" : "闪避冷却中",
                centeredStyle);

            if (combatGrowth != null)
            {
                GUI.Label(
                    new Rect(panel.x, panel.y - 20f, panel.width, 18f),
                    combatGrowth.GetActiveBoonsLabel(),
                    smallStyle);
            }
        }

        private void DrawPickupPrompt()
        {
            WeaponInstance candidate = weaponEquipment?.PickupCandidate;
            if (candidate?.Definition == null)
            {
                return;
            }

            Rect prompt = new Rect((Screen.width - 320f) * 0.5f, Screen.height - 144f, 320f, 32f);
            Fill(prompt, new Color(0.06f, 0.05f, 0.01f, 0.92f));
            GUI.Label(
                prompt,
                $"E　交换为：{GetWeaponName(candidate.Definition)}",
                centeredStyle);
        }

        private void DrawCombatFeedback()
        {
            if (Time.unscaledTime >= feedbackUntil || feedbackText.Length == 0)
            {
                return;
            }

            float remaining = Mathf.Clamp01((feedbackUntil - Time.unscaledTime) / 0.65f);
            if (playerHitFeedback)
            {
                Fill(
                    new Rect(0f, 0f, Screen.width, Screen.height),
                    new Color(0.75f, 0.02f, 0.02f, 0.08f * remaining));
            }

            Color previousColor = GUI.color;
            GUI.color = new Color(feedbackColor.r, feedbackColor.g, feedbackColor.b, remaining);
            GUI.Label(
                new Rect((Screen.width - 360f) * 0.5f, Screen.height * 0.58f, 360f, 36f),
                feedbackText,
                titleStyle);
            GUI.color = previousColor;
        }

        private void DrawDifficultySelection()
        {
            DrawModalBackdrop();
            float width = Mathf.Min(520f, Screen.width - 40f);
            Rect panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - 300f) * 0.5f, width, 300f);
            Fill(panel, new Color(0.04f, 0.05f, 0.07f, 0.98f));
            GUI.Label(new Rect(panel.x, panel.y + 22f, panel.width, 38f), "选择难度", titleStyle);
            GUI.Label(
                new Rect(panel.x + 24f, panel.y + 62f, panel.width - 48f, 42f),
                "敌人编队与机制完全相同，仅调整敌人数值。",
                centeredStyle);

            if (GUI.Button(
                    new Rect(panel.x + 42f, panel.y + 120f, panel.width - 84f, 52f),
                    "1　简单　　敌人数值 70%"))
            {
                encounter.TrySelectDifficulty(ArenaDifficulty.Easy);
            }
            if (GUI.Button(
                    new Rect(panel.x + 42f, panel.y + 188f, panel.width - 84f, 52f),
                    "2　标准　　敌人数值 100%"))
            {
                encounter.TrySelectDifficulty(ArenaDifficulty.Standard);
            }
        }

        private void DrawBoonSelection()
        {
            DrawModalBackdrop();
            float panelWidth = Mathf.Min(840f, Screen.width - 40f);
            Rect panel = new Rect(
                (Screen.width - panelWidth) * 0.5f,
                (Screen.height - 330f) * 0.5f,
                panelWidth,
                330f);
            Fill(panel, new Color(0.04f, 0.05f, 0.07f, 0.98f));
            GUI.Label(new Rect(panel.x, panel.y + 20f, panel.width, 40f), "选择一项强化", titleStyle);
            GUI.Label(
                new Rect(panel.x, panel.y + 58f, panel.width, 24f),
                "强化在本次三场战斗中持续生效",
                subtitleStyle);

            RunBoon[] boons = { RunBoon.Ferocity, RunBoon.Breaker, RunBoon.Footwork };
            float cardWidth = (panel.width - 80f) / 3f;
            for (int i = 0; i < boons.Length; i++)
            {
                RunBoon boon = boons[i];
                Rect card = new Rect(
                    panel.x + 20f + i * (cardWidth + 20f),
                    panel.y + 102f,
                    cardWidth,
                    180f);
                Fill(card, new Color(0.09f, 0.10f, 0.13f, 1f));
                GUI.Label(
                    new Rect(card.x + 8f, card.y + 18f, card.width - 16f, 28f),
                    $"{i + 1}　{combatGrowth?.GetBoonTitle(boon)}",
                    sectionStyle);
                GUI.Label(
                    new Rect(card.x + 14f, card.y + 58f, card.width - 28f, 48f),
                    combatGrowth?.GetBoonDescription(boon) ?? string.Empty,
                    centeredStyle);
                int nextLevel = (combatGrowth?.GetBoonLevel(boon) ?? 0) + 1;
                if (GUI.Button(
                        new Rect(card.x + 18f, card.y + 122f, card.width - 36f, 40f),
                        $"选择　升至 {nextLevel} 级"))
                {
                    encounter.TrySelectBoon(boon);
                }
            }
        }

        private void DrawVictory()
        {
            Rect panel = new Rect((Screen.width - 480f) * 0.5f, 112f, 480f, 142f);
            Fill(panel, new Color(0.08f, 0.06f, 0.01f, 0.94f));
            GUI.Label(new Rect(panel.x, panel.y + 10f, panel.width, 34f), "征服竞技场", titleStyle);
            GUI.Label(
                new Rect(panel.x, panel.y + 46f, panel.width, 24f),
                combatGrowth?.GetActiveBoonsLabel() ?? string.Empty,
                subtitleStyle);
            if (GUI.Button(
                    new Rect(panel.x + 100f, panel.y + 86f, panel.width - 200f, 40f),
                    "R　开始新一局"))
            {
                encounter.TryRestartRun();
            }
        }

        private void DrawPauseOverlay()
        {
            DrawModalBackdrop();
            Rect panel = new Rect((Screen.width - 420f) * 0.5f, (Screen.height - 220f) * 0.5f, 420f, 220f);
            Fill(panel, new Color(0.04f, 0.05f, 0.07f, 0.98f));
            GUI.Label(new Rect(panel.x, panel.y + 24f, panel.width, 40f), "已暂停", titleStyle);
            GUI.Label(
                new Rect(panel.x, panel.y + 76f, panel.width, 28f),
                "Esc　继续　　F1　操作说明",
                centeredStyle);
            if (GUI.Button(new Rect(panel.x + 70f, panel.y + 130f, panel.width - 140f, 46f), "继续游戏"))
            {
                SetPaused(false);
            }
        }

        private void DrawHelpOverlay()
        {
            Rect panel = new Rect(24f, 94f, 350f, 242f);
            Fill(panel, new Color(0.025f, 0.03f, 0.04f, 0.95f));
            GUI.Label(new Rect(panel.x + 18f, panel.y + 14f, panel.width - 36f, 26f), "操作说明", sectionStyle);
            GUI.Label(
                new Rect(panel.x + 18f, panel.y + 48f, panel.width - 36f, 174f),
                "WASD　移动\n鼠标　瞄准\n鼠标左键　武器攻击\n鼠标右键　踢击\n空格　闪避 / 取消攻击后摇\nE　交换高亮武器\nEsc　暂停\nF1　显示或隐藏本说明",
                smallStyle);
        }

        private void DrawHelpHint()
        {
            GUI.Label(new Rect(18f, Screen.height - 24f, 260f, 18f), "F1　操作说明　　Esc　暂停", smallStyle);
        }

        private void HandleCombatResult(CombatHitResult result)
        {
            if (playerHealth == null)
            {
                return;
            }

            playerHitFeedback = result.Target == playerHealth.gameObject
                && result.HealthDamageApplied > 0f;
            if (playerHitFeedback)
            {
                feedbackText = $"受击　-{result.HealthDamageApplied:0}";
                feedbackColor = new Color(1f, 0.18f, 0.12f);
            }
            else if (result.Source == playerHealth.gameObject)
            {
                if (result.Defeated)
                {
                    feedbackText = "击败";
                    feedbackColor = new Color(1f, 0.72f, 0.10f);
                }
                else if (result.Contact == CombatContact.Shield && result.BrokePosture)
                {
                    feedbackText = "破盾";
                    feedbackColor = new Color(1f, 0.52f, 0.05f);
                }
                else if (result.BrokePosture)
                {
                    feedbackText = "失衡";
                    feedbackColor = new Color(1f, 0.72f, 0.10f);
                }
                else if (result.Contact == CombatContact.Shield)
                {
                    feedbackText = "格挡";
                    feedbackColor = new Color(0.72f, 0.78f, 0.86f);
                }
                else
                {
                    feedbackText = $"命中　{result.HealthDamageApplied:0}";
                    feedbackColor = Color.white;
                }
            }
            else
            {
                return;
            }

            feedbackUntil = Time.unscaledTime + 0.65f;
        }

        private void SetPaused(bool paused)
        {
            isPaused = paused;
            encounter?.SetPaused(paused);
            Time.timeScale = paused || modalTimePaused ? 0f : 1f;
        }

        private void SyncModalTimePause(ArenaEncounterSnapshot snapshot)
        {
            bool shouldPause = snapshot.Phase == ArenaEncounterPhase.BoonSelection;
            if (modalTimePaused == shouldPause)
            {
                return;
            }

            modalTimePaused = shouldPause;
            Time.timeScale = modalTimePaused || isPaused ? 0f : 1f;
        }

        private void EnsureStyles()
        {
            if (chineseFont == null)
            {
                chineseFont = Font.CreateDynamicFontFromOSFont(
                    new[] { "Microsoft YaHei UI", "Microsoft YaHei", "SimHei", "Arial" },
                    16);
            }
            if (chineseFont != null)
            {
                GUI.skin.font = chineseFont;
            }

            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                font = chineseFont,
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                font = chineseFont,
                fontSize = 13,
                normal = { textColor = new Color(1f, 0.76f, 0.16f) }
            };
            sectionStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                font = chineseFont,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            centeredStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                font = chineseFont,
                fontSize = 13,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
            smallStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                font = chineseFont,
                fontSize = 12,
                normal = { textColor = new Color(0.82f, 0.84f, 0.88f) }
            };
        }

        private static string GetWeaponName(WeaponDefinition definition)
        {
            if (definition == null)
            {
                return "未装备";
            }

            return definition.Kind switch
            {
                WeaponKind.Sword => "长剑",
                WeaponKind.ChainBlade => "链刃",
                _ => definition.DisplayName
            };
        }

        private static string GetFightName(string fightLabel)
        {
            return fightLabel switch
            {
                "Mixed Opening" => "混合开场",
                "Melee Pressure" => "近战压力",
                "Final Pressure" => "最终围攻",
                "Final Siege" => "最终围攻",
                "FIGHT" => "战斗",
                null => string.Empty,
                _ => fightLabel
            };
        }

        private static void DrawBar(Rect rect, float normalized, Color fillColor)
        {
            Fill(rect, new Color(0.10f, 0.11f, 0.13f, 1f));
            Rect fill = rect;
            fill.width *= Mathf.Clamp01(normalized);
            Fill(fill, fillColor);
        }

        private static void GetBottomPanels(out Rect left, out Rect center, out Rect right)
        {
            const float margin = 18f;
            const float gap = 12f;
            const float height = 72f;
            float available = Mathf.Max(480f, Screen.width - margin * 2f - gap * 2f);
            float sideWidth = available * 0.28f;
            float centerWidth = available - sideWidth * 2f;
            float y = Screen.height - 100f;
            left = new Rect(margin, y, sideWidth, height);
            center = new Rect(left.xMax + gap, y, centerWidth, height);
            right = new Rect(center.xMax + gap, y, sideWidth, height);
        }

        private static void DrawModalBackdrop()
        {
            Fill(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.70f));
        }

        private static void Fill(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previousColor;
        }
    }
}
