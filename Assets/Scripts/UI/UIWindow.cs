using UnityEngine;

namespace UI
{
    public abstract class UIWindow : MonoBehaviour
    {
        [SerializeField]
        protected GameObject root;

        protected virtual void Update()
        {
            if (!root.activeSelf)
                return;

            if (!ReferenceEquals(InputManager.ActiveWindow, this))
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

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