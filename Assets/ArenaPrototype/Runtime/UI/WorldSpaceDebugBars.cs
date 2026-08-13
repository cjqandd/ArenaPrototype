using UnityEngine;

namespace ArenaPrototype.UI
{
    public sealed class WorldSpaceDebugBars : MonoBehaviour
    {
        [SerializeField] private Transform healthFill;
        [SerializeField] private Transform postureFill;
        [SerializeField] private Renderer postureRenderer;
        [SerializeField] private Color postureColor = new Color(1f, 0.75f, 0.05f);
        [SerializeField] private Color brokenColor = new Color(1f, 0.12f, 0.04f);

        private UnityEngine.Camera gameplayCamera;
        private Material postureMaterial;

        public void Configure(Transform newHealthFill, Transform newPostureFill, Renderer newPostureRenderer)
        {
            healthFill = newHealthFill;
            postureFill = newPostureFill;
            postureRenderer = newPostureRenderer;
        }

        private void Awake()
        {
            gameplayCamera = UnityEngine.Camera.main;
            if (postureRenderer != null)
            {
                postureMaterial = postureRenderer.material;
            }
        }

        private void LateUpdate()
        {
            if (gameplayCamera == null)
            {
                gameplayCamera = UnityEngine.Camera.main;
            }

            if (gameplayCamera != null)
            {
                transform.rotation = gameplayCamera.transform.rotation;
            }
        }

        public void SetValues(float healthNormalized, float postureNormalized, bool postureBroken)
        {
            SetFill(healthFill, healthNormalized);
            SetFill(postureFill, postureNormalized);

            if (postureMaterial != null)
            {
                postureMaterial.color = postureBroken ? brokenColor : postureColor;
            }
        }

        private static void SetFill(Transform fill, float value)
        {
            if (fill == null)
            {
                return;
            }

            value = Mathf.Clamp01(value);
            Vector3 scale = fill.localScale;
            scale.x = value;
            fill.localScale = scale;

            Vector3 position = fill.localPosition;
            position.x = (value - 1f) * 0.5f;
            fill.localPosition = position;
        }
    }
}

