using UnityEngine;

namespace ArenaPrototype.Art
{
    public enum ArenaEquipmentRole
    {
        Sword,
        ChainBlade,
        Bow,
        Arrow,
        Shield
    }

    [CreateAssetMenu(
        menuName = "Arena Prototype/美术/装备视觉配置",
        fileName = "EquipmentVisualProfile")]
    public sealed class EquipmentVisualProfile : ScriptableObject
    {
        [SerializeField] private ArenaEquipmentRole role;
        [SerializeField] private GameObject visualPrefab;

        [Header("模型适配")]
        [SerializeField] private Vector3 modelLocalScale = Vector3.one;

        [Header("装备姿态")]
        [SerializeField] private Vector3 equippedLocalPosition;
        [SerializeField] private Vector3 equippedLocalEuler;

        [Header("落地姿态")]
        [SerializeField] private Vector3 groundedLocalPosition;
        [SerializeField] private Vector3 groundedLocalEuler;

        [Header("拾取碰撞范围")]
        [SerializeField] private Vector3 interactionColliderCenter;
        [SerializeField] private Vector3 interactionColliderSize = new Vector3(0.35f, 0.30f, 1.60f);

        public ArenaEquipmentRole Role => role;
        public GameObject VisualPrefab => visualPrefab;
        public Vector3 ModelLocalScale => modelLocalScale;
        public Vector3 EquippedLocalPosition => equippedLocalPosition;
        public Vector3 EquippedLocalEuler => equippedLocalEuler;
        public Vector3 GroundedLocalPosition => groundedLocalPosition;
        public Vector3 GroundedLocalEuler => groundedLocalEuler;
        public Vector3 InteractionColliderCenter => interactionColliderCenter;
        public Vector3 InteractionColliderSize => interactionColliderSize;
        public bool IsConfigured => visualPrefab != null;

        public void Configure(
            ArenaEquipmentRole newRole,
            GameObject newVisualPrefab,
            Vector3 newModelLocalScale,
            Vector3 newEquippedLocalPosition,
            Vector3 newEquippedLocalEuler,
            Vector3 newGroundedLocalPosition,
            Vector3 newGroundedLocalEuler,
            Vector3 newColliderCenter,
            Vector3 newColliderSize)
        {
            role = newRole;
            visualPrefab = newVisualPrefab;
            modelLocalScale = newModelLocalScale;
            equippedLocalPosition = newEquippedLocalPosition;
            equippedLocalEuler = newEquippedLocalEuler;
            groundedLocalPosition = newGroundedLocalPosition;
            groundedLocalEuler = newGroundedLocalEuler;
            interactionColliderCenter = newColliderCenter;
            interactionColliderSize = newColliderSize;
        }
    }
}
