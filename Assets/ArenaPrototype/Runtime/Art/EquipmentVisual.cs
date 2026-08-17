using System.Linq;
using UnityEngine;

namespace ArenaPrototype.Art
{
    public sealed class EquipmentVisual : MonoBehaviour
    {
        [SerializeField] private ArenaEquipmentRole role;
        [SerializeField] private EquipmentVisualProfile profile;
        [SerializeField] private bool proceduralGraybox;
        [SerializeField] private Renderer[] renderers;

        public ArenaEquipmentRole Role => role;
        public EquipmentVisualProfile Profile => profile;
        public bool IsProceduralGraybox => proceduralGraybox;
        public Renderer PrimaryRenderer => renderers?.FirstOrDefault(renderer => renderer != null);

        public void Configure(
            ArenaEquipmentRole newRole,
            EquipmentVisualProfile newProfile,
            bool isProceduralGraybox)
        {
            role = newRole;
            profile = newProfile;
            proceduralGraybox = isProceduralGraybox;
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        public void SetVisible(bool visible)
        {
            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>(true);
            }

            foreach (Renderer targetRenderer in renderers)
            {
                if (targetRenderer != null)
                {
                    targetRenderer.enabled = visible;
                }
            }
        }
    }
}
