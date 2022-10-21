using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Google;
using PlayNANOO;

public class PlayNAOOLogin : MonoBehaviour
{
    Plugin plugin;
    GoogleSignInConfiguration googleSignInConfiguration;

    void Start()
    {
        plugin = Plugin.GetInstance();
        googleSignInConfiguration = new GoogleSignInConfiguration
        {
            RequestIdToken = true,
            WebClientId = "232879415191-t7kqtngfpofp8ct3f2lqeoe80p8oqlnk.apps.googleusercontent.com",
        };

        TokenLogin();
    }

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
            Debug.Log(task.Result.IdToken);

            StartCoroutine(SocialLogin(task.Result.IdToken));
        }
    }

    private string _storedToken;
    public void Login()
    {
        Login(_storedToken);
    }

    IEnumerator SocialLogin(string token)
    {
        yield return new WaitForEndOfFrame();

        Login(token);

        yield break;
    }

    private void Login(string token)
    {
        Debug.Log("Login Started");
        plugin.AccountSocialSignIn(token, Configure.PN_ACCOUNT_GOOGLE, (status, errorCode, jsonString, values) =>
        {
            Debug.Log("Login Status : " + status);
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
        Debug.Log("Login Called");
    }
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


    public void TokenLogOut()
    {
        plugin.AccountTokenSignOut((status, errorCode, jsonString, values) => {
            if (status.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                Debug.Log(values["status"].ToString());
            }
            else
            {
                Debug.Log("Fail");
            }
        });
    }

    public void AppleSignIn()
    {
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
    }
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
}

