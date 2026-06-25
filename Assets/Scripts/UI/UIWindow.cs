using UnityEngine;

namespace UI
{
    public abstract class UIWindow : MonoBehaviour
    {
        [SerializeField]
        protected GameObject root;

        public virtual void Open()
        {
            if (!InputManager.TryOpenWindow(this))
                return;

            root.SetActive(true);
        }

        public virtual void Close()
        {
            if (!root.activeSelf)
                return;

            root.SetActive(false);

            InputManager.CloseWindow(this);
        }
    }
}