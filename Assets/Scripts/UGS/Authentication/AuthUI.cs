using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UGS.Authentication
{
    public class AuthUI : MonoBehaviour
    {
        [SerializeField] private Button signInAnonymousButton;
        [SerializeField] private Button signUpButton;
        [SerializeField] private Button signInButton;
        [SerializeField] private Button hidePasswordButton;
        private bool isPasswordHidden = true;

        [Header("Inputs")]
        [SerializeField] private TMP_InputField usernameInputField;
        [SerializeField] private TMP_InputField passwordInputField;

        [SerializeField] private TMP_Text helpText;

        private void Start()
        {
            signInAnonymousButton.onClick.AddListener(async () => await HandleAnonymousSignIn());
            signUpButton.onClick.AddListener(async () => await HandleSignUp());
            signInButton.onClick.AddListener(async () => await HandleSignIn());
            hidePasswordButton.onClick.AddListener(HidePassword);
        }

        private async Task HandleAnonymousSignIn()
        {
            try
            {
                await AuthManager.Instance.SignInAnonymously();
                LoadNextScene();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                helpText.text = ex.Message;
            }
        }

        private async Task HandleSignUp()
        {
            try
            {
                await AuthManager.Instance.SignUp(usernameInputField.text, passwordInputField.text);

                helpText.text = "SignUp successful";
            
                LoadNextScene();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                helpText.text = ex.Message;
            }
        }

        private async Task HandleSignIn()
        {
            try
            {
                await AuthManager.Instance.SignIn(usernameInputField.text, passwordInputField.text);

                helpText.text = "SignIn successful";
                LoadNextScene();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                helpText.text = ex.Message;
            }
        }

        private void LoadNextScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        private void HidePassword()
        {
            isPasswordHidden = !isPasswordHidden;

            passwordInputField.contentType = isPasswordHidden 
                ? TMP_InputField.ContentType.Password 
                : TMP_InputField.ContentType.Standard;

            passwordInputField.ForceLabelUpdate();
        }
    }
}