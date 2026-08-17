using UnityEngine;

namespace ArenaPrototype.Art
{
    public static class ArenaVisualFactory
    {
        public static Transform CreateEquipmentPivot(
            string name,
            CharacterVisual characterVisual,
            Transform fallbackParent,
            bool useLeftHand,
            Vector3 fallbackLocalPosition)
        {
            Transform handSocket = null;
            if (characterVisual != null)
            {
                handSocket = useLeftHand
                    ? characterVisual.LeftHandSocket
                    : characterVisual.RightHandSocket;
            }

            GameObject pivot = new GameObject(name);
            pivot.transform.SetParent(handSocket != null ? handSocket : fallbackParent, false);
            pivot.transform.localPosition = handSocket != null ? Vector3.zero : fallbackLocalPosition;
            pivot.transform.localRotation = Quaternion.identity;
            return pivot.transform;
        }

        public static EquipmentVisual CreateEquippedVisual(
            ArenaEquipmentRole role,
            ArenaArtCatalog catalog,
            Transform parent,
            string name)
        {
            EquipmentVisualProfile profile = catalog != null
                ? catalog.GetEquipmentProfile(role)
                : null;
            if (profile == null || !profile.IsConfigured || parent == null)
            {
                return null;
            }

            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = profile.EquippedLocalPosition;
            root.transform.localRotation = Quaternion.Euler(profile.EquippedLocalEuler);
            InstantiateModel(profile, root.transform);

            EquipmentVisual visual = root.AddComponent<EquipmentVisual>();
            visual.Configure(role, profile, false);
            return visual;
        }

        public static EquipmentVisual PopulateGroundedModel(
            ArenaEquipmentRole role,
            EquipmentVisualProfile profile,
            Transform modelRoot)
        {
            if (profile == null || !profile.IsConfigured || modelRoot == null)
            {
                return null;
            }

            modelRoot.localPosition = profile.GroundedLocalPosition;
            modelRoot.localRotation = Quaternion.Euler(profile.GroundedLocalEuler);
            InstantiateModel(profile, modelRoot);

            EquipmentVisual visual = modelRoot.gameObject.AddComponent<EquipmentVisual>();
            visual.Configure(role, profile, false);
            return visual;
        }

        public static GameObject InstantiateProjectileModel(
            EquipmentVisualProfile profile,
            Transform parent)
        {
            return profile == null || !profile.IsConfigured
                ? null
                : InstantiateModel(profile, parent);
        }

        private static GameObject InstantiateModel(
            EquipmentVisualProfile profile,
            Transform parent)
        {
            GameObject model = Object.Instantiate(profile.VisualPrefab, parent);
            model.name = "Visual";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = profile.ModelLocalScale;
            return model;
        }
    }
}
