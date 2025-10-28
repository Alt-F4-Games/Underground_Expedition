using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance {get; private set;}

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
}
