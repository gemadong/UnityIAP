using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Google;
using PlayNANOO;
using Facebook.Unity;
#if UNITY_IOS
using System.Text;
using AppleAuth;
using AppleAuth.Native;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
#endif

public class PlayNAOOLogin : MonoBehaviour
{
#if UNITY_IOS
    IAppleAuthManager _appleAuthManager;
#endif
    Plugin plugin;
    GoogleSignInConfiguration googleSignInConfiguration;
    bool googleIsLogin = false;


    void Start()
    {
        plugin = Plugin.GetInstance();
        googleSignInConfiguration = new GoogleSignInConfiguration
        {
            RequestIdToken = true,
            WebClientId = "232879415191-t7kqtngfpofp8ct3f2lqeoe80p8oqlnk.apps.googleusercontent.com",
        };
        
        if (!FB.IsInitialized)
        {
            FB.Init(OnFBInitComplete, OnFBHideUnity);
        }
        else
        {
            FB.ActivateApp();
        }
#if UNITY_IOS
        if (AppleAuthManager.IsCurrentPlatformSupported)
        {
            var deserializer = new PayloadDeserializer();
            _appleAuthManager = new AppleAuthManager(deserializer);
        }
#endif
    }
#if UNITY_IOS
    private void Update()
    {
        _appleAuthManager?.Update();
    }
#endif


#region FacebookLogin
    void OnFBInitComplete()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    void OnFBHideUnity(bool isShow)
    {
        if (!isShow)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public void FacebookSignIn()
    {
        var para = new List<string>() { "public_profile", "email" };
        FB.LogInWithReadPermissions(para, FacebookAuthCallback);
    }

    void FacebookAuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            Debug.Log("result.RawResult : "+result.RawResult);
            
            plugin.AccountSocialSignIn(result.AccessToken.TokenString, Configure.PN_ACCOUNT_FACEBOOK, (status, errorCode, jsonString, values) =>
            {
                if (status.Equals(Configure.PN_API_STATE_SUCCESS))
                {
                    Debug.Log(values["access_token"].ToString());
                    Debug.Log(values["refresh_token"].ToString());
                    Debug.Log(values["uuid"].ToString());
                    Debug.Log(values["openID"].ToString());
                    Debug.Log(values["nickname"].ToString());
                    Debug.Log(values["linkedID"].ToString());
                    Debug.Log(values["linkedType"].ToString());
                    Debug.Log(values["country"].ToString());
                }
                else
                {
                    if (values != null)
                    {
                        if (values["ErrorCode"].ToString() == "30007")
                        {
                            Debug.Log(values["WithdrawalKey"].ToString());
                        }
                        else
                        {
                            Debug.Log("Fail");
                        }
                    }
                    else
                    {
                        Debug.Log("Fail");
                    }
                }
            });
        }
        else
        {
            Debug.Log("Login Cancel");
        }
    }
#endregion

#region GoogleLogin
    public void SignIn()
    {
        GoogleSignIn.Configuration = googleSignInConfiguration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<System.Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    Debug.Log("Got Error: " + error.Status + " " + error.Message);
                }
                else
                {
                    Debug.Log("Got Unexpected Exception?!?" + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            Debug.Log("Canceled");
        }
        else
        {
            StartCoroutine(SocialLogin(task.Result.IdToken));
        }
    }

    private string _storedToken;

    IEnumerator SocialLogin(string token)
    {
        yield return new WaitForEndOfFrame();

        Login(token);

        yield break;
    }
    private void Login(string token)
    {
        plugin.AccountSocialSignIn(token, Configure.PN_ACCOUNT_GOOGLE, (status, errorCode, jsonString, values) =>
        {
            Debug.Log("Login Status : " + status);
            if (status == Configure.PN_API_STATE_SUCCESS)
            {
                googleIsLogin = true;
                Debug.Log(values["access_token"].ToString());
                Debug.Log(values["refresh_token"].ToString());
                Debug.Log(values["uuid"].ToString());
                Debug.Log(values["openID"].ToString());
                Debug.Log(values["nickname"].ToString());
                Debug.Log(values["linkedID"].ToString());
                Debug.Log(values["linkedType"].ToString());
                Debug.Log(values["country"].ToString());
            }
            else
            {
                if (values != null)
                {
                    if (values["ErrorCode"].ToString() == "30007")
                    {
                        Debug.Log(values["WithdrawalKey"].ToString());
                    }
                    else
                    {
                        Debug.Log("Fail");
                    }
                }
                else
                {
                    Debug.Log("Fail");
                }
            }
        });
    }
#endregion

#region TokenLogin
    public void TokenLogin()
    {
        plugin.AccountTokenSignIn((status, errorCode, jsonString, values) => {
            if (status.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                Debug.Log(values["access_token"].ToString());
                Debug.Log(values["refresh_token"].ToString());
                Debug.Log(values["uuid"].ToString());
                Debug.Log(values["openID"].ToString());
                Debug.Log(values["nickname"].ToString());
                Debug.Log(values["linkedID"].ToString());
                Debug.Log(values["linkedType"].ToString());
                Debug.Log(values["country"].ToString());
            }
            else
            {
                if (values != null)
                {
                    if (values["ErrorCode"].ToString() == "30007")
                    {
                        Debug.Log(values["WithdrawalKey"].ToString());
                    }
                    else if(values["ErrorCode"].ToString() == "30002")
                    {
                        TokenRefresh();
                    }
                    else
                    {
                        Debug.Log("Fail");
                    }
                }
                else
                {
                    Debug.Log("Fail");
                }
            }
        });
    }
#endregion

#region TokenOut
    public void TokenLogOut()
    {
        plugin.AccountTokenSignOut((status, errorCode, jsonString, values) => {
            if (status.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                Debug.Log(values["status"].ToString());
                if (googleIsLogin) 
                { 
                    GoogleSignIn.DefaultInstance.SignOut();
                    googleIsLogin = false;
                }
            }
            else
            {
                Debug.Log("Fail");
            }
        });
    }
#endregion

#region AppleLogin
    public void AppleSignIn()
    {
#if UNITY_ANDROID
        plugin.OpenAppleID((status, errorCode, jsonString, values) =>
        {
            if (status == Configure.PN_API_STATE_SUCCESS)
            {
                Debug.Log(values["access_token"].ToString());
                Debug.Log(values["refresh_token"].ToString());
                Debug.Log(values["uuid"].ToString());
                Debug.Log(values["openID"].ToString());
                Debug.Log(values["nickname"].ToString());
                Debug.Log(values["linkedID"].ToString());
                Debug.Log(values["linkedType"].ToString());
                Debug.Log(values["country"].ToString());
            }
            else
            {
                if (values != null)
                {
                    if (values["ErrorCode"].ToString() == "30007")
                    {
                        Debug.Log(values["WithdrawalKey"].ToString());
                    }
                    else
                    {
                        Debug.Log("Fail");
                    }
                }
                else
                {
                    Debug.Log("Fail");
                }
            }
        });
#endif
#if UNITY_IOS
        var loginArgs = new AppleAuthLoginArgs();

        this._appleAuthManager.LoginWithAppleId(
            loginArgs,
            credential =>
            {
                var appleIdCredential = credential as IAppleIDCredential;
                if (appleIdCredential != null)
                {
                    string idToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken, 0, appleIdCredential.IdentityToken.Length);
                    plugin.AccountSocialSignIn(idToken, Configure.PN_ACCOUNT_APPLE_ID, (status, errorCode, jsonString, values) =>
                    {
                        if (status == Configure.PN_API_STATE_SUCCESS)
                        {
                            Debug.Log(values["access_token"].ToString());
                            Debug.Log(values["refresh_token"].ToString());
                            Debug.Log(values["uuid"].ToString());
                            Debug.Log(values["openID"].ToString());
                            Debug.Log(values["nickname"].ToString());
                            Debug.Log(values["linkedID"].ToString());
                            Debug.Log(values["linkedType"].ToString());
                            Debug.Log(values["country"].ToString());
                        }
                        else
                        {
                            if (values != null)
                            {
                                if (values["ErrorCode"].ToString() == "30007")
                                {
                                    Debug.Log(values["WithdrawalKey"].ToString());
                                }
                                else
                                {
                                    Debug.Log("Fail");
                                }
                            }
                            else
                            {
                                Debug.Log("Fail");
                            }
                        }
                    });
                }
            },
            error =>
            {
                // Something went wrong
                var authorizationErrorCode = error.GetAuthorizationErrorCode();
            }
            );
#endif
    }
#endregion

#region TokenRefresh
    public void TokenRefresh()
    {
        plugin.AccountTokenRefresh((status, errorCode, jsonString, values) => {
            if (status.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                Debug.Log(values["access_token"].ToString());
                Debug.Log(values["refresh_token"].ToString());
                Debug.Log(values["uuid"].ToString());
                Debug.Log(values["openID"].ToString());
                Debug.Log(values["nickname"].ToString());
                Debug.Log(values["linkedID"].ToString());
                Debug.Log(values["linkedType"].ToString());
                Debug.Log(values["country"].ToString());
                TokenLogin();
            }
            else
            {
                if (values != null)
                {
                    if (values["ErrorCode"].ToString() == "30007")
                    {
                        Debug.Log(values["WithdrawalKey"].ToString());
                    }
                    else
                    {
                        Debug.Log("Fail");
                    }
                }
                else
                {
                    Debug.Log("Fail");
                }
            }
        });
    }
#endregion
}

