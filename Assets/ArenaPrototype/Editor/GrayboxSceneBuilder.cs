using System.IO;
using ArenaPrototype.CameraSystem;
using ArenaPrototype.Combat;
using ArenaPrototype.Player;
using ArenaPrototype.UI;
using ArenaPrototype.Movement;
using ArenaPrototype.Enemies;
using ArenaPrototype.Environment;
using ArenaPrototype.Weapons;
using ArenaPrototype.Art;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ArenaPrototype.Editor
{
    public static class GrayboxSceneBuilder
    {
        private const string SceneFolder = "Assets/ArenaPrototype/Scenes";
        private const string MaterialFolder = "Assets/ArenaPrototype/Art/GeneratedMaterials";
        private const string WeaponDataFolder = "Assets/ArenaPrototype/Data/Weapons";
        private const string CombatDataFolder = "Assets/ArenaPrototype/Data/Combat";
        private const string EnemyDataFolder = "Assets/ArenaPrototype/Data/Enemies";
        private const string EnvironmentDataFolder = "Assets/ArenaPrototype/Data/Environment";
        private const string ArtDataFolder = "Assets/ArenaPrototype/Data/Art";
        private const string ArtCatalogPath = ArtDataFolder + "/ArenaArtCatalog.asset";
        private const string RoleCharacterPrefabFolder = "Assets/ArenaPrototype/Prefabs/Characters/Roles";
        private const string ScenePath = SceneFolder + "/GrayboxArena.unity";

        [MenuItem("Arena Prototype/创建第一版灰盒场景 %#g")]
        public static void BuildScene()
        {
            ArenaArtCatalog artCatalog = AssetDatabase.LoadAssetAtPath<ArenaArtCatalog>(ArtCatalogPath);
            BuildScene(artCatalog);
        }

        public static void BuildScene(ArenaArtCatalog artCatalog)
        {
            EnsureFolder(SceneFolder);
            EnsureFolder(MaterialFolder);
            EnsureFolder(WeaponDataFolder);
            EnsureFolder(CombatDataFolder);
            EnsureFolder(EnemyDataFolder);
            EnsureFolder(EnvironmentDataFolder);
            EnsureFolder(ArtDataFolder);

            Material floorMaterial = GetOrCreateMaterial("Floor", new Color(0.27f, 0.30f, 0.34f));
            Material wallMaterial = GetOrCreateMaterial("Wall", new Color(0.16f, 0.18f, 0.21f));
            Material playerMaterial = GetOrCreateMaterial("Player", new Color(0.10f, 0.42f, 0.90f));
            Material facingMaterial = GetOrCreateMaterial("Facing", new Color(1.00f, 0.70f, 0.08f));
            Material weaponMaterial = GetOrCreateMaterial("Sword", new Color(0.75f, 0.78f, 0.82f));
            Material chainBladeMaterial = GetOrCreateMaterial("ChainBlade", new Color(0.28f, 0.75f, 0.82f));
            Material dummyMaterial = GetOrCreateMaterial("TrainingDummy", new Color(0.78f, 0.20f, 0.16f));
            Material swordEnemyMaterial = GetOrCreateMaterial("SwordEnemy", new Color(0.52f, 0.10f, 0.62f));
            Material shieldMaterial = GetOrCreateMaterial("Shield", new Color(0.30f, 0.36f, 0.42f));
            Material archerEnemyMaterial = GetOrCreateMaterial("ArcherEnemy", new Color(0.05f, 0.52f, 0.40f));
            Material archerWeaponMaterial = GetOrCreateMaterial("ArcherWeapon", new Color(0.46f, 0.25f, 0.10f));
            Material spikeMaterial = GetOrCreateMaterial("Spikes", new Color(0.72f, 0.04f, 0.04f));
            Material rangeFillMaterial = GetOrCreateTransparentMaterial(
                "AttackRangeFill",
                new Color(0.05f, 0.75f, 1f, 0.18f));
            Material rangeOutlineMaterial = GetOrCreateTransparentMaterial(
                "AttackRangeOutline",
                new Color(0.05f, 0.75f, 1f, 0.85f));
            Material kickFillMaterial = GetOrCreateTransparentMaterial(
                "KickRangeFill",
                new Color(1f, 0.82f, 0.08f, 0.14f));
            Material kickOutlineMaterial = GetOrCreateTransparentMaterial(
                "KickRangeOutline",
                new Color(1f, 0.82f, 0.08f, 0.72f));
            Material barBackgroundMaterial = GetOrCreateUnlitMaterial("BarBackground", new Color(0.05f, 0.05f, 0.05f));
            Material healthBarMaterial = GetOrCreateUnlitMaterial("HealthBar", new Color(0.10f, 0.85f, 0.20f));
            Material postureBarMaterial = GetOrCreateUnlitMaterial("PostureBar", new Color(1f, 0.75f, 0.05f));
            Material pickupIndicatorMaterial = GetOrCreateUnlitMaterial(
                "WeaponPickupIndicator",
                new Color(1f, 0.82f, 0.06f));
            Material archerAimMaterial = GetOrCreateUnlitMaterial(
                "ArcherAimTelegraph",
                new Color(1f, 0.20f, 0.05f));
            Material projectileMaterial = GetOrCreateUnlitMaterial(
                "ArcherProjectile",
                new Color(1f, 0.72f, 0.12f));
            WeaponDefinition swordDefinition = GetOrCreateWeaponDefinition("Sword");
            WeaponDefinition chainBladeDefinition = GetOrCreateChainBladeDefinition();
            KickDefinition kickDefinition = GetOrCreateKickDefinition();
            DodgeDefinition dodgeDefinition = GetOrCreateDodgeDefinition();
            CheerDefinition cheerDefinition = GetOrCreateCheerDefinition();
            SwordEnemyDefinition swordEnemyDefinition = GetOrCreateSwordEnemyDefinition();
            ShieldDefinition shieldDefinition = GetOrCreateShieldDefinition();
            ArcherEnemyDefinition archerEnemyDefinition = GetOrCreateArcherEnemyDefinition();
            SpikeWallDefinition spikeWallDefinition = GetOrCreateSpikeWallDefinition();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject environment = new GameObject("Environment");
            CreateCube("Floor", environment.transform, new Vector3(0f, -0.25f, 0f), new Vector3(24f, 0.5f, 18f), floorMaterial);
            CreateCube("Wall_North", environment.transform, new Vector3(0f, 1f, 9.25f), new Vector3(24.5f, 2.5f, 0.5f), wallMaterial);
            CreateCube("Wall_South", environment.transform, new Vector3(0f, 1f, -9.25f), new Vector3(24.5f, 2.5f, 0.5f), wallMaterial);
            CreateCube("Wall_East", environment.transform, new Vector3(12.25f, 1f, 0f), new Vector3(0.5f, 2.5f, 18f), wallMaterial);
            CreateCube("Wall_West", environment.transform, new Vector3(-12.25f, 1f, 0f), new Vector3(0.5f, 2.5f, 18f), wallMaterial);
            CreateSpikeWall(
                environment.transform,
                new Vector3(0f, 1f, 8.45f),
                Vector3.back,
                spikeWallDefinition,
                spikeMaterial);

            GameObject player = new GameObject("Player");
            player.transform.position = Vector3.zero;

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.45f;
            controller.center = new Vector3(0f, 1f, 0f);
            controller.stepOffset = 0.3f;
            controller.slopeLimit = 45f;
            PlayerController playerController = player.AddComponent<PlayerController>();

            CharacterVisual playerVisual = CreateCharacterVisual(
                ArenaCharacterRole.Player,
                artCatalog,
                player.transform,
                playerMaterial);

            GameObject facingMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            facingMarker.name = "FacingMarker";
            facingMarker.transform.SetParent(player.transform, false);
            facingMarker.transform.localPosition = new Vector3(0f, 1f, 0.72f);
            facingMarker.transform.localScale = new Vector3(0.22f, 0.22f, 0.75f);
            Object.DestroyImmediate(facingMarker.GetComponent<Collider>());
            facingMarker.GetComponent<Renderer>().sharedMaterial = facingMaterial;

            GameObject weaponPivot = new GameObject("WeaponPivot");
            weaponPivot.transform.SetParent(player.transform, false);
            weaponPivot.transform.localPosition = new Vector3(0f, 1f, 0f);

            PlayerMeleeCombat meleeCombat = player.AddComponent<PlayerMeleeCombat>();
            meleeCombat.Configure(swordDefinition, weaponPivot.transform);

            GameObject kickVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            kickVisual.name = "KickVisual";
            kickVisual.transform.SetParent(player.transform, false);
            kickVisual.transform.localPosition = new Vector3(-0.28f, 0.35f, 0f);
            kickVisual.transform.localScale = new Vector3(0.22f, 0.22f, 0.65f);
            Object.DestroyImmediate(kickVisual.GetComponent<Collider>());
            kickVisual.GetComponent<Renderer>().sharedMaterial = facingMaterial;

            PlayerKickCombat kickCombat = player.AddComponent<PlayerKickCombat>();
            kickCombat.Configure(kickDefinition, meleeCombat, kickVisual.transform);
            meleeCombat.SetKickCombat(kickCombat);

            PlayerDodge playerDodge = player.AddComponent<PlayerDodge>();
            playerDodge.Configure(dodgeDefinition, playerController, meleeCombat, kickCombat);
            playerController.SetDodge(playerDodge);
            meleeCombat.SetDodge(playerDodge);
            kickCombat.SetDodge(playerDodge);

            PlayerWeaponEquipment weaponEquipment = player.AddComponent<PlayerWeaponEquipment>();
            weaponEquipment.Configure(weaponPivot.transform, meleeCombat, kickCombat, playerDodge);

            WeaponInstance startingSword = GrayboxWeaponFactory.Create(
                "PlayerSword",
                player.transform.position,
                Quaternion.identity,
                swordDefinition,
                weaponMaterial,
                pickupIndicatorMaterial);
            weaponEquipment.EquipInitial(startingSword);

            GrayboxWeaponFactory.Create(
                "GroundChainBlade",
                new Vector3(2.6f, 0.15f, -1.8f),
                Quaternion.Euler(0f, 90f, 0f),
                chainBladeDefinition,
                chainBladeMaterial,
                pickupIndicatorMaterial);

            PlayerHealth playerHealth = player.AddComponent<PlayerHealth>();
            playerHealth.Configure(
                playerVisual,
                playerController,
                meleeCombat,
                kickCombat,
                playerDodge,
                null);

            PlayerCheer playerCheer = player.AddComponent<PlayerCheer>();
            playerCheer.Configure(cheerDefinition);
            PlayerCombatGrowth combatGrowth = player.AddComponent<PlayerCombatGrowth>();
            combatGrowth.Configure(playerCheer);
            meleeCombat.SetCombatGrowth(combatGrowth);
            kickCombat.SetCombatGrowth(combatGrowth);
            playerDodge.SetCombatGrowth(combatGrowth);

            GameObject attackRange = new GameObject("AttackRangePreview");
            attackRange.transform.SetParent(player.transform, false);
            attackRange.transform.localPosition = new Vector3(0f, 0.035f, 0f);
            attackRange.AddComponent<MeshFilter>();
            attackRange.AddComponent<MeshRenderer>();
            LineRenderer lineRenderer = attackRange.AddComponent<LineRenderer>();
            lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            AttackRangeVisualizer visualizer = attackRange.AddComponent<AttackRangeVisualizer>();
            visualizer.Configure(
                swordDefinition,
                meleeCombat,
                rangeFillMaterial,
                rangeOutlineMaterial);

            GameObject kickRange = new GameObject("KickRangePreview");
            kickRange.transform.SetParent(player.transform, false);
            kickRange.transform.localPosition = new Vector3(0f, 0.045f, 0f);
            kickRange.AddComponent<MeshFilter>();
            kickRange.AddComponent<MeshRenderer>();
            LineRenderer kickLine = kickRange.AddComponent<LineRenderer>();
            kickLine.shadowCastingMode = ShadowCastingMode.Off;
            kickLine.receiveShadows = false;
            KickRangeVisualizer kickVisualizer = kickRange.AddComponent<KickRangeVisualizer>();
            kickVisualizer.Configure(kickDefinition, kickCombat, kickFillMaterial, kickOutlineMaterial);

            CreateTrainingDummy(
                new Vector3(-4f, 0f, 3.5f),
                dummyMaterial,
                barBackgroundMaterial,
                healthBarMaterial,
                postureBarMaterial);

            GameObject weaponDropRegistryObject = new GameObject("WeaponDropRegistry");
            WeaponDropRegistry weaponDropRegistry = weaponDropRegistryObject.AddComponent<WeaponDropRegistry>();
            GameObject attackCoordinatorObject = new GameObject("EnemyAttackCoordinator");
            EnemyAttackCoordinator attackCoordinator = attackCoordinatorObject.AddComponent<EnemyAttackCoordinator>();

            EnemyWaveMember wave1Sword = CreateSwordEnemy(
                "SwordEnemy_W1",
                1,
                new Vector3(4f, 0f, 4.5f),
                player.transform,
                playerHealth,
                swordEnemyDefinition,
                ArenaCharacterRole.Swordsman,
                artCatalog,
                swordEnemyMaterial,
                weaponMaterial,
                barBackgroundMaterial,
                healthBarMaterial,
                postureBarMaterial,
                false);

            EnemyWaveMember wave1Archer = CreateArcherEnemy(
                "ArcherEnemy_W1",
                1,
                new Vector3(-6f, 0f, -3.8f),
                player.transform,
                playerHealth,
                archerEnemyDefinition,
                artCatalog,
                archerEnemyMaterial,
                archerWeaponMaterial,
                archerAimMaterial,
                projectileMaterial,
                barBackgroundMaterial,
                healthBarMaterial,
                postureBarMaterial);

            EnemyWaveMember wave2SwordA = CreateSwordEnemy(
                "SwordEnemy_W2_A",
                2,
                new Vector3(-4.5f, 0f, 5f),
                player.transform,
                playerHealth,
                swordEnemyDefinition,
                ArenaCharacterRole.Swordsman,
                artCatalog,
                swordEnemyMaterial,
                weaponMaterial,
                barBackgroundMaterial,
                healthBarMaterial,
                postureBarMaterial,
                false);
            EnemyWaveMember wave2SwordB = CreateSwordEnemy(
                "ShieldEnemy_W2",
                2,
                new Vector3(4.5f, 0f, 5f),
                player.transform,
                playerHealth,
                swordEnemyDefinition,
                ArenaCharacterRole.ShieldBearer,
                artCatalog,
                swordEnemyMaterial,
                weaponMaterial,
                barBackgroundMaterial,
                healthBarMaterial,
                postureBarMaterial,
                true);
            AddShieldToSwordEnemy(wave2SwordB, shieldDefinition, shieldMaterial);

            EnemyWaveMember wave3SwordA = CreateSwordEnemy(
                "SwordEnemy_W3_A",
                3,
                new Vector3(-5f, 0f, 4.5f),
                player.transform,
                playerHealth,
                swordEnemyDefinition,
                ArenaCharacterRole.Swordsman,
                artCatalog,
                swordEnemyMaterial,
                weaponMaterial,
                barBackgroundMaterial,
                healthBarMaterial,
                postureBarMaterial,
                false);
            EnemyWaveMember wave3SwordB = CreateSwordEnemy(
                "ShieldEnemy_W3",
                3,
                new Vector3(5f, 0f, 4.5f),
                player.transform,
                playerHealth,
                swordEnemyDefinition,
                ArenaCharacterRole.ShieldBearer,
                artCatalog,
                swordEnemyMaterial,
                weaponMaterial,
                barBackgroundMaterial,
                healthBarMaterial,
                postureBarMaterial,
                true);
            AddShieldToSwordEnemy(wave3SwordB, shieldDefinition, shieldMaterial);
            EnemyWaveMember wave3Archer = CreateArcherEnemy(
                "ArcherEnemy_W3",
                3,
                new Vector3(0f, 0f, -6f),
                player.transform,
                playerHealth,
                archerEnemyDefinition,
                artCatalog,
                archerEnemyMaterial,
                archerWeaponMaterial,
                archerAimMaterial,
                projectileMaterial,
                barBackgroundMaterial,
                healthBarMaterial,
                postureBarMaterial);

            EnemyWaveMember[] allEnemies =
            {
                wave1Sword,
                wave1Archer,
                wave2SwordA,
                wave2SwordB,
                wave3SwordA,
                wave3SwordB,
                wave3Archer
            };
            foreach (EnemyWaveMember enemy in allEnemies)
            {
                enemy.GetComponent<SwordEnemy>()?.SetAttackCoordinator(attackCoordinator);
                enemy.GetComponent<ArcherEnemy>()?.SetAttackCoordinator(attackCoordinator);
            }

            AddEnemyWeaponDrop(
                wave1Sword,
                swordDefinition,
                weaponMaterial,
                pickupIndicatorMaterial,
                weaponDropRegistry);
            AddEnemyWeaponDrop(
                wave2SwordA,
                swordDefinition,
                weaponMaterial,
                pickupIndicatorMaterial,
                weaponDropRegistry);
            AddEnemyWeaponDrop(
                wave2SwordB,
                swordDefinition,
                weaponMaterial,
                pickupIndicatorMaterial,
                weaponDropRegistry);
            AddEnemyWeaponDrop(
                wave3SwordA,
                swordDefinition,
                weaponMaterial,
                pickupIndicatorMaterial,
                weaponDropRegistry);
            AddEnemyWeaponDrop(
                wave3SwordB,
                swordDefinition,
                weaponMaterial,
                pickupIndicatorMaterial,
                weaponDropRegistry);

            GameObject encounterObject = new GameObject("ArenaEncounterController");
            ArenaEncounterController encounter = encounterObject.AddComponent<ArenaEncounterController>();
            encounter.Configure(
                playerHealth,
                combatGrowth,
                new[]
                {
                    new ArenaEncounterController.Wave(
                        "混合开场",
                        new[] { wave1Sword, wave1Archer }),
                    new ArenaEncounterController.Wave(
                        "近战压力",
                        new[] { wave2SwordA, wave2SwordB }),
                    new ArenaEncounterController.Wave(
                        "最终围攻",
                        new[] { wave3SwordA, wave3SwordB, wave3Archer })
                },
                weaponDropRegistry,
                attackCoordinator);

            GameObject hudObject = new GameObject("ArenaHudController");
            ArenaHudController hud = hudObject.AddComponent<ArenaHudController>();
            hud.Configure(
                playerHealth,
                playerCheer,
                combatGrowth,
                meleeCombat,
                playerDodge,
                weaponEquipment,
                encounter);

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            UnityEngine.Camera camera = cameraObject.AddComponent<UnityEngine.Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.11f);
            camera.fieldOfView = 50f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            cameraObject.AddComponent<AudioListener>();

            Vector3 cameraOffset = new Vector3(0f, 14f, -12f);
            cameraObject.transform.position = player.transform.position + cameraOffset;
            cameraObject.transform.LookAt(player.transform.position + Vector3.up * 0.8f);
            ArenaCameraFollow cameraFollow = cameraObject.AddComponent<ArenaCameraFollow>();
            cameraFollow.Configure(player.transform, cameraOffset);

            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            Selection.activeGameObject = player;
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));

            Debug.Log("[ArenaPrototype] 灰盒场景已创建。按 Play 后使用 WASD 移动，鼠标控制朝向。");
        }

        private static CharacterVisual CreateCharacterVisual(
            ArenaCharacterRole role,
            ArenaArtCatalog artCatalog,
            Transform parent,
            Material fallbackMaterial)
        {
            GameObject visualPrefab = artCatalog != null
                ? artCatalog.GetCharacterPrefab(role)
                : null;
            if (visualPrefab == null)
            {
                visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GetRoleCharacterPrefabPath(role));
            }
            if (visualPrefab != null)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(visualPrefab, parent) as GameObject;
                if (instance == null)
                {
                    throw new System.InvalidOperationException($"无法实例化角色视觉预制体：{role}");
                }

                instance.name = "CharacterVisual";
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                CharacterVisual productionVisual = instance.GetComponent<CharacterVisual>();
                if (productionVisual == null || !productionVisual.IsConfigured)
                {
                    throw new System.InvalidOperationException($"角色视觉预制体未正确配置：{role}");
                }

                return productionVisual;
            }

            GameObject visualRoot = new GameObject("CharacterVisual_Graybox");
            visualRoot.transform.SetParent(parent, false);
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(visualRoot.transform, false);
            body.transform.localPosition = Vector3.up;
            Object.DestroyImmediate(body.GetComponent<Collider>());
            Renderer bodyRenderer = body.GetComponent<Renderer>();
            bodyRenderer.sharedMaterial = fallbackMaterial;

            CharacterVisual grayboxVisual = visualRoot.AddComponent<CharacterVisual>();
            grayboxVisual.Configure(new[] { bodyRenderer }, null, null, null);
            return grayboxVisual;
        }

        private static string GetRoleCharacterPrefabPath(ArenaCharacterRole role)
        {
            string prefabName = role switch
            {
                ArenaCharacterRole.Player => "PFB_Player_A",
                ArenaCharacterRole.Swordsman => "PFB_Enemy_Swordsman_A",
                ArenaCharacterRole.Archer => "PFB_Enemy_Archer_A",
                ArenaCharacterRole.ShieldBearer => "PFB_Enemy_Shield_A",
                _ => string.Empty
            };

            return string.IsNullOrEmpty(prefabName)
                ? string.Empty
                : $"{RoleCharacterPrefabFolder}/{prefabName}.prefab";
        }

        private static GameObject CreateCube(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent);
            cube.transform.position = position;
            cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            return cube;
        }

        private static Material GetOrCreateMaterial(string name, Color color)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            material = new Material(shader)
            {
                name = name,
                color = color
            };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Material GetOrCreateTransparentMaterial(string name, Color color)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }

            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material GetOrCreateUnlitMaterial(string name, Color color)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static WeaponDefinition GetOrCreateWeaponDefinition(string name)
        {
            string path = $"{WeaponDataFolder}/{name}.asset";
            WeaponDefinition definition = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
            if (definition != null)
            {
                return definition;
            }

            definition = ScriptableObject.CreateInstance<WeaponDefinition>();
            definition.name = name;
            AssetDatabase.CreateAsset(definition, path);
            return definition;
        }

        private static WeaponDefinition GetOrCreateChainBladeDefinition()
        {
            string path = $"{WeaponDataFolder}/ChainBlade.asset";
            WeaponDefinition definition = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
            if (definition != null)
            {
                return definition;
            }

            definition = ScriptableObject.CreateInstance<WeaponDefinition>();
            definition.name = "ChainBlade";
            AssetDatabase.CreateAsset(definition, path);

            SerializedObject serialized = new SerializedObject(definition);
            serialized.FindProperty("displayName").stringValue = "Chain Blade";
            serialized.FindProperty("kind").enumValueIndex = (int)WeaponKind.ChainBlade;
            serialized.FindProperty("healthDamage").floatValue = 16f;
            serialized.FindProperty("postureDamage").floatValue = 22f;
            serialized.FindProperty("knockbackSpeed").floatValue = 5.5f;
            serialized.FindProperty("attackRange").floatValue = 3.4f;
            serialized.FindProperty("attackArcDegrees").floatValue = 150f;
            serialized.FindProperty("attackHeight").floatValue = 1.8f;
            serialized.FindProperty("windupDuration").floatValue = 0.20f;
            serialized.FindProperty("activeDuration").floatValue = 0.16f;
            serialized.FindProperty("recoveryDuration").floatValue = 0.42f;
            serialized.FindProperty("windupAngle").floatValue = -85f;
            serialized.FindProperty("swingAngle").floatValue = 150f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            return definition;
        }

        private static KickDefinition GetOrCreateKickDefinition()
        {
            string path = $"{CombatDataFolder}/Kick.asset";
            KickDefinition definition = AssetDatabase.LoadAssetAtPath<KickDefinition>(path);
            if (definition != null)
            {
                return definition;
            }

            definition = ScriptableObject.CreateInstance<KickDefinition>();
            definition.name = "Kick";
            AssetDatabase.CreateAsset(definition, path);
            return definition;
        }

        private static DodgeDefinition GetOrCreateDodgeDefinition()
        {
            string path = $"{CombatDataFolder}/Dodge.asset";
            DodgeDefinition definition = AssetDatabase.LoadAssetAtPath<DodgeDefinition>(path);
            if (definition != null)
            {
                return definition;
            }

            definition = ScriptableObject.CreateInstance<DodgeDefinition>();
            definition.name = "Dodge";
            AssetDatabase.CreateAsset(definition, path);
            return definition;
        }

        private static CheerDefinition GetOrCreateCheerDefinition()
        {
            string path = $"{CombatDataFolder}/Cheer.asset";
            CheerDefinition definition = AssetDatabase.LoadAssetAtPath<CheerDefinition>(path);
            if (definition != null)
            {
                return definition;
            }

            definition = ScriptableObject.CreateInstance<CheerDefinition>();
            definition.name = "Cheer";
            AssetDatabase.CreateAsset(definition, path);
            return definition;
        }

        private static SwordEnemyDefinition GetOrCreateSwordEnemyDefinition()
        {
            string path = $"{EnemyDataFolder}/SwordEnemy.asset";
            SwordEnemyDefinition definition = AssetDatabase.LoadAssetAtPath<SwordEnemyDefinition>(path);
            if (definition != null)
            {
                return definition;
            }

            definition = ScriptableObject.CreateInstance<SwordEnemyDefinition>();
            definition.name = "SwordEnemy";
            AssetDatabase.CreateAsset(definition, path);
            return definition;
        }

        private static ArcherEnemyDefinition GetOrCreateArcherEnemyDefinition()
        {
            string path = $"{EnemyDataFolder}/ArcherEnemy.asset";
            ArcherEnemyDefinition definition = AssetDatabase.LoadAssetAtPath<ArcherEnemyDefinition>(path);
            if (definition != null)
            {
                return definition;
            }

            definition = ScriptableObject.CreateInstance<ArcherEnemyDefinition>();
            definition.name = "ArcherEnemy";
            AssetDatabase.CreateAsset(definition, path);
            return definition;
        }

        private static ShieldDefinition GetOrCreateShieldDefinition()
        {
            string path = $"{EnemyDataFolder}/Shield.asset";
            ShieldDefinition definition = AssetDatabase.LoadAssetAtPath<ShieldDefinition>(path);
            if (definition != null)
            {
                return definition;
            }

            definition = ScriptableObject.CreateInstance<ShieldDefinition>();
            definition.name = "Shield";
            AssetDatabase.CreateAsset(definition, path);
            return definition;
        }

        private static SpikeWallDefinition GetOrCreateSpikeWallDefinition()
        {
            string path = $"{EnvironmentDataFolder}/SpikeWall.asset";
            SpikeWallDefinition definition = AssetDatabase.LoadAssetAtPath<SpikeWallDefinition>(path);
            if (definition != null)
            {
                return definition;
            }

            definition = ScriptableObject.CreateInstance<SpikeWallDefinition>();
            definition.name = "SpikeWall";
            AssetDatabase.CreateAsset(definition, path);
            return definition;
        }

        private static void CreateSpikeWall(
            Transform parent,
            Vector3 position,
            Vector3 surfaceNormal,
            SpikeWallDefinition definition,
            Material spikeMaterial)
        {
            GameObject root = new GameObject("SpikeWall_North");
            root.transform.SetParent(parent);
            root.transform.position = position;

            BoxCollider trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(9f, 2.2f, 1f);

            const int spikeCount = 11;
            var renderers = new Renderer[spikeCount];
            for (int i = 0; i < spikeCount; i++)
            {
                float x = Mathf.Lerp(-4f, 4f, i / (float)(spikeCount - 1));
                GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cube);
                spike.name = $"Spike_{i + 1:00}";
                spike.transform.SetParent(root.transform, false);
                spike.transform.localPosition = new Vector3(x, 0f, 0.05f);
                spike.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                spike.transform.localScale = new Vector3(0.42f, 0.42f, 1.2f);
                Object.DestroyImmediate(spike.GetComponent<Collider>());
                renderers[i] = spike.GetComponent<Renderer>();
                renderers[i].sharedMaterial = spikeMaterial;
            }

            ArenaSpikeWall spikeWall = root.AddComponent<ArenaSpikeWall>();
            spikeWall.Configure(definition, surfaceNormal, renderers);
        }

        private static EnemyWaveMember CreateSwordEnemy(
            string name,
            int waveNumber,
            Vector3 position,
            Transform player,
            PlayerHealth playerHealth,
            SwordEnemyDefinition definition,
            ArenaCharacterRole characterRole,
            ArenaArtCatalog artCatalog,
            Material bodyMaterial,
            Material weaponMaterial,
            Material barBackground,
            Material healthBar,
            Material postureBar,
            bool showHealth)
        {
            GameObject enemy = new GameObject(name);
            enemy.transform.position = position;

            EnemyWaveMember waveMember = enemy.AddComponent<EnemyWaveMember>();
            waveMember.Configure(waveNumber);

            CharacterController controller = enemy.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.45f;
            controller.center = new Vector3(0f, 1f, 0f);
            controller.stepOffset = 0.3f;

            CharacterVisual characterVisual = CreateCharacterVisual(
                characterRole,
                artCatalog,
                enemy.transform,
                bodyMaterial);

            GameObject facingMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            facingMarker.name = "FacingMarker";
            facingMarker.transform.SetParent(enemy.transform, false);
            facingMarker.transform.localPosition = new Vector3(0f, 1f, 0.68f);
            facingMarker.transform.localScale = new Vector3(0.18f, 0.18f, 0.60f);
            Object.DestroyImmediate(facingMarker.GetComponent<Collider>());
            facingMarker.GetComponent<Renderer>().sharedMaterial = weaponMaterial;

            GameObject weaponPivot = new GameObject("WeaponPivot");
            weaponPivot.transform.SetParent(enemy.transform, false);
            weaponPivot.transform.localPosition = new Vector3(0f, 1f, 0f);

            GameObject sword = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sword.name = "Sword";
            sword.transform.SetParent(weaponPivot.transform, false);
            sword.transform.localPosition = new Vector3(0.62f, 0f, 0.50f);
            sword.transform.localRotation = Quaternion.Euler(0f, 25f, 0f);
            sword.transform.localScale = new Vector3(0.12f, 0.12f, 1.35f);
            Object.DestroyImmediate(sword.GetComponent<Collider>());
            sword.GetComponent<Renderer>().sharedMaterial = weaponMaterial;

            WorldSpaceDebugBars bars = CreateDebugBars(
                enemy.transform,
                barBackground,
                healthBar,
                postureBar,
                showHealth);

            SwordEnemy swordEnemy = enemy.AddComponent<SwordEnemy>();
            swordEnemy.Configure(
                definition,
                player,
                playerHealth,
                weaponPivot.transform,
                characterVisual,
                bars,
                waveMember);
            return waveMember;
        }

        private static EnemyWaveMember CreateArcherEnemy(
            string name,
            int waveNumber,
            Vector3 position,
            Transform player,
            PlayerHealth playerHealth,
            ArcherEnemyDefinition definition,
            ArenaArtCatalog artCatalog,
            Material bodyMaterial,
            Material bowMaterial,
            Material aimMaterial,
            Material projectileMaterial,
            Material barBackground,
            Material healthBar,
            Material postureBar)
        {
            GameObject enemy = new GameObject(name);
            enemy.transform.position = position;

            EnemyWaveMember waveMember = enemy.AddComponent<EnemyWaveMember>();
            waveMember.Configure(waveNumber);

            CharacterController controller = enemy.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.45f;
            controller.center = new Vector3(0f, 1f, 0f);
            controller.stepOffset = 0.3f;

            CharacterVisual characterVisual = CreateCharacterVisual(
                ArenaCharacterRole.Archer,
                artCatalog,
                enemy.transform,
                bodyMaterial);

            GameObject bowRoot = new GameObject("BowRoot");
            bowRoot.transform.SetParent(enemy.transform, false);
            bowRoot.transform.localPosition = new Vector3(0.48f, 1.05f, 0.30f);

            GameObject bow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bow.name = "Bow";
            bow.transform.SetParent(bowRoot.transform, false);
            bow.transform.localScale = new Vector3(0.85f, 0.08f, 0.08f);
            Object.DestroyImmediate(bow.GetComponent<Collider>());
            bow.GetComponent<Renderer>().sharedMaterial = bowMaterial;

            GameObject nockedArrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nockedArrow.name = "NockedArrow";
            nockedArrow.transform.SetParent(bowRoot.transform, false);
            nockedArrow.transform.localPosition = new Vector3(0f, 0f, 0.38f);
            nockedArrow.transform.localScale = new Vector3(0.06f, 0.06f, 0.82f);
            Object.DestroyImmediate(nockedArrow.GetComponent<Collider>());
            nockedArrow.GetComponent<Renderer>().sharedMaterial = projectileMaterial;

            GameObject muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(bowRoot.transform, false);
            muzzle.transform.localPosition = new Vector3(0f, 0f, 0.82f);

            WorldSpaceDebugBars bars = CreateDebugBars(
                enemy.transform,
                barBackground,
                healthBar,
                postureBar,
                false);

            LineRenderer aimLine = enemy.AddComponent<LineRenderer>();
            aimLine.sharedMaterial = aimMaterial;
            aimLine.useWorldSpace = true;
            aimLine.positionCount = 2;
            aimLine.widthMultiplier = 0.035f;
            aimLine.shadowCastingMode = ShadowCastingMode.Off;
            aimLine.receiveShadows = false;
            aimLine.enabled = false;

            ArcherEnemy archerEnemy = enemy.AddComponent<ArcherEnemy>();
            archerEnemy.Configure(
                definition,
                player,
                playerHealth,
                muzzle.transform,
                characterVisual,
                bars,
                aimLine,
                projectileMaterial,
                waveMember);
            return waveMember;
        }

        private static void AddShieldToSwordEnemy(
            EnemyWaveMember waveMember,
            ShieldDefinition shieldDefinition,
            Material shieldMaterial)
        {
            GameObject shield = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shield.name = "Shield";
            shield.transform.SetParent(waveMember.transform, false);
            shield.transform.localPosition = new Vector3(0f, 1f, 0.66f);
            shield.transform.localScale = new Vector3(0.92f, 1.25f, 0.14f);
            Object.DestroyImmediate(shield.GetComponent<Collider>());
            Renderer shieldRenderer = shield.GetComponent<Renderer>();
            shieldRenderer.sharedMaterial = shieldMaterial;

            SwordEnemy swordEnemy = waveMember.GetComponent<SwordEnemy>();
            swordEnemy.ConfigureShield(shieldDefinition, shieldRenderer);
        }

        private static void AddEnemyWeaponDrop(
            EnemyWaveMember waveMember,
            WeaponDefinition weaponDefinition,
            Material weaponMaterial,
            Material pickupIndicatorMaterial,
            WeaponDropRegistry registry)
        {
            EnemyWeaponDrop drop = waveMember.gameObject.AddComponent<EnemyWeaponDrop>();
            drop.Configure(
                waveMember,
                weaponDefinition,
                weaponMaterial,
                pickupIndicatorMaterial,
                registry);
        }

        private static void CreateTrainingDummy(
            Vector3 position,
            Material material,
            Material barBackground,
            Material healthBar,
            Material postureBar)
        {
            GameObject dummy = new GameObject("TrainingDummy");
            dummy.transform.position = position;

            CharacterController controller = dummy.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.45f;
            controller.center = new Vector3(0f, 1f, 0f);
            controller.stepOffset = 0.3f;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(dummy.transform, false);
            body.transform.localPosition = Vector3.up;
            Object.DestroyImmediate(body.GetComponent<Collider>());
            Renderer renderer = body.GetComponent<Renderer>();
            renderer.sharedMaterial = material;

            WorldSpaceDebugBars debugBars = CreateDebugBars(
                dummy.transform,
                barBackground,
                healthBar,
                postureBar,
                true);

            TrainingDummy trainingDummy = dummy.AddComponent<TrainingDummy>();
            trainingDummy.Configure(renderer, debugBars);
        }

        private static WorldSpaceDebugBars CreateDebugBars(
            Transform parent,
            Material backgroundMaterial,
            Material healthMaterial,
            Material postureMaterial,
            bool showHealth)
        {
            GameObject root = new GameObject("DebugBars");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(0f, 2.55f, 0f);
            root.transform.localScale = new Vector3(1.4f, 0.12f, 0.08f);

            Transform healthFill = null;
            if (showHealth)
            {
                CreateBarPart("HealthBackground", root.transform, new Vector3(0f, 0.65f, 0f), Vector3.one, backgroundMaterial);
                healthFill = CreateBarPart("HealthFill", root.transform, new Vector3(0f, 0.65f, -0.06f), Vector3.one * 0.9f, healthMaterial).transform;
            }

            float postureY = showHealth ? -0.65f : 0f;
            CreateBarPart("PostureBackground", root.transform, new Vector3(0f, postureY, 0f), Vector3.one, backgroundMaterial);
            GameObject postureFillObject = CreateBarPart("PostureFill", root.transform, new Vector3(0f, postureY, -0.06f), Vector3.one * 0.9f, postureMaterial);

            WorldSpaceDebugBars bars = root.AddComponent<WorldSpaceDebugBars>();
            bars.Configure(healthFill, postureFillObject.transform, postureFillObject.GetComponent<Renderer>());
            return bars;
        }

        private static GameObject CreateBarPart(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            Object.DestroyImmediate(part.GetComponent<Collider>());
            part.GetComponent<Renderer>().sharedMaterial = material;
            return part;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] currentScenes = EditorBuildSettings.scenes;
            foreach (EditorBuildSettingsScene scene in currentScenes)
            {
                if (scene.path == scenePath)
                {
                    return;
                }
            }

            var updatedScenes = new EditorBuildSettingsScene[currentScenes.Length + 1];
            currentScenes.CopyTo(updatedScenes, 0);
            updatedScenes[^1] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = updatedScenes;
        }
    }
}
