using System;
using System.Collections.Generic;
using ArenaPrototype.Art;
using UnityEditor;
using UnityEngine;

namespace ArenaPrototype.Editor
{
    public sealed class ArenaArtManagerWindow : EditorWindow
    {
        private ArenaArtCatalog catalog;
        private Vector2 scroll;
        private readonly List<string> validationMessages = new List<string>();
        private GameObject newEquipmentPrefab;
        private ArenaEquipmentRole newEquipmentRole = ArenaEquipmentRole.Sword;

        [MenuItem("Arena Prototype/美术资源/打开美术资源管理器")]
        public static void Open()
        {
            ArenaArtManagerWindow window = GetWindow<ArenaArtManagerWindow>("美术资源管理器");
            window.minSize = new Vector2(560f, 620f);
            window.Show();
        }

        private void OnEnable()
        {
            LoadCatalog();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Arena Prototype 美术资源管理器", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "只在这里更换角色身份预制体和装备视觉配置。不要直接修改 ThirdParty 原包，也不要把模型拖进战斗脚本。",
                MessageType.Info);

            catalog = (ArenaArtCatalog)EditorGUILayout.ObjectField(
                "美术目录",
                catalog,
                typeof(ArenaArtCatalog),
                false);
            if (catalog == null)
            {
                if (GUILayout.Button("加载默认美术目录"))
                {
                    LoadCatalog();
                }
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawSetSelection();
            DrawCharacterSlots();
            DrawEquipmentSlots();
            DrawEquipmentProfileCreator();
            DrawValidationAndApply();
            EditorGUILayout.EndScrollView();
        }

        private void DrawSetSelection()
        {
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("1. 选择美术套装", EditorStyles.boldLabel);

            ArenaArtSet activeSet = (ArenaArtSet)EditorGUILayout.ObjectField(
                "当前套装",
                catalog.ActiveArtSet,
                typeof(ArenaArtSet),
                false);
            ArenaArtSet fallbackSet = (ArenaArtSet)EditorGUILayout.ObjectField(
                "后备套装",
                catalog.FallbackArtSet,
                typeof(ArenaArtSet),
                false);
            if (activeSet != catalog.ActiveArtSet || fallbackSet != catalog.FallbackArtSet)
            {
                Undo.RecordObject(catalog, "修改美术套装");
                catalog.ConfigureArtSets(activeSet, fallbackSet);
                SaveAsset(catalog);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("使用 KayKit 套装"))
            {
                ArenaArtSet kayKit = AssetDatabase.LoadAssetAtPath<ArenaArtSet>(
                    ArenaArtManagementSetup.KayKitSetPath);
                ArenaArtManagementSetup.SwitchActiveSet(kayKit, false);
                LoadCatalog();
            }
            if (GUILayout.Button("切回灰盒"))
            {
                ArenaArtSet graybox = AssetDatabase.LoadAssetAtPath<ArenaArtSet>(
                    ArenaArtManagementSetup.GrayboxSetPath);
                ArenaArtManagementSetup.SwitchActiveSet(graybox, false);
                LoadCatalog();
            }
            if (GUILayout.Button("在 Project 中定位"))
            {
                Selection.activeObject = catalog.ActiveArtSet;
                EditorGUIUtility.PingObject(catalog.ActiveArtSet);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCharacterSlots()
        {
            ArenaArtSet activeSet = catalog.ActiveArtSet;
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("2. 角色身份", EditorStyles.boldLabel);
            if (activeSet == null || activeSet.ProceduralGraybox)
            {
                EditorGUILayout.HelpBox("当前使用程序灰盒角色。切换到正式套装后才能编辑角色槽。", MessageType.Info);
                return;
            }

            DrawCharacterSlot(activeSet, ArenaCharacterRole.Player, "玩家");
            DrawCharacterSlot(activeSet, ArenaCharacterRole.Swordsman, "剑士");
            DrawCharacterSlot(activeSet, ArenaCharacterRole.Archer, "弓箭手");
            DrawCharacterSlot(activeSet, ArenaCharacterRole.ShieldBearer, "盾兵");
        }

        private static void DrawCharacterSlot(
            ArenaArtSet artSet,
            ArenaCharacterRole role,
            string label)
        {
            GameObject current = artSet.GetCharacterPrefab(role);
            GameObject next = (GameObject)EditorGUILayout.ObjectField(
                label,
                current,
                typeof(GameObject),
                false);
            if (next != current)
            {
                Undo.RecordObject(artSet, $"修改{label}视觉");
                artSet.SetCharacterPrefab(role, next);
                SaveAsset(artSet);
            }
        }

        private void DrawEquipmentSlots()
        {
            ArenaArtSet activeSet = catalog.ActiveArtSet;
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("3. 武器与装备", EditorStyles.boldLabel);
            if (activeSet == null || activeSet.ProceduralGraybox)
            {
                EditorGUILayout.HelpBox("当前使用程序灰盒装备。切换到正式套装后才能编辑装备槽。", MessageType.Info);
                return;
            }

            DrawEquipmentSlot(activeSet, ArenaEquipmentRole.Sword, "单手剑");
            DrawEquipmentSlot(activeSet, ArenaEquipmentRole.ChainBlade, "链刃");
            DrawEquipmentSlot(activeSet, ArenaEquipmentRole.Bow, "弓");
            DrawEquipmentSlot(activeSet, ArenaEquipmentRole.Arrow, "箭");
            DrawEquipmentSlot(activeSet, ArenaEquipmentRole.Shield, "盾牌");
        }

        private static void DrawEquipmentSlot(
            ArenaArtSet artSet,
            ArenaEquipmentRole role,
            string label)
        {
            EditorGUILayout.BeginHorizontal();
            EquipmentVisualProfile current = artSet.GetEquipmentProfile(role);
            EquipmentVisualProfile next = (EquipmentVisualProfile)EditorGUILayout.ObjectField(
                label,
                current,
                typeof(EquipmentVisualProfile),
                false);
            if (next != current)
            {
                Undo.RecordObject(artSet, $"修改{label}视觉配置");
                artSet.SetEquipmentProfile(role, next);
                SaveAsset(artSet);
            }
            using (new EditorGUI.DisabledScope(next == null))
            {
                if (GUILayout.Button("编辑姿态", GUILayout.Width(80f)))
                {
                    Selection.activeObject = next;
                    EditorGUIUtility.PingObject(next);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (next == null)
            {
                EditorGUILayout.HelpBox($"{label}为空，将使用灰盒后备外观。", MessageType.Warning);
            }
        }

        private void DrawEquipmentProfileCreator()
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("4. 从新模型创建装备配置", EditorStyles.boldLabel);
            newEquipmentPrefab = (GameObject)EditorGUILayout.ObjectField(
                "视觉预制体",
                newEquipmentPrefab,
                typeof(GameObject),
                false);
            newEquipmentRole = (ArenaEquipmentRole)EditorGUILayout.EnumPopup(
                "装备类型",
                newEquipmentRole);
            using (new EditorGUI.DisabledScope(
                newEquipmentPrefab == null
                || catalog.ActiveArtSet == null
                || catalog.ActiveArtSet.ProceduralGraybox))
            {
                if (GUILayout.Button("创建配置并放入当前槽"))
                {
                    EquipmentVisualProfile profile =
                        ArenaArtManagementSetup.CreateUserEquipmentProfile(
                            newEquipmentPrefab,
                            newEquipmentRole);
                    if (profile != null)
                    {
                        Undo.RecordObject(catalog.ActiveArtSet, "创建装备视觉配置");
                        catalog.ActiveArtSet.SetEquipmentProfile(newEquipmentRole, profile);
                        SaveAsset(catalog.ActiveArtSet);
                        newEquipmentPrefab = null;
                        validationMessages.Clear();
                    }
                }
            }
        }

        private void DrawValidationAndApply()
        {
            EditorGUILayout.Space(14f);
            EditorGUILayout.LabelField("5. 验证并应用", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("验证当前套装"))
            {
                ValidateCurrentCatalog();
            }
            if (GUILayout.Button("重建灰盒场景并应用"))
            {
                ValidateCurrentCatalog();
                if (validationMessages.Count == 0)
                {
                    ArenaArtManagementSetup.ApplyActiveSetToScene(true);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "“重建灰盒场景并应用”会重新生成 GrayboxArena。直接在这个场景里手工摆放、但没有写入生成器的对象会被覆盖。",
                MessageType.Warning);

            if (validationMessages.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "没有阻塞错误。链刃等空槽会使用灰盒后备资源。",
                    MessageType.Info);
            }
            else
            {
                foreach (string message in validationMessages)
                {
                    EditorGUILayout.HelpBox(message, MessageType.Error);
                }
            }
        }

        private void ValidateCurrentCatalog()
        {
            validationMessages.Clear();
            validationMessages.AddRange(ArenaArtManagementSetup.ValidateCatalog(catalog));
            Repaint();
        }

        private void LoadCatalog()
        {
            catalog = AssetDatabase.LoadAssetAtPath<ArenaArtCatalog>(
                ArenaArtManagementSetup.CatalogPath);
            validationMessages.Clear();
        }

        private static void SaveAsset(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }
    }
}
