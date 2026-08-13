using UnityEngine;

namespace ArenaPrototype.UI
{
    public sealed class PlayerHealthDebugBar : MonoBehaviour
    {
        [SerializeField] private Transform fill;
        private UnityEngine.Camera gameplayCamera;

        public void Configure(Transform newFill)
        {
            fill = newFill;
        }

        private void Awake()
        {
            gameplayCamera = UnityEngine.Camera.main;
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

        public void SetValue(float normalizedValue)
        {
            if (fill == null)
            {
                return;
            }

            float value = Mathf.Clamp01(normalizedValue);
            Vector3 scale = fill.localScale;
            scale.x = value;
            fill.localScale = scale;

            Vector3 position = fill.localPosition;
            position.x = (value - 1f) * 0.5f;
            fill.localPosition = position;
        }
    }
}

