using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UGS.Authentication
{
    public class LogOutUI : MonoBehaviour
    {
        [SerializeField] private Button logoutButton;

        private void Start()
        {
            logoutButton.onClick.AddListener(async () => await HandleLogout());
        }

        private async Task HandleLogout()
        {
            await AuthManager.Instance.SignOut();
            SceneManager.LoadScene(0);
        }  
    }
}
