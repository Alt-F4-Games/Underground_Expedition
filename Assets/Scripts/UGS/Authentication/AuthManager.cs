using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace UGS.Authentication
{
    public class AuthManager : MonoBehaviour
    {
        public static AuthManager Instance;

        private async void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                await UnityServices.InitializeAsync();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public bool IsSignedIn => AuthenticationService.Instance.IsSignedIn;

        public async Task SignUp(string username, string password)
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            Debug.Log("SignUp successful. PlayerID: " + AuthenticationService.Instance.PlayerId);
        }

        public async Task SignIn(string username, string password)
        {
            if (IsSignedIn)
            {
                AuthenticationService.Instance.SignOut();
            }

            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            Debug.Log("SignIn successful. PlayerID: " + AuthenticationService.Instance.PlayerId);
        }

        public async Task SignInAnonymously()
        {
            if (!IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Anonymous SignIn. PlayerID: " + AuthenticationService.Instance.PlayerId);
            }
        }

        public async Task SignOut()
        {
            if (IsSignedIn)
            {
                AuthenticationService.Instance.SignOut();
                Debug.Log("Signed out");
                await Task.CompletedTask;
            }
        }
    }
}
