using UnityEngine;

namespace ArenaPrototype.Combat
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(LineRenderer))]
    public sealed class AttackRangeVisualizer : MonoBehaviour
    {
        private const int ArcSegments = 32;

        [SerializeField] private WeaponDefinition weapon;
        [SerializeField] private PlayerMeleeCombat combat;
        [SerializeField] private Color previewColor = new Color(0.05f, 0.75f, 1f, 0.18f);
        [SerializeField] private Color activeColor = new Color(1f, 0.28f, 0.05f, 0.38f);
        [SerializeField, Min(0.001f)] private float outlineWidth = 0.045f;

        private Mesh rangeMesh;
        private MeshRenderer meshRenderer;
        private LineRenderer lineRenderer;
        private Material fillMaterial;
        private Material outlineMaterial;
        private bool wasActive;

        public void Configure(
            WeaponDefinition newWeapon,
            PlayerMeleeCombat newCombat,
            Material fill,
            Material outline)
        {
            weapon = newWeapon;
            combat = newCombat;
            meshRenderer = GetComponent<MeshRenderer>();
            lineRenderer = GetComponent<LineRenderer>();
            meshRenderer.sharedMaterial = fill;
            lineRenderer.sharedMaterial = outline;
        }

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            lineRenderer = GetComponent<LineRenderer>();
            fillMaterial = meshRenderer.material;
            outlineMaterial = lineRenderer.material;
            if (combat != null)
            {
                combat.WeaponChanged += HandleWeaponChanged;
                weapon = combat.CurrentWeapon;
            }
            BuildVisual();
            ApplyColor(combat != null && combat.IsHitboxActive);
        }

        private void Update()
        {
            bool isActive = combat != null && combat.IsHitboxActive;
            if (isActive != wasActive)
            {
                ApplyColor(isActive);
            }
        }

        private void OnDestroy()
        {
            if (combat != null)
            {
                combat.WeaponChanged -= HandleWeaponChanged;
            }

            if (rangeMesh != null)
            {
                Destroy(rangeMesh);
            }
        }

        private void HandleWeaponChanged(WeaponDefinition newWeapon)
        {
            weapon = newWeapon;
            BuildVisual();
        }

        private void BuildVisual()
        {
            if (weapon == null)
            {
                GetComponent<MeshFilter>().sharedMesh = null;
                GetComponent<LineRenderer>().positionCount = 0;
                return;
            }

            MeshFilter meshFilter = GetComponent<MeshFilter>();
            lineRenderer = GetComponent<LineRenderer>();

            if (rangeMesh == null)
            {
                rangeMesh = new Mesh { name = "Attack Range Fan" };
            }

            Vector3[] vertices = new Vector3[ArcSegments + 2];
            int[] triangles = new int[ArcSegments * 3];
            vertices[0] = Vector3.zero;

            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.widthMultiplier = outlineWidth;
            lineRenderer.positionCount = ArcSegments + 2;
            lineRenderer.SetPosition(0, Vector3.zero);

            for (int i = 0; i <= ArcSegments; i++)
            {
                float angle = Mathf.Lerp(
                    -weapon.AttackArcDegrees * 0.5f,
                    weapon.AttackArcDegrees * 0.5f,
                    i / (float)ArcSegments);
                Vector3 point = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * weapon.AttackRange;
                vertices[i + 1] = point;
                lineRenderer.SetPosition(i + 1, point);

                if (i < ArcSegments)
                {
                    int triangleIndex = i * 3;
                    triangles[triangleIndex] = 0;
                    triangles[triangleIndex + 1] = i + 1;
                    triangles[triangleIndex + 2] = i + 2;
                }
            }

            rangeMesh.Clear();
            rangeMesh.vertices = vertices;
            rangeMesh.triangles = triangles;
            rangeMesh.RecalculateNormals();
            rangeMesh.RecalculateBounds();
            meshFilter.sharedMesh = rangeMesh;
        }

        private void ApplyColor(bool isActive)
        {
            wasActive = isActive;
            Color color = isActive ? activeColor : previewColor;
            Color outlineColor = new Color(color.r, color.g, color.b, Mathf.Min(1f, color.a + 0.55f));

            if (fillMaterial != null)
            {
                fillMaterial.color = color;
            }

            if (outlineMaterial != null)
            {
                outlineMaterial.color = outlineColor;
            }
        }
    }
}
