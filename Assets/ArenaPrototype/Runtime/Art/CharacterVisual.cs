using System;
using System.Linq;
using UnityEngine;

namespace ArenaPrototype.Art
{
    public sealed class CharacterVisual : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private Renderer[] bodyRenderers;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform leftHandSocket;
        [SerializeField] private Transform rightHandSocket;

        private MaterialPropertyBlock propertyBlock;
        private Color tint = Color.white;
        private Color emission = Color.black;

        public Animator Animator => animator;
        public Transform LeftHandSocket => leftHandSocket;
        public Transform RightHandSocket => rightHandSocket;
        public Renderer PrimaryRenderer => bodyRenderers?.FirstOrDefault(renderer => renderer != null);
        public bool IsConfigured => PrimaryRenderer != null && animator != null;

        public void Configure(
            Renderer[] newBodyRenderers,
            Animator newAnimator,
            Transform newLeftHandSocket,
            Transform newRightHandSocket)
        {
            bodyRenderers = newBodyRenderers?
                .Where(renderer => renderer != null)
                .Distinct()
                .ToArray() ?? Array.Empty<Renderer>();
            animator = newAnimator;
            leftHandSocket = newLeftHandSocket;
            rightHandSocket = newRightHandSocket;
            ApplyProperties();
        }

        private void Awake()
        {
            EnsureReferences();
            ApplyProperties();
        }

        private void OnValidate()
        {
            EnsureReferences();
        }

        public void SetTint(Color color)
        {
            tint = color;
            ApplyProperties();
        }

        public void SetEmission(Color color)
        {
            emission = color;
            ApplyProperties();
        }

        public void ClearEmission()
        {
            emission = Color.black;
            ApplyProperties();
        }

        public void SetVisible(bool visible)
        {
            EnsureReferences();
            foreach (Renderer bodyRenderer in bodyRenderers)
            {
                if (bodyRenderer != null)
                {
                    bodyRenderer.enabled = visible;
                }
            }
        }

        private void EnsureReferences()
        {
            if (bodyRenderers == null || bodyRenderers.Length == 0)
            {
                bodyRenderers = GetComponentsInChildren<Renderer>(true);
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            Transform[] transforms = GetComponentsInChildren<Transform>(true);
            if (leftHandSocket == null)
            {
                leftHandSocket = transforms.FirstOrDefault(transform =>
                    string.Equals(transform.name, "handslot.l", StringComparison.OrdinalIgnoreCase));
            }

            if (rightHandSocket == null)
            {
                rightHandSocket = transforms.FirstOrDefault(transform =>
                    string.Equals(transform.name, "handslot.r", StringComparison.OrdinalIgnoreCase));
            }
        }

        private void ApplyProperties()
        {
            EnsureReferences();
            propertyBlock ??= new MaterialPropertyBlock();
            foreach (Renderer bodyRenderer in bodyRenderers)
            {
                if (bodyRenderer == null)
                {
                    continue;
                }

                bodyRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, tint);
                propertyBlock.SetColor(EmissionColorId, emission);
                bodyRenderer.SetPropertyBlock(propertyBlock);
                propertyBlock.Clear();
            }
        }
    }
}
