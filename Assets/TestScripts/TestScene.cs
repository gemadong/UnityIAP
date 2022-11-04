using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Google;
using PlayNANOO;
using Facebook.Unity;
using Firebase;
using Firebase.Analytics;
using Firebase.Messaging;
#if UNITY_ANDROID
using Google.Play.AppUpdate;
using Google.Play.Common;
#endif
#if UNITY_IOS
using System.Text;
using AppleAuth;
using AppleAuth.Native;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using Unity.Notifications.iOS;
using NotificationServices = UnityEngine.iOS.NotificationServices;
using NotificationType = UnityEngine.iOS.NotificationType;
#endif

public class TestScene : MonoBehaviour
{
    [SerializeField] private GameObject _canvas;
    [SerializeField] private GameObject _acceptTermsWindowPrefeb;
    [SerializeField] private Button _loginGoogleButtonPrefeb;
    [SerializeField] private Button _loginFacebookButtonPrefeb;
    [SerializeField] private Button _loginAppleButtonPrefeb;
    [SerializeField] private GameObject _tapToStartButtonPrefeb;
#if UNITY_ANDROID
    [SerializeField] private GameObject _updateWindowPrefeb;
#endif

    Toggle _acceptTermsTiggle;
    Toggle _personalDataTiggle;
    Toggle _pushTiggle;
    Toggle _pushNightTiggle;
    Button _startButton;

    GoogleSignInConfiguration googleSignInConfiguration;
    Plugin plugin;

    bool googleIsLogin = false;
    bool isnightEnabled = true;
    bool isfcmEnabled = true;

#if UNITY_IOS
    IAppleAuthManager _appleAuthManager;
#endif
    private void Start()
    {
        //PlayNANOO
        plugin = Plugin.GetInstance();
        
        //Google SignIn
        googleSignInConfiguration = new GoogleSignInConfiguration
        {
            RequestIdToken = true,
            WebClientId = "232879415191-t7kqtngfpofp8ct3f2lqeoe80p8oqlnk.apps.googleusercontent.com",
        };

        //Firebase Push
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseMessaging.TokenReceived += OnTokenReceived;
                FirebaseMessaging.MessageReceived += OnMessageReceived;
            }
            else
            {
                Debug.LogError("Could not resolve all: " + task.Result);
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });
        
        //Facebook Login
        if (!FB.IsInitialized) FB.Init(OnFBInitComplete, OnFBHideUnity);
        else FB.ActivateApp();

        //Accept Terms
        if (PlayerPrefs.GetInt("FirstAcceptTerms", 0) == 0)
        {
            acceptTermsWindow = Instantiate(_acceptTermsWindowPrefeb);
            acceptTermsWindow.transform.SetParent(_canvas.transform, false);
            ToggleFind(acceptTermsWindow);
        }
        else if(PlayerPrefs.GetInt("FirstAcceptTerms", 0) == 1) TokenLogin();

#if UNITY_ANDROID
        //InApp Update Check
        StartCoroutine(CheckForUpdate());
#endif
    }

    private void Update()
    {

        //Accept Terms Start Button Open
        if (_acceptTermsTiggle)
        {
            if (_acceptTermsTiggle.isOn && _personalDataTiggle.isOn) _startButton.interactable = true;
            else _startButton.interactable = false;
        }

#if UNITY_IOS
        //Apple Login
        _appleAuthManager?.Update();
#endif
    }

    public void TapToStartButton()
    {
        Debug.Log("Game Start!!!");
    }


    #region AccepTerms

    GameObject acceptTermsWindow;
    void ToggleFind(GameObject acceptTermsWindow)
    {
        _acceptTermsTiggle = acceptTermsWindow.transform.Find("AcceptTermsTiggle").gameObject.GetComponent<Toggle>();
        _personalDataTiggle = acceptTermsWindow.transform.Find("PersonalDataTiggle").gameObject.GetComponent<Toggle>();
        _pushTiggle = acceptTermsWindow.transform.Find("PushTiggle").gameObject.GetComponent<Toggle>();
        _pushNightTiggle = acceptTermsWindow.transform.Find("PushNightTiggle").gameObject.GetComponent<Toggle>();
        _startButton = acceptTermsWindow.transform.Find("Start").gameObject.GetComponent<Button>();

    }
    void AcceptTermsWindowDestroy()
    {
        _acceptTermsTiggle = null;
        _personalDataTiggle = null;
        _pushTiggle = null;
        _pushNightTiggle = null;
        _startButton = null;
        Destroy(acceptTermsWindow);
    }
    public void AcceptTermsStartButton()
    {
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        isfcmEnabled = _pushTiggle.isOn;
        isnightEnabled = _pushNightTiggle.isOn;
        PlayerPrefs.SetInt("FirstAcceptTerms", 1);
        TokenLogin();
        AcceptTermsWindowDestroy();
    }

    public void AcceptTermsAllAgreeButton()
    {
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        isfcmEnabled = true;
        isnightEnabled = true;
        PlayerPrefs.SetInt("FirstAcceptTerms", 1);
        TokenLogin();
        AcceptTermsWindowDestroy();
    }
    #endregion

    #region InAppUpdate
#if UNITY_ANDROID
    IEnumerator CheckForUpdate()
    {
        AppUpdateManager appUpdateManager = new AppUpdateManager();
        PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation = appUpdateManager.GetAppUpdateInfo();

        yield return appUpdateInfoOperation;
        if (appUpdateInfoOperation.IsSuccessful)
        {
            var appUpdateInfoResult = appUpdateInfoOperation.GetResult();

            if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateAvailable) _updateWindowPrefeb.SetActive(true);
            else Debug.Log("NO Update");
        }
        else Debug.Log("NO Update");
    }
    public void UpdateTest()
    {
        StartCoroutine(StartUpdateWindow());
    }

    public IEnumerator StartUpdateWindow()
    {
        _updateWindowPrefeb.SetActive(false);
        AppUpdateManager appUpdateManager = new AppUpdateManager();
        PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation = appUpdateManager.GetAppUpdateInfo();

        yield return appUpdateInfoOperation;
        if (appUpdateInfoOperation.IsSuccessful)
        {
            var appUpdateInfoResult = appUpdateInfoOperation.GetResult();

            if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateAvailable)
            {
                var appUpdateOptions = AppUpdateOptions.ImmediateAppUpdateOptions();
                var startUpdateRequest = appUpdateManager.StartUpdate(appUpdateInfoResult, appUpdateOptions);
                yield return startUpdateRequest;
            }
            else Debug.Log("NO Update");

        }
        else Debug.Log("NO Update");
    }
#endif
    #endregion

    #region LoginButton
    Button loginGoogleButton;
    Button loginFacebookButton;
    Button loginAppleButton;
    void LoginButtonInstantiate()
    {
        loginGoogleButton = Instantiate(_loginGoogleButtonPrefeb);
        loginGoogleButton.transform.SetParent(_canvas.transform, false);
        loginGoogleButton.onClick.AddListener(GoogleSignInButton);
        loginFacebookButton = Instantiate(_loginFacebookButtonPrefeb);
        loginFacebookButton.transform.SetParent(_canvas.transform, false);
        loginFacebookButton.onClick.AddListener(FacebookSignIn);
        loginAppleButton = Instantiate(_loginAppleButtonPrefeb);
        loginAppleButton.transform.SetParent(_canvas.transform, false);
        loginAppleButton.onClick.AddListener(AppleSignInButton);
    }
    void LoginButtonDestroy()
    {
        Destroy(loginGoogleButton);
        Destroy(loginFacebookButton);
        Destroy(loginAppleButton);
    }
    #endregion
    
    #region FacebookLogin
    void OnFBInitComplete()
    {
        if (FB.IsInitialized) FB.ActivateApp();
        else Debug.Log("Failed to Initialize the Facebook SDK");
    }

    void OnFBHideUnity(bool isShow)
    {
        if (!isShow) Time.timeScale = 0;
        else Time.timeScale = 1;
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
            Debug.Log("result.RawResult : " + result.RawResult);

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
                    LoginButtonInstantiate();
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
    public void GoogleSignInButton()
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
                LoginButtonInstantiate();
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

    #region AppleLogin
    public void AppleSignInButton()
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
                LoginButtonInstantiate();
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
                                if (values["ErrorCode"].ToString() == "30007") Debug.Log(values["WithdrawalKey"].ToString());
                                else Debug.Log("Fail");
                            }
                            else Debug.Log("Fail");
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
                _tapToStartButtonPrefeb.SetActive(true);
            }
            else
            {
                if (values != null)
                {
                    if (values["ErrorCode"].ToString() == "30007") Debug.Log(values["WithdrawalKey"].ToString());
                    else if (values["ErrorCode"].ToString() == "30002") TokenRefresh();
                    else Debug.Log("Fail");
                }
                else Debug.Log("Fail");
                LoginButtonInstantiate();
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
            else Debug.Log("Fail");
        });
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
                    if (values["ErrorCode"].ToString() == "30007") Debug.Log(values["WithdrawalKey"].ToString());
                    else Debug.Log("Fail");
                }
                else Debug.Log("Fail");
            }
        });
    }
    #endregion

    #region Analytics
    public void LogEvent(string eventName)
    {
        FirebaseAnalytics.LogEvent(eventName);
    }
    public void LogEvent(string eventName, string paramName, int paramValue)
    {
        FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
    }
    public void LogEvent(string eventName, string paramName, float paramValue)
    {
        FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
    }
    public void LogEvent(string eventName, string paramName, string paramValue)
    {
        FirebaseAnalytics.LogEvent(eventName, paramName, paramValue);
    }
    public void LogEvent(string eventName, params Parameter[] paramArray)
    {
        FirebaseAnalytics.LogEvent(eventName, paramArray);
    }
    #endregion

    #region PushMessaging
#if UNITY_ANDROID
    public void StartSaveToken()
    {
        StartCoroutine(AndroidToken(isnightEnabled, isfcmEnabled));
    }

    IEnumerator AndroidToken(bool isEnabled, bool isNightEnabled)
    {
        var task = FirebaseMessaging.GetTokenAsync();
        while (!task.IsCompleted) yield return new WaitForEndOfFrame();

        SaveToken(task.Result, isEnabled, isNightEnabled);
    }

    void SaveToken(string token, bool isEnabled, bool isNightEnabled)
    {
        plugin.PushNotification.Save(token, isEnabled, isNightEnabled, (status, error, jsonString, values) =>
        {
            if (status.Equals(Configure.PN_API_STATE_SUCCESS)) Debug.Log("SaveToken Success !!");
            else Debug.Log("Fail");
        });
    }
#endif
#if UNITY_IOS
    string deviceToken;
    IEnumerator RequestAuthorization()
    {
        Debug.Log("Start Coroutine!!!");
        var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge;
        using (var req = new AuthorizationRequest(authorizationOption, true))
        {
            while (!req.IsFinished)
            {
                yield return null;
            };

            string res = "\n RequestAuthorization:";
            res += "\n finished: " + req.IsFinished;
            res += "\n granted :  " + req.Granted;
            res += "\n error:  " + req.Error;
            res += "\n deviceToken:  " + req.DeviceToken;
            deviceToken = req.DeviceToken;
            IOSToken(deviceToken);
        }
    }

    public void TokenSaveIOS()
    {
        NotificationServices.RegisterForNotifications(NotificationType.Alert | NotificationType.Badge | NotificationType.Sound, true);
        StartCoroutine(RequestAuthorization());
        
    }
    void IOSToken(string deviceToken)
    {
        Debug.Log("IOS Token Click");
        SaveToken(deviceToken, isfcmEnabled, isnightEnabled);
    }
    void SaveToken(string token, bool isEnabled, bool isNightEnabled)
    {
        Debug.Log("SaveToken Start!!!");
        Debug.Log("isFCMEnable :" + isEnabled);
        Debug.Log("isNightEnabled : " + isNightEnabled);
        plugin.PushNotification.Save(token, true, true, (status, error, jsonString, values) =>
        {
            if (status.Equals(Configure.PN_API_STATE_SUCCESS)) Debug.Log("Success Good!!!");
            else Debug.Log("Fail");
        });
        Debug.Log("SaveToken End!!!");
    }
#endif
    public void ChangeToken(bool isEnabled, bool isNightEnabled)
    {
        plugin.PushNotification.Change(isEnabled, isNightEnabled, (status, errorCode, jsonString, values) => {
            if (status.Equals(Configure.PN_API_STATE_SUCCESS)) Debug.Log("Success");
            else Debug.Log("Fail");
        });
    }
    public void IsNightEnable(bool isNightEnabled)
    {
        ChangeToken(isfcmEnabled, isNightEnabled);
        isnightEnabled = isNightEnabled;
    }
    public void ISFCMEnable(bool isEnabled)
    {
        ChangeToken(isEnabled, isnightEnabled);
        isfcmEnabled = isEnabled;

    }
    public void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        Debug.Log("Received Registration Token: " + token.Token);
    }
    public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("Received a new message from: " + e.Message.From);
    }
    #endregion

    public void ExitButton()
    {
        Application.Quit();
    }
}
