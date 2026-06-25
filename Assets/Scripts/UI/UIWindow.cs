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

            InputManager.PushUI();
        }

        public virtual void Close()
        {
            root.SetActive(false);

            InputManager.PopUI();
        }
    }
}