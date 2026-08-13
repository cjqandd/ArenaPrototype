using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArenaPrototype.Art;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArenaPrototype.Editor
{
    [InitializeOnLoad]
    public static class ArenaProductionArtSetup
    {
        private const string GenericCharacterRoot = "Assets/ArenaPrototype/Prefabs/Characters";
        private const string RolePrefabRoot = GenericCharacterRoot + "/Roles";
        private const string AnimationRoot = "Assets/ArenaPrototype/Art/Production/Animation/KayKitAdventurers";
        private const string MaterialRoot = "Assets/ArenaPrototype/Art/Production/Materials/KayKitAdventurers";
        private const string ArtDataRoot = "Assets/ArenaPrototype/Data/Art";
        private const string CatalogPath = ArtDataRoot + "/ArenaArtCatalog.asset";
        private const string ControllerPath = AnimationRoot + "/AC_KayKit_Locomotion.controller";
        private const string GeneralAnimationPath = "Assets/ThirdParty/KayKit/Adventurers_2.0_FREE/Animations/fbx/Rig_Medium/Rig_Medium_General.fbx";
        private const string MovementAnimationPath = "Assets/ThirdParty/KayKit/Adventurers_2.0_FREE/Animations/fbx/Rig_Medium/Rig_Medium_MovementBasic.fbx";

        private static readonly (string source, string output)[] RolePrefabs =
        {
            ("PFB_Character_Barbarian_A", "PFB_Player_A"),
            ("PFB_Character_Rogue_A", "PFB_Enemy_Swordsman_A"),
            ("PFB_Character_Ranger_A", "PFB_Enemy_Archer_A"),
            ("PFB_Character_Knight_A", "PFB_Enemy_Shield_A")
        };

        private static bool running;
        private static int retries;

        static ArenaProductionArtSetup()
        {
            EditorApplication.delayCall += TryAutomaticSetup;
        }

        [MenuItem("Arena Prototype/美术资源/创建并应用 KayKit 角色预制体")]
        public static void RunFromMenu()
        {
            RunSetup(true);
        }

        public static void RunBatchMode()
        {
            RunSetup(false);
        }

        private static void TryAutomaticSetup()
        {
            if (HasSuccessfulReport())
            {
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode ||
                AssetDatabase.LoadAssetAtPath<GameObject>($"{GenericCharacterRoot}/PFB_Character_Barbarian_A.prefab") == null)
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
                EnsureFolder(RolePrefabRoot);
                EnsureFolder(AnimationRoot);
                EnsureFolder(ArtDataRoot);

                ConfigureEmissionMaterials();
                AnimatorController controller = CreateLocomotionController();
                Dictionary<string, GameObject> rolePrefabs = CreateRolePrefabs(controller);
                ArenaArtCatalog catalog = CreateCatalog(rolePrefabs);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                List<string> errors = ValidateAssets(catalog);
                if (errors.Count == 0)
                {
                    GrayboxSceneBuilder.BuildScene(catalog);
                    errors.AddRange(ValidateGeneratedScene());
                }

                WriteReport(errors);
                if (errors.Count == 0)
                {
                    Debug.Log("[Arena Production Art] KayKit 正式角色预制体已创建并替换进 GrayboxArena；玩法数值与命中判定未改变。");
                    if (showDialog)
                    {
                        EditorUtility.DisplayDialog("KayKit 角色替换完成", "四个角色身份预制体已创建并应用到灰盒场景。", "确定");
                    }
                }
                else
                {
                    string message = string.Join("\n", errors);
                    Debug.LogError($"[Arena Production Art] 验证失败：\n{message}");
                    if (showDialog)
                    {
                        EditorUtility.DisplayDialog("KayKit 角色替换需要检查", message, "确定");
                    }
                }
            }
            catch (Exception exception)
            {
                WriteReport(new[] { exception.ToString() });
                Debug.LogException(exception);
            }
            finally
            {
                running = false;
            }
        }

        private static AnimatorController CreateLocomotionController()
        {
            AnimatorController existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (existing != null)
            {
                return existing;
            }

            AnimationClip idle = LoadClip(GeneralAnimationPath, "Idle_A");
            AnimationClip walk = LoadClip(MovementAnimationPath, "Walking_A");
            AnimationClip run = LoadClip(MovementAnimationPath, "Running_A");

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState locomotion = stateMachine.AddState("Locomotion");
            stateMachine.defaultState = locomotion;

            var blendTree = new BlendTree
            {
                name = "LocomotionBlend",
                blendType = BlendTreeType.Simple1D,
                blendParameter = "Speed",
                useAutomaticThresholds = false
            };
            AssetDatabase.AddObjectToAsset(blendTree, controller);
            blendTree.AddChild(idle, 0f);
            blendTree.AddChild(walk, 2f);
            blendTree.AddChild(run, 5f);
            locomotion.motion = blendTree;
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static Dictionary<string, GameObject> CreateRolePrefabs(RuntimeAnimatorController controller)
        {
            var results = new Dictionary<string, GameObject>(StringComparer.Ordinal);
            foreach ((string sourceName, string outputName) in RolePrefabs)
            {
                GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>($"{GenericCharacterRoot}/{sourceName}.prefab");
                if (source == null)
                {
                    throw new InvalidOperationException($"找不到角色视觉预制体：{sourceName}");
                }

                GameObject root = new GameObject(outputName);
                try
                {
                    GameObject model = PrefabUtility.InstantiatePrefab(source) as GameObject;
                    if (model == null)
                    {
                        throw new InvalidOperationException($"无法实例化角色视觉预制体：{sourceName}");
                    }

                    model.name = "Model";
                    model.transform.SetParent(root.transform, false);
                    model.transform.localScale = Vector3.one * 0.82f;

                    Animator animator = model.GetComponentInChildren<Animator>(true);
                    if (animator == null)
                    {
                        throw new InvalidOperationException($"角色缺少 Animator：{sourceName}");
                    }

                    animator.runtimeAnimatorController = controller;
                    animator.applyRootMotion = false;
                    animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

                    Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
                    Transform[] transforms = model.GetComponentsInChildren<Transform>(true);
                    Transform leftHand = FindTransform(transforms, "handslot.l");
                    Transform rightHand = FindTransform(transforms, "handslot.r");

                    CharacterVisual visual = root.AddComponent<CharacterVisual>();
                    visual.Configure(renderers, animator, leftHand, rightHand);
                    CharacterLocomotionAnimator locomotion = root.AddComponent<CharacterLocomotionAnimator>();
                    locomotion.Configure(visual);

                    string path = $"{RolePrefabRoot}/{outputName}.prefab";
                    GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path, out bool success);
                    if (!success || prefab == null)
                    {
                        throw new InvalidOperationException($"无法保存角色身份预制体：{path}");
                    }

                    results[outputName] = prefab;
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }

            return results;
        }

        private static ArenaArtCatalog CreateCatalog(IReadOnlyDictionary<string, GameObject> prefabs)
        {
            ArenaArtCatalog catalog = AssetDatabase.LoadAssetAtPath<ArenaArtCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<ArenaArtCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            catalog.Configure(
                prefabs["PFB_Player_A"],
                prefabs["PFB_Enemy_Swordsman_A"],
                prefabs["PFB_Enemy_Archer_A"],
                prefabs["PFB_Enemy_Shield_A"]);
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static void ConfigureEmissionMaterials()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { MaterialRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    continue;
                }

                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                EditorUtility.SetDirty(material);
            }
        }

        private static List<string> ValidateAssets(ArenaArtCatalog catalog)
        {
            var errors = new List<string>();
            if (catalog == null || !catalog.IsComplete)
            {
                errors.Add("ArenaArtCatalog 未完整配置四个角色身份。");
                return errors;
            }

            foreach ((string _, string outputName) in RolePrefabs)
            {
                string path = $"{RolePrefabRoot}/{outputName}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                CharacterVisual visual = prefab != null ? prefab.GetComponent<CharacterVisual>() : null;
                if (visual == null || !visual.IsConfigured)
                {
                    errors.Add($"角色身份预制体未正确配置：{path}");
                    continue;
                }

                if (visual.LeftHandSocket == null || visual.RightHandSocket == null)
                {
                    errors.Add($"角色身份预制体缺少双手挂点：{path}");
                }

                if (visual.Animator.runtimeAnimatorController == null || visual.Animator.applyRootMotion)
                {
                    errors.Add($"角色身份预制体动画设置错误：{path}");
                }
            }

            return errors;
        }

        private static IEnumerable<string> ValidateGeneratedScene()
        {
            var errors = new List<string>();
            if (SceneManager.GetActiveScene().path != "Assets/ArenaPrototype/Scenes/GrayboxArena.unity")
            {
                errors.Add("生成后活动场景不是 GrayboxArena。");
                return errors;
            }

            CharacterVisual[] visuals = UnityEngine.Object.FindObjectsByType<CharacterVisual>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .Where(visual => visual.Animator != null)
                .ToArray();
            if (visuals.Length != 8)
            {
                errors.Add($"场景中的正式角色视觉数量错误：检测到 {visuals.Length}，预期 8。");
            }

            if (visuals.Any(visual => !visual.IsConfigured))
            {
                errors.Add("场景中存在未正确配置的正式角色视觉。");
            }

            return errors;
        }

        private static AnimationClip LoadClip(string path, string name)
        {
            AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .FirstOrDefault(candidate => candidate.name == name);
            return clip ?? throw new InvalidOperationException($"找不到动画片段：{name}（{path}）");
        }

        private static Transform FindTransform(IEnumerable<Transform> transforms, string name)
        {
            return transforms.FirstOrDefault(transform =>
                string.Equals(transform.name, name, StringComparison.OrdinalIgnoreCase));
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
            "Arena_KayKit_ROLE_REPLACEMENT_RESULT.txt"));

        private static bool HasSuccessfulReport()
        {
            return File.Exists(ReportPath) && File.ReadAllText(ReportPath).Contains("Status=Success");
        }

        private static void WriteReport(IEnumerable<string> errorSequence)
        {
            string[] errors = errorSequence.ToArray();
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? throw new InvalidOperationException());
            var lines = new List<string>
            {
                $"Status={(errors.Length == 0 ? "Success" : "Failed")}",
                $"UnityVersion={Application.unityVersion}",
                "ArtCatalog=Assets/ArenaPrototype/Data/Art/ArenaArtCatalog.asset",
                "Player=PFB_Player_A (Barbarian)",
                "Swordsman=PFB_Enemy_Swordsman_A (Rogue)",
                "Archer=PFB_Enemy_Archer_A (Ranger)",
                "ShieldBearer=PFB_Enemy_Shield_A (Knight)",
                "Scene=Assets/ArenaPrototype/Scenes/GrayboxArena.unity",
                "ProductionCharacterInstances=8",
                "RootMotion=False",
                "Locomotion=Idle_A + Walking_A + Running_A",
                "GameplayValuesChanged=False",
                "HitDetectionChanged=False",
                "WeaponsReplaced=False"
            };
            if (errors.Length > 0)
            {
                lines.Add("Errors:");
                lines.AddRange(errors);
            }

            File.WriteAllLines(ReportPath, lines);
        }
    }
}
