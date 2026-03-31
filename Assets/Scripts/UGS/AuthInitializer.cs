using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UGS
{
    public class AuthInitializer : MonoBehaviour
    {
        [SerializeField] private Button signInAnonymousButton;
        [SerializeField] private Button signUpWithUsernamePassword;
        [SerializeField] private Button signInWithUsernamePassword;
        [SerializeField] private TMP_Text HelpText;

        [Header("Username & Password")]
        [SerializeField] private TMP_InputField usernameInputField;
        [SerializeField] private TMP_InputField passwordInputField;
        
        async void Start()
        {
            signInAnonymousButton.interactable = false;
            signUpWithUsernamePassword.interactable = false;
            signInWithUsernamePassword.interactable = false;
            await UnityServices.InitializeAsync();
            signInAnonymousButton.interactable = true;
            signUpWithUsernamePassword.interactable = true;
            signInWithUsernamePassword.interactable = true;
            
            
            signInAnonymousButton.onClick.AddListener(SignIn);
            
            signUpWithUsernamePassword.onClick.AddListener(() =>
            {
                SignUpWithUsernamePasswordAsync(usernameInputField.text, passwordInputField.text);
            });
            
            
            signInWithUsernamePassword.onClick.AddListener(() =>
            {
                SignInWithUsernamePasswordAsync(usernameInputField.text, passwordInputField.text);
            });
        }
        private async void SignIn()
        {
            signInAnonymousButton.interactable = false;
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

        private async void SignUpWithUsernamePasswordAsync(string username, string password)
        {
            try
            {
                await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
                Debug.Log("SignUp is successful.");
                HelpText.text = "SignUp is successful.";
                
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
                HelpText.text = ex.Message;
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
                HelpText.text = ex.Message;
            }
        }
        
        
        private async void SignInWithUsernamePasswordAsync(string username, string password)
        {
            try
            {
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
                Debug.Log("SignIn is successful.");
                Debug.Log("Player ID: " + AuthenticationService.Instance.PlayerId);
                HelpText.text = "SignIn is successful.";
                await Task.Delay(4000);
                LoadNextScene();
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
                HelpText.text = ex.Message;
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
                HelpText.text = ex.Message;
            }
        }

        
        private void LoadNextScene()
        {
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentIndex + 1);
        }
    }
}