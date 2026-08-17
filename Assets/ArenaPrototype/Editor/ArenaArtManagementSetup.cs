using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArenaPrototype.Art;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArenaPrototype.Editor
{
    [InitializeOnLoad]
    public static class ArenaArtManagementSetup
    {
        public const string CatalogPath = "Assets/ArenaPrototype/Data/Art/ArenaArtCatalog.asset";
        public const string GrayboxSetPath = "Assets/ArenaPrototype/Data/Art/Sets/ARTSET_Graybox.asset";
        public const string KayKitSetPath = "Assets/ArenaPrototype/Data/Art/Sets/ARTSET_KayKit_Adventurers.asset";
        public const string EquipmentProfileRoot = "Assets/ArenaPrototype/Data/Art/Profiles/Equipment";

        private const string RolePrefabRoot = "Assets/ArenaPrototype/Prefabs/Characters/Roles";
        private const string WeaponPrefabRoot = "Assets/ArenaPrototype/Prefabs/Weapons";
        private const string ScenePath = "Assets/ArenaPrototype/Scenes/GrayboxArena.unity";

        private static bool running;
        private static int retries;

        static ArenaArtManagementSetup()
        {
            EditorApplication.delayCall += TryAutomaticSetup;
        }

        [MenuItem("Arena Prototype/美术资源/初始化美术套装")]
        public static void RunFromMenu()
        {
            RunSetup(true);
        }

        public static void RunBatchMode()
        {
            RunSetup(false);
        }

        public static void ApplyActiveSetToScene(bool showDialog = true)
        {
            ArenaArtCatalog catalog = AssetDatabase.LoadAssetAtPath<ArenaArtCatalog>(CatalogPath);
            if (catalog == null)
            {
                if (showDialog)
                {
                    EditorUtility.DisplayDialog("无法应用美术套装", "找不到 ArenaArtCatalog。请先初始化美术套装。", "确定");
                }
                return;
            }

            GrayboxSceneBuilder.BuildScene(catalog);
            List<string> errors = ValidateScene(catalog);
            WriteReport(errors, GetCatalogWarnings(catalog));
            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    errors.Count == 0 ? "美术套装已应用" : "美术套装需要检查",
                    errors.Count == 0
                        ? $"已应用：{catalog.ActiveArtSet?.DisplayName ?? "旧版目录"}\n链刃缺少正式模型时会继续使用灰盒。"
                        : string.Join("\n", errors),
                    "确定");
            }
        }

        public static void SwitchActiveSet(ArenaArtSet artSet, bool applyToScene)
        {
            ArenaArtCatalog catalog = AssetDatabase.LoadAssetAtPath<ArenaArtCatalog>(CatalogPath);
            ArenaArtSet graybox = AssetDatabase.LoadAssetAtPath<ArenaArtSet>(GrayboxSetPath);
            if (catalog == null || artSet == null)
            {
                return;
            }

            Undo.RecordObject(catalog, "切换美术套装");
            catalog.ConfigureArtSets(artSet, graybox);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            if (applyToScene)
            {
                ApplyActiveSetToScene(false);
            }
        }

        public static EquipmentVisualProfile CreateUserEquipmentProfile(
            GameObject visualPrefab,
            ArenaEquipmentRole role)
        {
            if (visualPrefab == null)
            {
                return null;
            }

            EnsureFolder(EquipmentProfileRoot);
            string safeName = new string(visualPrefab.name
                .Where(character => char.IsLetterOrDigit(character) || character == '_')
                .ToArray());
            string path = AssetDatabase.GenerateUniqueAssetPath(
                $"{EquipmentProfileRoot}/WVIS_{safeName}_{role}.asset");
            EquipmentVisualProfile profile = ScriptableObject.CreateInstance<EquipmentVisualProfile>();
            ConfigureProfileDefaults(profile, role, visualPrefab);
            AssetDatabase.CreateAsset(profile, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = profile;
            EditorGUIUtility.PingObject(profile);
            return profile;
        }

        public static List<string> ValidateCatalog(ArenaArtCatalog catalog)
        {
            var errors = new List<string>();
            if (catalog == null)
            {
                errors.Add("缺少 ArenaArtCatalog。");
                return errors;
            }

            if (catalog.ActiveArtSet == null)
            {
                errors.Add("尚未选择当前美术套装。");
                return errors;
            }

            if (!catalog.ActiveArtSet.ProceduralGraybox)
            {
                foreach (ArenaCharacterRole role in Enum.GetValues(typeof(ArenaCharacterRole)))
                {
                    GameObject prefab = catalog.GetCharacterPrefab(role);
                    CharacterVisual visual = prefab != null ? prefab.GetComponent<CharacterVisual>() : null;
                    if (visual == null || !visual.IsConfigured)
                    {
                        errors.Add($"角色槽 {role} 缺少正确配置的身份预制体。");
                    }
                }

                foreach (ArenaEquipmentRole role in new[]
                {
                    ArenaEquipmentRole.Sword,
                    ArenaEquipmentRole.Bow,
                    ArenaEquipmentRole.Arrow,
                    ArenaEquipmentRole.Shield
                })
                {
                    EquipmentVisualProfile profile = catalog.GetEquipmentProfile(role);
                    if (profile == null || !profile.IsConfigured)
                    {
                        errors.Add($"装备槽 {role} 缺少视觉配置。");
                    }
                }
            }

            return errors;
        }

        public static List<string> GetCatalogWarnings(ArenaArtCatalog catalog)
        {
            var warnings = new List<string>();
            if (catalog != null
                && catalog.ActiveArtSet != null
                && !catalog.ActiveArtSet.ProceduralGraybox
                && catalog.GetEquipmentProfile(ArenaEquipmentRole.ChainBlade) == null)
            {
                warnings.Add("链刃没有正式模型，将使用灰盒后备外观。");
            }

            return warnings;
        }

        private static void TryAutomaticSetup()
        {
            if (HasSuccessfulReport())
            {
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating
                || EditorApplication.isPlayingOrWillChangePlaymode
                || AssetDatabase.LoadAssetAtPath<GameObject>(
                    $"{RolePrefabRoot}/PFB_Player_A.prefab") == null)
            {
                if (retries++ < 180)
                {
                    EditorApplication.delayCall += TryAutomaticSetup;
                }
                return;
            }

            RunSetup(false);
        }

        private static void RunSetup(bool showDialog)
        {
            if (running)
            {
                return;
            }

            running = true;
            try
            {
                EnsureFolder("Assets/ArenaPrototype/Data/Art/Sets");
                EnsureFolder(EquipmentProfileRoot);

                ArenaArtSet graybox = CreateGrayboxSet();
                EquipmentVisualProfile sword = CreateEquipmentProfile(
                    "WVIS_KayKit_Sword_1H",
                    ArenaEquipmentRole.Sword,
                    $"{WeaponPrefabRoot}/PFB_Weapon_Sword_1H_A.prefab");
                EquipmentVisualProfile bow = CreateEquipmentProfile(
                    "WVIS_KayKit_Bow",
                    ArenaEquipmentRole.Bow,
                    $"{WeaponPrefabRoot}/PFB_Weapon_Bow_A.prefab");
                EquipmentVisualProfile arrow = CreateEquipmentProfile(
                    "WVIS_KayKit_Arrow",
                    ArenaEquipmentRole.Arrow,
                    $"{WeaponPrefabRoot}/PFB_Weapon_Arrow_A.prefab");
                EquipmentVisualProfile shield = CreateEquipmentProfile(
                    "WVIS_KayKit_Shield_Round",
                    ArenaEquipmentRole.Shield,
                    $"{WeaponPrefabRoot}/PFB_Weapon_Shield_Round_A.prefab");
                ArenaArtSet kayKit = CreateKayKitSet(sword, bow, arrow, shield);

                ArenaArtCatalog catalog = AssetDatabase.LoadAssetAtPath<ArenaArtCatalog>(CatalogPath);
                if (catalog == null)
                {
                    throw new InvalidOperationException("找不到 ArenaArtCatalog。");
                }

                catalog.ConfigureArtSets(kayKit, graybox);
                EditorUtility.SetDirty(catalog);
                AssetDatabase.SaveAssets();

                List<string> errors = ValidateCatalog(catalog);
                if (errors.Count == 0)
                {
                    GrayboxSceneBuilder.BuildScene(catalog);
                    errors.AddRange(ValidateScene(catalog));
                }

                List<string> warnings = GetCatalogWarnings(catalog);
                WriteReport(errors, warnings);
                if (showDialog)
                {
                    EditorUtility.DisplayDialog(
                        errors.Count == 0 ? "美术资源管理器初始化完成" : "美术资源管理器需要检查",
                        errors.Count == 0
                            ? "KayKit 角色、剑、弓、箭和盾已加入美术套装。链刃继续使用灰盒。"
                            : string.Join("\n", errors),
                        "确定");
                }
            }
            catch (Exception exception)
            {
                WriteReport(new List<string> { exception.ToString() }, new List<string>());
                Debug.LogException(exception);
            }
            finally
            {
                running = false;
            }
        }

        private static ArenaArtSet CreateGrayboxSet()
        {
            ArenaArtSet set = LoadOrCreateAsset<ArenaArtSet>(GrayboxSetPath);
            set.Configure(
                "Graybox 灰盒",
                true,
                null, null, null, null,
                null, null, null, null, null);
            EditorUtility.SetDirty(set);
            return set;
        }

        private static ArenaArtSet CreateKayKitSet(
            EquipmentVisualProfile sword,
            EquipmentVisualProfile bow,
            EquipmentVisualProfile arrow,
            EquipmentVisualProfile shield)
        {
            ArenaArtSet set = LoadOrCreateAsset<ArenaArtSet>(KayKitSetPath);
            set.Configure(
                "KayKit Adventurers",
                false,
                AssetDatabase.LoadAssetAtPath<GameObject>($"{RolePrefabRoot}/PFB_Player_A.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>($"{RolePrefabRoot}/PFB_Enemy_Swordsman_A.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>($"{RolePrefabRoot}/PFB_Enemy_Archer_A.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>($"{RolePrefabRoot}/PFB_Enemy_Shield_A.prefab"),
                sword,
                null,
                bow,
                arrow,
                shield);
            EditorUtility.SetDirty(set);
            return set;
        }

        private static EquipmentVisualProfile CreateEquipmentProfile(
            string assetName,
            ArenaEquipmentRole role,
            string prefabPath)
        {
            string path = $"{EquipmentProfileRoot}/{assetName}.asset";
            EquipmentVisualProfile profile = LoadOrCreateAsset<EquipmentVisualProfile>(path);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException($"找不到装备视觉预制体：{prefabPath}");
            }

            ConfigureProfileDefaults(profile, role, prefab);
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void ConfigureProfileDefaults(
            EquipmentVisualProfile profile,
            ArenaEquipmentRole role,
            GameObject prefab)
        {
            Vector3 equippedPosition = role == ArenaEquipmentRole.Arrow
                ? new Vector3(0f, 0f, 0.38f)
                : Vector3.zero;
            Vector3 groundedEuler = role == ArenaEquipmentRole.Sword
                ? new Vector3(0f, 90f, 0f)
                : Vector3.zero;
            Vector3 colliderSize = role == ArenaEquipmentRole.ChainBlade
                ? new Vector3(0.45f, 0.30f, 2.65f)
                : new Vector3(0.35f, 0.30f, 1.60f);
            Vector3 colliderCenter = role == ArenaEquipmentRole.ChainBlade
                ? new Vector3(0f, 0f, 1.15f)
                : Vector3.zero;

            profile.Configure(
                role,
                prefab,
                Vector3.one,
                equippedPosition,
                Vector3.zero,
                Vector3.zero,
                groundedEuler,
                colliderCenter,
                colliderSize);
        }

        private static List<string> ValidateScene(ArenaArtCatalog catalog)
        {
            var errors = new List<string>();
            if (SceneManager.GetActiveScene().path != ScenePath)
            {
                errors.Add("当前活动场景不是 GrayboxArena。");
                return errors;
            }

            CharacterVisual[] characters = UnityEngine.Object.FindObjectsByType<CharacterVisual>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            int animatedCharacters = characters.Count(character => character.Animator != null);
            if (!catalog.ActiveArtSet.ProceduralGraybox && animatedCharacters != 8)
            {
                errors.Add($"正式角色数量错误：检测到 {animatedCharacters}，预期 8。");
            }

            EquipmentVisual[] visuals = UnityEngine.Object.FindObjectsByType<EquipmentVisual>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            if (!catalog.ActiveArtSet.ProceduralGraybox)
            {
                ValidateEquipmentCount(visuals, ArenaEquipmentRole.Sword, 6, errors);
                ValidateEquipmentCount(visuals, ArenaEquipmentRole.Bow, 2, errors);
                ValidateEquipmentCount(visuals, ArenaEquipmentRole.Arrow, 2, errors);
                ValidateEquipmentCount(visuals, ArenaEquipmentRole.Shield, 2, errors);
            }

            return errors;
        }

        private static void ValidateEquipmentCount(
            EquipmentVisual[] visuals,
            ArenaEquipmentRole role,
            int expected,
            ICollection<string> errors)
        {
            int count = visuals.Count(visual => visual.Role == role && !visual.IsProceduralGraybox);
            if (count != expected)
            {
                errors.Add($"{role} 视觉数量错误：检测到 {count}，预期 {expected}。");
            }
        }

        private static T LoadOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }

        private static string ReportPath => Path.GetFullPath(Path.Combine(
            Application.dataPath,
            "..",
            "ExternalAssets",
            "Validation",
            "Arena_ART_SET_MANAGEMENT_RESULT.txt"));

        private static bool HasSuccessfulReport()
        {
            return File.Exists(ReportPath)
                && File.ReadAllText(ReportPath).Contains("Status=Success");
        }

        private static void WriteReport(
            IReadOnlyCollection<string> errors,
            IReadOnlyCollection<string> warnings)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath)
                ?? throw new InvalidOperationException());
            var lines = new List<string>
            {
                $"Status={(errors.Count == 0 ? "Success" : "Failed")}",
                $"UnityVersion={Application.unityVersion}",
                $"Catalog={CatalogPath}",
                $"ActiveSet={AssetDatabase.GetAssetPath(AssetDatabase.LoadAssetAtPath<ArenaArtCatalog>(CatalogPath)?.ActiveArtSet)}",
                $"FallbackSet={GrayboxSetPath}",
                "Characters=Player,Swordsman,Archer,ShieldBearer",
                "Equipment=Sword,Bow,Arrow,Shield",
                "ChainBladeFallback=Graybox",
                "GameplayValuesChanged=False",
                "HitDetectionChanged=False"
            };
            if (warnings.Count > 0)
            {
                lines.Add("Warnings:");
                lines.AddRange(warnings);
            }
            if (errors.Count > 0)
            {
                lines.Add("Errors:");
                lines.AddRange(errors);
            }
            File.WriteAllLines(ReportPath, lines);
        }
    }
}
