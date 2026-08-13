using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArenaPrototype.Editor
{
    [InitializeOnLoad]
    public static class KayKitAdventurersAssetSetup
    {
        private const string ThirdPartyRoot = "Assets/ThirdParty/KayKit/Adventurers_2.0_FREE";
        private const string CharacterSourceRoot = ThirdPartyRoot + "/Characters/fbx";
        private const string WeaponSourceRoot = ThirdPartyRoot + "/Assets/fbx(unity)";
        private const string AnimationSourceRoot = ThirdPartyRoot + "/Animations/fbx/Rig_Medium";
        private const string MaterialRoot = "Assets/ArenaPrototype/Art/Production/Materials/KayKitAdventurers";
        private const string CharacterPrefabRoot = "Assets/ArenaPrototype/Prefabs/Characters";
        private const string WeaponPrefabRoot = "Assets/ArenaPrototype/Prefabs/Weapons";

        private static readonly string[] CharacterNames =
        {
            "Barbarian",
            "Knight",
            "Mage",
            "Ranger",
            "Rogue",
            "Rogue_Hooded"
        };

        private static readonly (string source, string prefab, string material)[] CharacterPrefabs =
        {
            ("Barbarian", "PFB_Character_Barbarian_A", "barbarian"),
            ("Knight", "PFB_Character_Knight_A", "knight"),
            ("Mage", "PFB_Character_Mage_A", "mage"),
            ("Ranger", "PFB_Character_Ranger_A", "ranger"),
            ("Rogue", "PFB_Character_Rogue_A", "rogue"),
            ("Rogue_Hooded", "PFB_Character_RogueHooded_A", "rogue")
        };

        private static readonly (string source, string prefab, string material)[] WeaponPrefabs =
        {
            ("sword_1handed", "PFB_Weapon_Sword_1H_A", "knight"),
            ("sword_2handed", "PFB_Weapon_Sword_2H_A", "knight"),
            ("shield_round", "PFB_Weapon_Shield_Round_A", "knight"),
            ("bow", "PFB_Weapon_Bow_A", "ranger"),
            ("arrow_bow", "PFB_Weapon_Arrow_A", "ranger")
        };

        private static bool running;
        private static int retryCount;

        static KayKitAdventurersAssetSetup()
        {
            EditorApplication.delayCall += TryAutomaticSetup;
        }

        [MenuItem("Arena Prototype/美术资源/设置 KayKit Adventurers")]
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
                AssetDatabase.LoadAssetAtPath<GameObject>($"{CharacterSourceRoot}/Barbarian.fbx") == null)
            {
                if (retryCount++ < 120)
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
                ConfigureCharacterModels();
                Avatar referenceAvatar = LoadAvatar($"{CharacterSourceRoot}/Barbarian.fbx");
                ConfigureAnimationModels(referenceAvatar);
                ConfigureStaticModels();
                ConfigureTextures();

                EnsureFolder(MaterialRoot);
                EnsureFolder(CharacterPrefabRoot);
                EnsureFolder(WeaponPrefabRoot);

                Dictionary<string, Material> materials = CreateMaterials();
                CreateCharacterPrefabs(materials);
                CreateWeaponPrefabs(materials);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                List<string> errors = ValidateImport();
                WriteReport(errors);

                if (errors.Count == 0)
                {
                    Debug.Log("[KayKit Import] Adventurers 2.0 FREE 已完成资源导入、URP 材质和视觉预制体设置；未接入玩法或场景。");
                    if (showDialog)
                    {
                        EditorUtility.DisplayDialog("KayKit 资源设置完成", "资源、URP 材质和视觉预制体已准备好。没有修改玩法或场景。", "确定");
                    }
                }
                else
                {
                    string message = string.Join("\n", errors);
                    Debug.LogError($"[KayKit Import] 验证未通过：\n{message}");
                    if (showDialog)
                    {
                        EditorUtility.DisplayDialog("KayKit 资源需要检查", message, "确定");
                    }
                }
            }
            catch (Exception exception)
            {
                WriteReport(new List<string> { exception.ToString() });
                Debug.LogException(exception);
            }
            finally
            {
                running = false;
            }
        }

        private static void ConfigureCharacterModels()
        {
            foreach (string characterName in CharacterNames)
            {
                string path = $"{CharacterSourceRoot}/{characterName}.fbx";
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null)
                {
                    throw new InvalidOperationException($"找不到角色模型：{path}");
                }

                importer.globalScale = 1f;
                importer.meshCompression = ModelImporterMeshCompression.Off;
                importer.isReadable = false;
                importer.addCollider = false;
                importer.importAnimation = false;
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importCameras = false;
                importer.importLights = false;
                importer.optimizeGameObjects = false;
                importer.preserveHierarchy = true;
                importer.SaveAndReimport();
            }
        }

        private static void ConfigureAnimationModels(Avatar referenceAvatar)
        {
            if (referenceAvatar == null || !referenceAvatar.isValid || !referenceAvatar.isHuman)
            {
                throw new InvalidOperationException("Barbarian 的 Unity Humanoid Avatar 未能正确生成。");
            }

            foreach (string path in new[]
                     {
                         $"{AnimationSourceRoot}/Rig_Medium_General.fbx",
                         $"{AnimationSourceRoot}/Rig_Medium_MovementBasic.fbx"
                     })
            {
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null)
                {
                    throw new InvalidOperationException($"找不到动画模型：{path}");
                }

                importer.globalScale = 1f;
                importer.meshCompression = ModelImporterMeshCompression.Off;
                importer.isReadable = false;
                importer.addCollider = false;
                importer.importAnimation = true;
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                importer.sourceAvatar = referenceAvatar;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importCameras = false;
                importer.importLights = false;
                importer.optimizeGameObjects = false;
                importer.preserveHierarchy = true;
                importer.SaveAndReimport();

                ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
                foreach (ModelImporterClipAnimation clip in clips)
                {
                    bool shouldLoop = clip.name.Contains("Idle", StringComparison.OrdinalIgnoreCase) ||
                                      clip.name.Contains("Walking", StringComparison.OrdinalIgnoreCase) ||
                                      clip.name.Contains("Running", StringComparison.OrdinalIgnoreCase);
                    clip.loopTime = shouldLoop;
                    clip.loopPose = shouldLoop;
                    clip.keepOriginalOrientation = true;
                    clip.keepOriginalPositionY = true;
                    clip.keepOriginalPositionXZ = true;
                    clip.lockRootRotation = true;
                    clip.lockRootHeightY = true;
                    clip.lockRootPositionXZ = true;
                }

                importer.clipAnimations = clips;
                importer.SaveAndReimport();
            }
        }

        private static void ConfigureStaticModels()
        {
            string absoluteFolder = ToAbsolutePath(WeaponSourceRoot);
            foreach (string fullPath in Directory.GetFiles(absoluteFolder, "*.fbx", SearchOption.TopDirectoryOnly))
            {
                string path = ToAssetPath(fullPath);
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null)
                {
                    throw new InvalidOperationException($"找不到静态模型：{path}");
                }

                importer.globalScale = 1f;
                importer.meshCompression = ModelImporterMeshCompression.Off;
                importer.isReadable = false;
                importer.addCollider = false;
                importer.importAnimation = false;
                importer.animationType = ModelImporterAnimationType.None;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importCameras = false;
                importer.importLights = false;
                importer.SaveAndReimport();
            }
        }

        private static void ConfigureTextures()
        {
            foreach (string root in new[] { CharacterSourceRoot, WeaponSourceRoot })
            {
                string absoluteFolder = ToAbsolutePath(root);
                foreach (string fullPath in Directory.GetFiles(absoluteFolder, "*.png", SearchOption.TopDirectoryOnly))
                {
                    string path = ToAssetPath(fullPath);
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null)
                    {
                        throw new InvalidOperationException($"找不到贴图：{path}");
                    }

                    importer.textureType = TextureImporterType.Default;
                    importer.sRGBTexture = true;
                    importer.alphaSource = TextureImporterAlphaSource.None;
                    importer.maxTextureSize = 1024;
                    importer.textureCompression = TextureImporterCompression.Compressed;
                    importer.SaveAndReimport();
                }
            }
        }

        private static Dictionary<string, Material> CreateMaterials()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                throw new InvalidOperationException("找不到 Universal Render Pipeline/Lit Shader。");
            }

            var materials = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
            foreach (string name in new[] { "barbarian", "knight", "mage", "ranger", "rogue" })
            {
                string materialPath = $"{MaterialRoot}/MAT_KayKit_{ToTitleCase(name)}_A.mat";
                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null)
                {
                    material = new Material(shader);
                    AssetDatabase.CreateAsset(material, materialPath);
                }
                else
                {
                    material.shader = shader;
                }

                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{CharacterSourceRoot}/{name}_texture.png");
                if (texture == null)
                {
                    throw new InvalidOperationException($"找不到角色贴图：{name}_texture.png");
                }

                material.SetTexture("_BaseMap", texture);
                material.SetColor("_BaseColor", Color.white);
                material.SetFloat("_Metallic", 0f);
                material.SetFloat("_Smoothness", 0.15f);
                material.SetFloat("_Cull", 0f);
                material.doubleSidedGI = true;
                material.enableInstancing = true;
                EditorUtility.SetDirty(material);
                materials[name] = material;
            }

            return materials;
        }

        private static void CreateCharacterPrefabs(IReadOnlyDictionary<string, Material> materials)
        {
            foreach ((string source, string prefab, string material) in CharacterPrefabs)
            {
                CreateVisualPrefab(
                    $"{CharacterSourceRoot}/{source}.fbx",
                    $"{CharacterPrefabRoot}/{prefab}.prefab",
                    prefab,
                    materials[material],
                    true);
            }
        }

        private static void CreateWeaponPrefabs(IReadOnlyDictionary<string, Material> materials)
        {
            foreach ((string source, string prefab, string material) in WeaponPrefabs)
            {
                CreateVisualPrefab(
                    $"{WeaponSourceRoot}/{source}.fbx",
                    $"{WeaponPrefabRoot}/{prefab}.prefab",
                    prefab,
                    materials[material],
                    false);
            }
        }

        private static void CreateVisualPrefab(
            string sourcePath,
            string prefabPath,
            string rootName,
            Material material,
            bool disableRootMotion)
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
            if (source == null)
            {
                throw new InvalidOperationException($"找不到预制体源模型：{sourcePath}");
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException($"无法实例化源模型：{sourcePath}");
            }

            try
            {
                instance.name = rootName;
                instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                instance.transform.localScale = Vector3.one;

                foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
                {
                    Material[] assigned = renderer.sharedMaterials;
                    for (int i = 0; i < assigned.Length; i++)
                    {
                        assigned[i] = material;
                    }

                    renderer.sharedMaterials = assigned;
                }

                if (disableRootMotion)
                {
                    foreach (Animator animator in instance.GetComponentsInChildren<Animator>(true))
                    {
                        animator.applyRootMotion = false;
                        animator.runtimeAnimatorController = null;
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath, out bool success);
                if (!success)
                {
                    throw new InvalidOperationException($"无法保存视觉预制体：{prefabPath}");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static List<string> ValidateImport()
        {
            var errors = new List<string>();
            foreach (string characterName in CharacterNames)
            {
                string path = $"{CharacterSourceRoot}/{characterName}.fbx";
                Avatar avatar = LoadAvatar(path);
                if (avatar == null || !avatar.isValid || !avatar.isHuman)
                {
                    errors.Add($"Humanoid Avatar 无效：{characterName}");
                }

                GameObject character = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (character == null || !HasTransform(character, "handslot.l") || !HasTransform(character, "handslot.r"))
                {
                    errors.Add($"左右手挂点不完整：{characterName}");
                }
            }

            foreach ((string _, string prefab, string _) in CharacterPrefabs)
            {
                ValidatePrefab($"{CharacterPrefabRoot}/{prefab}.prefab", errors);
            }

            foreach ((string _, string prefab, string _) in WeaponPrefabs)
            {
                ValidatePrefab($"{WeaponPrefabRoot}/{prefab}.prefab", errors);
            }

            int generalClips = CountAnimationClips($"{AnimationSourceRoot}/Rig_Medium_General.fbx");
            int movementClips = CountAnimationClips($"{AnimationSourceRoot}/Rig_Medium_MovementBasic.fbx");
            if (generalClips < 15)
            {
                errors.Add($"通用动画数量不足：检测到 {generalClips}，预期至少 15");
            }

            if (movementClips < 11)
            {
                errors.Add($"基础移动动画数量不足：检测到 {movementClips}，预期至少 11");
            }

            ValidateAnimationCurves($"{AnimationSourceRoot}/Rig_Medium_General.fbx", errors);
            ValidateAnimationCurves($"{AnimationSourceRoot}/Rig_Medium_MovementBasic.fbx", errors);

            return errors;
        }

        private static void ValidateAnimationCurves(string path, ICollection<string> errors)
        {
            foreach (AnimationClip clip in AssetDatabase.LoadAllAssetsAtPath(path)
                         .OfType<AnimationClip>()
                         .Where(clip => !clip.name.StartsWith("__preview__", StringComparison.Ordinal)))
            {
                foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
                {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                    if (curve == null)
                    {
                        continue;
                    }

                    foreach (Keyframe key in curve.keys)
                    {
                        if (!IsFinite(key.time) || !IsFinite(key.value))
                        {
                            errors.Add($"动画含无效关键帧：{clip.name} / {binding.path} / {binding.propertyName}");
                            return;
                        }
                    }
                }
            }
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static void ValidatePrefab(string path, ICollection<string> errors)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                errors.Add($"缺少视觉预制体：{path}");
                return;
            }

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                errors.Add($"视觉预制体没有 Renderer：{path}");
            }

            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material == null || material.shader == null || material.shader.name != "Universal Render Pipeline/Lit")
                    {
                        errors.Add($"视觉预制体材质不是 URP/Lit：{path}");
                        return;
                    }
                }
            }
        }

        private static Avatar LoadAvatar(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Avatar>().FirstOrDefault();
        }

        private static bool HasTransform(GameObject root, string name)
        {
            return root.GetComponentsInChildren<Transform>(true)
                .Any(transform => string.Equals(transform.name, name, StringComparison.OrdinalIgnoreCase));
        }

        private static int CountAnimationClips(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .Count(clip => !clip.name.StartsWith("__preview__", StringComparison.Ordinal));
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

        private static string ToAbsolutePath(string assetPath)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(Path.Combine(projectRoot, assetPath));
        }

        private static string ToAssetPath(string fullPath)
        {
            string normalized = Path.GetFullPath(fullPath).Replace('\\', '/');
            string dataPath = Path.GetFullPath(Application.dataPath).Replace('\\', '/');
            return "Assets" + normalized.Substring(dataPath.Length);
        }

        private static string ToTitleCase(string value)
        {
            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        private static string ReportPath => Path.GetFullPath(Path.Combine(
            Application.dataPath,
            "..",
            "ExternalAssets",
            "Validation",
            "KayKit_Adventurers_2.0_FREE_IMPORT_RESULT.txt"));

        private static bool HasSuccessfulReport()
        {
            return File.Exists(ReportPath) && File.ReadAllText(ReportPath).Contains("Status=Success");
        }

        private static void WriteReport(IReadOnlyCollection<string> errors)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? throw new InvalidOperationException());
            int generalClips = CountAnimationClips($"{AnimationSourceRoot}/Rig_Medium_General.fbx");
            int movementClips = CountAnimationClips($"{AnimationSourceRoot}/Rig_Medium_MovementBasic.fbx");
            var lines = new List<string>
            {
                $"Status={(errors.Count == 0 ? "Success" : "Failed")}",
                $"UnityVersion={Application.unityVersion}",
                "RenderPipeline=Universal Render Pipeline",
                $"CharacterModels={CharacterNames.Length}",
                $"CharacterPrefabs={CharacterPrefabs.Length}",
                $"WeaponPrefabs={WeaponPrefabs.Length}",
                $"GeneralAnimationClips={generalClips}",
                $"MovementAnimationClips={movementClips}",
                "RootMotion=False",
                "GameplayIntegration=False",
                "SceneChanges=False"
            };

            if (errors.Count > 0)
            {
                lines.Add("Errors:");
                lines.AddRange(errors);
            }

            File.WriteAllLines(ReportPath, lines);
        }
    }
}
