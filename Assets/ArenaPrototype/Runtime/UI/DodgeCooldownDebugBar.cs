using ArenaPrototype.Movement;
using UnityEngine;

namespace ArenaPrototype.UI
{
    public sealed class DodgeCooldownDebugBar : MonoBehaviour
    {
        [SerializeField] private PlayerDodge dodge;
        [SerializeField] private Transform fill;
        [SerializeField] private Renderer fillRenderer;
        [SerializeField] private Color readyColor = new Color(0.12f, 0.90f, 1f);
        [SerializeField] private Color cooldownColor = new Color(0.25f, 0.28f, 0.32f);
        [SerializeField] private Color invulnerableColor = new Color(1f, 1f, 1f);

        private UnityEngine.Camera gameplayCamera;
        private Material fillMaterial;

        public void Configure(PlayerDodge newDodge, Transform newFill, Renderer newFillRenderer)
        {
            dodge = newDodge;
            fill = newFill;
            fillRenderer = newFillRenderer;
        }

        private void Awake()
        {
            gameplayCamera = UnityEngine.Camera.main;
            if (fillRenderer != null)
            {
                fillMaterial = fillRenderer.material;
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

            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (dodge == null || fill == null)
            {
                return;
            }

            float readyAmount = dodge.IsReady ? 1f : 1f - dodge.CooldownNormalized;
            Vector3 scale = fill.localScale;
            scale.x = Mathf.Clamp01(readyAmount);
            fill.localScale = scale;

            Vector3 position = fill.localPosition;
            position.x = (Mathf.Clamp01(readyAmount) - 1f) * 0.5f;
            fill.localPosition = position;

            if (fillMaterial != null)
            {
                fillMaterial.color = dodge.IsInvulnerable
                    ? invulnerableColor
                    : dodge.IsReady ? readyColor : cooldownColor;
            }
        }
    }
}

