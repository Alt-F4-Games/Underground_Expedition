using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UGS
{
    public class AuthInitializer : MonoBehaviour
    {
        [SerializeField] private Button signInButton;
        
        async void Start()
        {
            signInButton.interactable = false;
            await UnityServices.InitializeAsync();
            signInButton.interactable = true;
            signInButton.onClick.AddListener(SignIn);
        }
        private async void SignIn()
        {
            signInButton.interactable = false;
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
            
            Debug.Log("Player ID: " + AuthenticationService.Instance.PlayerId);
            await Task.Delay(4000);
            Debug.Log("Sign In Complete");
            LoadNextScene();
        }

        private void LoadNextScene()
        {
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentIndex + 1);
        }
    }
}