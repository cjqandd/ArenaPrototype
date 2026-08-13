using ArenaPrototype.Combat;
using UnityEngine;

namespace ArenaPrototype.Weapons
{
    public static class GrayboxWeaponFactory
    {
        public static WeaponInstance Create(
            string name,
            Vector3 position,
            Quaternion rotation,
            WeaponDefinition definition,
            Material weaponMaterial,
            Material pickupIndicatorMaterial)
        {
            GameObject root = new GameObject(name);
            root.transform.SetPositionAndRotation(position, rotation);

            BoxCollider interactionCollider = root.AddComponent<BoxCollider>();
            ConfigureInteractionCollider(interactionCollider, definition.Kind);
            interactionCollider.isTrigger = true;

            Rigidbody weaponRigidbody = root.AddComponent<Rigidbody>();
            weaponRigidbody.mass = 1f;
            weaponRigidbody.useGravity = false;
            weaponRigidbody.isKinematic = true;
            weaponRigidbody.interpolation = RigidbodyInterpolation.None;
            weaponRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            GameObject modelRoot = new GameObject("Model");
            modelRoot.transform.SetParent(root.transform, false);
            if (definition.Kind == WeaponKind.ChainBlade)
            {
                CreateChainBladeModel(modelRoot.transform, weaponMaterial);
            }
            else
            {
                CreateSwordModel(modelRoot.transform, weaponMaterial);
            }

            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicator.name = "PickupIndicator";
            indicator.transform.SetParent(root.transform, false);
            indicator.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            indicator.transform.localScale = new Vector3(0.22f, 0.025f, 0.22f);
            RemoveCollider(indicator);
            indicator.GetComponent<Renderer>().sharedMaterial = pickupIndicatorMaterial;

            WeaponInstance weapon = root.AddComponent<WeaponInstance>();
            weapon.Configure(
                definition,
                modelRoot.transform,
                indicator,
                interactionCollider,
                weaponRigidbody);
            return weapon;
        }

        private static void ConfigureInteractionCollider(BoxCollider collider, WeaponKind kind)
        {
            if (kind == WeaponKind.ChainBlade)
            {
                collider.center = new Vector3(0f, 0f, 1.15f);
                collider.size = new Vector3(0.45f, 0.30f, 2.65f);
            }
            else
            {
                collider.size = new Vector3(0.35f, 0.30f, 1.60f);
            }
        }

        private static void CreateSwordModel(Transform parent, Material material)
        {
            GameObject blade = CreatePart(
                "Blade",
                parent,
                new Vector3(0f, 0f, 0.72f),
                new Vector3(0.12f, 0.12f, 1.45f),
                material);
            blade.transform.localRotation = Quaternion.identity;
        }

        private static void CreateChainBladeModel(Transform parent, Material material)
        {
            CreatePart(
                "Handle",
                parent,
                new Vector3(0f, 0f, 0.20f),
                new Vector3(0.16f, 0.16f, 0.42f),
                material);

            const int segmentCount = 8;
            for (int i = 0; i < segmentCount; i++)
            {
                float z = 0.55f + i * 0.28f;
                GameObject segment = CreatePart(
                    $"ChainSegment_{i + 1:00}",
                    parent,
                    new Vector3(0f, 0f, z),
                    new Vector3(0.18f, 0.10f, 0.22f),
                    material);
                segment.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            }

            CreatePart(
                "EndBlade",
                parent,
                new Vector3(0f, 0f, 2.75f),
                new Vector3(0.30f, 0.10f, 0.70f),
                material);
        }

        private static GameObject CreatePart(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            RemoveCollider(part);
            part.GetComponent<Renderer>().sharedMaterial = material;
            return part;
        }

        private static void RemoveCollider(GameObject target)
        {
            Collider collider = target.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(collider);
                }
                else
                {
                    Object.DestroyImmediate(collider);
                }
            }
        }
    }
}
