using UnityEngine;
using UnityEngine.UI;
using Network.Interaction;

namespace UI
{
    /// <summary>
    /// Manages the circular progress bar around the crosshair.
    /// Only displays when the local player is actively holding the interact button.
    /// </summary>
    public class InteractionProgressUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("The circular image set to Filled -> Radial 360.")]
        [SerializeField] private Image _progressRing;

        private NetworkPlayerInteractor _localInteractor;

        private void Start()
        {
            if (_progressRing != null)
            {
                _progressRing.fillAmount = 0f;
                _progressRing.enabled = false; 
            }
        }

        private void Update()
        {
            if (_localInteractor == null)
            {
                FindLocalInteractor();
                return;
            }
            
            float progress = _localInteractor.GetInteractionProgress();
            
            if (progress > 0f)
            {
                if (!_progressRing.enabled) _progressRing.enabled = true;
                _progressRing.fillAmount = progress;
            }
            else
            {
                if (_progressRing.enabled)
                {
                    _progressRing.enabled = false;
                    _progressRing.fillAmount = 0f;
                }
            }
        }

        private void FindLocalInteractor()
        {
            var interactors = FindObjectsByType<NetworkPlayerInteractor>(FindObjectsSortMode.None);
            
            foreach (var interactor in interactors)
            {
                if (interactor.HasInputAuthority)
                {
                    _localInteractor = interactor;
                    return;
                }
            }
        }
    }
}