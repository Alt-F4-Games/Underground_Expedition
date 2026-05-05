using Fusion;
using UnityEngine;

namespace Network.Interaction
{
    /// <summary>
    /// Base class for all interactable objects.
    /// Handles interaction data and provides built-in visual feedback using a fresnel effect.
    /// Visual feedback is applied locally using MaterialPropertyBlock to avoid material instancing.
    /// </summary>
    public abstract class InteractableBase : NetworkBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [Tooltip("Text displayed on the player's screen when aiming at this object.")]
        [SerializeField] protected string _promptMessage = "Interact";

        [Tooltip("Time in seconds the button must be held. Set to 0 for instant interaction.")]
        [SerializeField] protected float _interactionDuration = 1.5f;

        [Header("Visual Feedback")]
        [Tooltip("Fresnel intensity when the object is highlighted.")]
        [SerializeField] private float _fresnelOnValue = 1f;

        [Tooltip("Fresnel intensity when the object is not highlighted.")]
        [SerializeField] private float _fresnelOffValue = 0f;

        [Tooltip("Shader property name used to control the fresnel effect.")]
        [SerializeField] private string _fresnelProperty = "_FresnelIntensity";

        // Cached renderers for this object and its children
        private Renderer[] _renderers;

        // MaterialPropertyBlock used to modify material properties without creating new material instances
        private MaterialPropertyBlock _mpb;

        /// <summary>
        /// Returns the interaction prompt displayed to the player.
        /// </summary>
        public virtual string GetInteractPrompt() => _promptMessage;

        /// <summary>
        /// Returns how long the interaction must be held.
        /// </summary>
        public virtual float GetInteractionDuration() => _interactionDuration;

        /// <summary>
        /// Determines whether the specified player can interact with this object.
        /// </summary>
        public virtual bool CanInteract(PlayerRef player)
        {
            return true;
        }

        /// <summary>
        /// Called when the object is spawned on the network.
        /// Caches renderer references and initializes the MaterialPropertyBlock.
        /// </summary>
        public override void Spawned()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            _mpb = new MaterialPropertyBlock();
        }

        /// <summary>
        /// Called locally when the player starts hovering this interactable.
        /// Applies the highlight effect.
        /// </summary>
        public virtual void OnHoverEnter(NetworkPlayerController player)
        {
            SetFresnel(_fresnelOnValue);
        }

        /// <summary>
        /// Called locally when the player stops hovering this interactable.
        /// Removes the highlight effect.
        /// </summary>
        public virtual void OnHoverExit(NetworkPlayerController player)
        {
            SetFresnel(_fresnelOffValue);
        }

        /// <summary>
        /// Applies a fresnel value to all cached renderers using MaterialPropertyBlock.
        /// This avoids modifying shared materials or creating runtime material instances.
        /// </summary>
        private void SetFresnel(float value)
        {
            if (_renderers == null) return;

            foreach (var renderer in _renderers)
            {
                renderer.GetPropertyBlock(_mpb);
                _mpb.SetFloat(_fresnelProperty, value);
                renderer.SetPropertyBlock(_mpb);
            }
        }

        /// <summary>
        /// Executes the interaction logic. Must be implemented by derived classes.
        /// </summary>
        public abstract void OnInteract(NetworkPlayerController player);
    }
}