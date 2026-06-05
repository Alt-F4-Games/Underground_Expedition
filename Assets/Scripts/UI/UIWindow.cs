using UnityEngine;

namespace UI
{
    public abstract class UIWindow : MonoBehaviour
    {
        [SerializeField]
        protected GameObject root;

        public virtual void Open()
        {
            root.SetActive(true);

            InputManager.SetMode(InputMode.UI);
            InputBlocker.PushBlock();
        }

        public virtual void Close()
        {
            root.SetActive(false);

            InputBlocker.PopBlock();

            if (!InputBlocker.IsBlocked)
            {
                InputManager.SetMode(InputMode.Game);
            }
        }
    }
}