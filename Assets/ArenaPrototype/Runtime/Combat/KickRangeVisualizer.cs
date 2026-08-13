using UnityEngine;

namespace ArenaPrototype.Combat
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(LineRenderer))]
    public sealed class KickRangeVisualizer : MonoBehaviour
    {
        private const int ArcSegments = 24;

        [SerializeField] private KickDefinition kick;
        [SerializeField] private PlayerKickCombat combat;
        [SerializeField] private Color previewColor = new Color(1f, 0.82f, 0.08f, 0.14f);
        [SerializeField] private Color activeColor = new Color(1f, 0.22f, 0.04f, 0.42f);
        [SerializeField, Min(0.001f)] private float outlineWidth = 0.04f;

        private Mesh rangeMesh;
        private Material fillMaterial;
        private Material outlineMaterial;
        private bool wasActive;

        public void Configure(
            KickDefinition newKick,
            PlayerKickCombat newCombat,
            Material fill,
            Material outline)
        {
            kick = newKick;
            combat = newCombat;
            GetComponent<MeshRenderer>().sharedMaterial = fill;
            GetComponent<LineRenderer>().sharedMaterial = outline;
        }

        private void Awake()
        {
            fillMaterial = GetComponent<MeshRenderer>().material;
            outlineMaterial = GetComponent<LineRenderer>().material;
            BuildVisual();
            ApplyColor(combat != null && combat.IsHitboxActive);
        }

        private void Update()
        {
            bool active = combat != null && combat.IsHitboxActive;
            if (active != wasActive)
            {
                ApplyColor(active);
            }
        }

        private void BuildVisual()
        {
            if (kick == null)
            {
                return;
            }

            rangeMesh = new Mesh { name = "Kick Range Fan" };
            Vector3[] vertices = new Vector3[ArcSegments + 2];
            int[] triangles = new int[ArcSegments * 3];
            LineRenderer line = GetComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.widthMultiplier = outlineWidth;
            line.positionCount = ArcSegments + 2;
            line.SetPosition(0, Vector3.zero);

            for (int i = 0; i <= ArcSegments; i++)
            {
                float angle = Mathf.Lerp(-kick.ArcDegrees * 0.5f, kick.ArcDegrees * 0.5f, i / (float)ArcSegments);
                Vector3 point = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * kick.Range;
                vertices[i + 1] = point;
                line.SetPosition(i + 1, point);

                if (i < ArcSegments)
                {
                    int index = i * 3;
                    triangles[index] = 0;
                    triangles[index + 1] = i + 1;
                    triangles[index + 2] = i + 2;
                }
            }

            rangeMesh.vertices = vertices;
            rangeMesh.triangles = triangles;
            rangeMesh.RecalculateNormals();
            rangeMesh.RecalculateBounds();
            GetComponent<MeshFilter>().sharedMesh = rangeMesh;
        }

        private void ApplyColor(bool active)
        {
            wasActive = active;
            Color color = active ? activeColor : previewColor;
            fillMaterial.color = color;
            outlineMaterial.color = new Color(color.r, color.g, color.b, Mathf.Min(1f, color.a + 0.55f));
        }

        private void OnDestroy()
        {
            if (rangeMesh != null)
            {
                Destroy(rangeMesh);
            }
        }
    }
}

