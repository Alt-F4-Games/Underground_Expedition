using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance {get; private set;}

    [Header("Experience")]
    [SerializeField] private TextMeshProUGUI _experienceText;
    [SerializeField] private TextMeshProUGUI _levelText;
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

    public void ShowScreen(GameObject screenToShow)
    {
        screenToShow.SetActive(true);   
    }

    public void HideScreen(GameObject screenToHide)
    {
        screenToHide.SetActive(false);
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }
    
    private void HandleLevelUp(int newLevel)
    {
        _levelText.text = "NIVEL: " + newLevel.ToString();
    }
    
    
}
