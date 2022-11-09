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
using CodeStage.AntiCheat.Genuine.CodeHash;
using System.IO;
using TMPro;
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
using Unity.Advertisement.IosSupport;
#endif

public class TestScene : MonoBehaviour
{
    [SerializeField] private GameObject _canvas;
    [SerializeField] private GameObject _acceptTermsWindowPrefeb;
    [SerializeField] private GameObject _inspectionWindowPrefeb;
    [SerializeField] private Button _tapToStartButton;
    [SerializeField] private Button _loginGoogleButtonPrefeb;
    [SerializeField] private Button _loginFacebookButtonPrefeb;
    [SerializeField] private Button _loginAppleButtonPrefeb;
    [SerializeField] private TextMeshProUGUI testText;
#if UNITY_ANDROID
    [SerializeField] private GameObject _updateWindowPrefeb;
#endif

    Toggle _acceptTermsTiggle;
    Toggle _personalDataTiggle;
    Toggle _pushTiggle;
    Toggle _pushNightTiggle;
    Button _startButton;
    Button _allAgreeButton;

    Plugin plugin;
    RemoteConfig remoteConfig;
    GoogleSignInConfiguration googleSignInConfiguration;

    bool googleIsLogin = false;
    bool isnightEnabled = true;
    bool isfcmEnabled = true;

#if UNITY_IOS
    IAppleAuthManager _appleAuthManager;
#endif
    void Awake()
    {
#if UNITY_IOS
        // Check the user's consent status.
        // If the status is undetermined, display the request request:
        if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            ATTrackingStatusBinding.RequestAuthorizationTracking();
        }
#endif
    }
    private void Start()
    {
        //PlayNANOO
        plugin = Plugin.GetInstance();
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

#if UNITY_ANDROID
        CheckIfDeviceIsRooted();
#endif
#if UNITY_IOS
        RemoteConfigGet();
#endif
    }

    private void Update()
    {
#if UNITY_IOS
        //Apple Login
        _appleAuthManager?.Update();
#endif
    }

    public void TapToStartButton()
    {
        testText.text = "Game Start!!!";
    }

#region RootCheck
    public static string GetData()
    {
        string result = "";

        if (Application.platform == RuntimePlatform.Android)
        {
            var osBuild = new AndroidJavaClass("android.os.Build");
            string brand = osBuild.GetStatic<string>("BRAND");
            string fingerPrint = osBuild.GetStatic<string>("FINGERPRINT");
            string model = osBuild.GetStatic<string>("MODEL");
            string menufacturer = osBuild.GetStatic<string>("MANUFACTURER");
            string device = osBuild.GetStatic<string>("DEVICE");
            string product = osBuild.GetStatic<string>("PRODUCT");

            result += Application.installerName;
            result += "/";
            result += Application.installMode.ToString();
            result += "/";
            result += Application.buildGUID;
            result += "/";
            result += "Genuine :" + Application.genuine;
            result += "/";
            result += "Rooted : " + isRooted();
            result += "/";
            result += "Model : " + model;
            result += "/";
            result += "Menufacturer : " + menufacturer;
            result += "/";
            result += "Device : " + device;
            result += "/";
            result += "Fingerprint : " + fingerPrint;
            result += "/";
            result += "Product : " + product;
        }
        else
        {
            result += Application.installerName;
            result += "/";
            result += Application.installMode.ToString();
            result += "/";
            result += Application.buildGUID;
            result += "/";
            result += "Genuine :" + Application.genuine;
            result += "/";
        }
        return result;
    }
    public static bool isRooted()
    {
        bool isRoot = false;

        if (Application.platform == RuntimePlatform.Android)
        {
            if (isRootedPrivate("/system/bin/su"))
                isRoot = true;
            if (isRootedPrivate("/system/xbin/su"))
                isRoot = true;
            if (isRootedPrivate("/system/app/SuperUser.apk"))
                isRoot = true;
            if (isRootedPrivate("/data/data/com.noshufou.android.su"))
                isRoot = true;
            if (isRootedPrivate("/sbin/su"))
                isRoot = true;
        }

        return isRoot;
    }
    public static bool isRootedPrivate(string path)
    {
        bool boolTemp = false;

        if (File.Exists(path))
        {
            boolTemp = true;
        }

        return boolTemp;
    }
    public void CheckIfDeviceIsRooted()
    {
        if (isRooted())
        {
            testText.text = "Root Mode";
        }
        else
        {
            testText.text = "No Root";
            RemoteConfigGet();
        }

    }
#endregion

#region RemoteConfig
    void RemoteConfigGet()
    {
        plugin.RemoteConfig.Init("dbtest-remote-config-5EDF726F", (isSuccess) => {
            if (isSuccess)
            {
#if UNITY_ANDROID
                string json = plugin.RemoteConfig.GetJson("dbtest-remote-config-5EDF726F", "_remoteConfig").ToString();
#endif
#if UNITY_IOS
                string json = plugin.RemoteConfig.GetJson("dbtest-remote-config-5EDF726F", "_remoteConfigIOS").ToString();
#endif
                remoteConfig = JsonUtility.FromJson<RemoteConfig>(json);
                if (remoteConfig._isStart)
                {
                    testText.text = "Start!";
#if UNITY_ANDROID
                    if (remoteConfig._bundleVersion == Application.version)
                    {
                        testText.text = "Version Good";
                        HashCodeGeneration();
                    }
                    else testText.text = "No Version";
#endif
#if UNITY_IOS
                    AcceptTermsInstantiate();
#endif
                }
                else testText.text = "Stop!";
            }
            else testText.text = "Json?!";
        });
    }
    #endregion

#if UNITY_ANDROID
#region HashCode
    void HashCodeGeneration()
    {
        if (!CodeHashGenerator.IsTargetPlatformCompatible()) return;
        CodeHashGenerator.HashGenerated += OnGotHash;
        CodeHashGenerator.Generate();
    }
    void OnGotHash(HashGeneratorResult result)
    {
        if (!result.Success) return;
        if (remoteConfig._hashCode == result.CodeHash)
        {
            testText.text = "Hash Code";
            AcceptTermsInstantiate();
        }
        else
        {
            testText.text = "No Hash";
        }
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

            if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateAvailable)
            {
                var appUpdateOptions = AppUpdateOptions.ImmediateAppUpdateOptions();
                var startUpdateRequest = appUpdateManager.StartUpdate(appUpdateInfoResult, appUpdateOptions);
                yield return startUpdateRequest;
            }
            else 
            {
                GameObject updateWindowPrefeb = Instantiate(_updateWindowPrefeb);
                updateWindowPrefeb.transform.SetParent(_canvas.transform, false);
                Button upDateButten = updateWindowPrefeb.transform.Find("UpDateButton").gameObject.GetComponent<Button>();
                upDateButten.onClick.AddListener(UpDateButton);
                //Accept Terms
                //if (PlayerPrefs.GetInt("FirstAcceptTerms", 0) == 0)
                //{
                //    acceptTermsWindow = Instantiate(_acceptTermsWindowPrefeb);
                //    acceptTermsWindow.transform.SetParent(_canvas.transform, false);
                //    ToggleFind(acceptTermsWindow);
                //}
                //else if (PlayerPrefs.GetInt("FirstAcceptTerms", 0) == 1) TokenLogin();
                //Debug.Log("NO Update");
            } 
        }
        else
        {
            GameObject updateWindowPrefeb = Instantiate(_updateWindowPrefeb);
            updateWindowPrefeb.transform.SetParent(_canvas.transform, false);
            Button upDateButten = updateWindowPrefeb.transform.Find("UpDateButton").gameObject.GetComponent<Button>();
            upDateButten.onClick.AddListener(UpDateButton);
            //Accept Terms
            //if (PlayerPrefs.GetInt("FirstAcceptTerms", 0) == 0)
            //{
            //    acceptTermsWindow = Instantiate(_acceptTermsWindowPrefeb);
            //    acceptTermsWindow.transform.SetParent(_canvas.transform, false);
            //    ToggleFind(acceptTermsWindow);
            //}
            //else if (PlayerPrefs.GetInt("FirstAcceptTerms", 0) == 1) TokenLogin();
            //Debug.Log("NO Update");
        }
    }
    void UpDateButton()
    {
        Debug.Log("??? ???? ??!!");
    }
#endif
#endregion
#endif

#region AccepTerms

    GameObject acceptTermsWindow;
    void AcceptTermsInstantiate()
    {
        if (PlayerPrefs.GetInt("FirstAcceptTerms", 0) == 0)
        {
            testText.text = "AccepTerms";
            acceptTermsWindow = Instantiate(_acceptTermsWindowPrefeb);
            acceptTermsWindow.transform.SetParent(_canvas.transform, false);
            ToggleFind(acceptTermsWindow);
        }
        else if (PlayerPrefs.GetInt("FirstAcceptTerms", 0) == 1)
        {
            testText.text = "AccepTerms Finish.";
            TokenLogin();
        }
    }
    void ToggleFind(GameObject acceptTermsWindow)
    {
        _acceptTermsTiggle = acceptTermsWindow.transform.Find("AcceptTermsTiggle").gameObject.GetComponent<Toggle>();
        _acceptTermsTiggle.onValueChanged.AddListener(delegate {
            StartButtonInteractable(_acceptTermsTiggle);
        });
        _personalDataTiggle = acceptTermsWindow.transform.Find("PersonalDataTiggle").gameObject.GetComponent<Toggle>();
        _personalDataTiggle.onValueChanged.AddListener(delegate {
            StartButtonInteractable(_personalDataTiggle);
        });
        _pushTiggle = acceptTermsWindow.transform.Find("PushTiggle").gameObject.GetComponent<Toggle>();
        _pushTiggle.onValueChanged.AddListener(delegate {
            PushNightTiggleEnable(_pushTiggle);
        });
        _pushNightTiggle = acceptTermsWindow.transform.Find("PushNightTiggle").gameObject.GetComponent<Toggle>();
        _startButton = acceptTermsWindow.transform.Find("Start").gameObject.GetComponent<Button>();
        _startButton.onClick.AddListener(AcceptTermsStartButton);
        _allAgreeButton = acceptTermsWindow.transform.Find("AllAgreeStart").gameObject.GetComponent<Button>();
        _allAgreeButton.onClick.AddListener(AcceptTermsAllAgreeButton);
    }
    void PushNightTiggleEnable(bool ison)
    {
        if (_pushTiggle.isOn)
        {
            _pushNightTiggle.interactable = true;
            _pushNightTiggle.isOn = true;
        }
        else
        {
            _pushNightTiggle.interactable = false;
            if (_pushNightTiggle.isOn) _pushNightTiggle.isOn = false;
        }
    }
    void StartButtonInteractable(bool ison)
    {
        if (_acceptTermsTiggle.isOn && _personalDataTiggle.isOn) _startButton.interactable = true;
        else _startButton.interactable = false;
    }
    void AcceptTermsStartButton()
    {
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        isfcmEnabled = _pushTiggle.isOn;
        isnightEnabled = _pushNightTiggle.isOn;
        PlayerPrefs.SetInt("FirstAcceptTerms", 1);
        TokenLogin();
        AcceptTermsWindowDestroy();
    }
    void AcceptTermsAllAgreeButton()
    {
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        isfcmEnabled = true;
        isnightEnabled = true;
        PlayerPrefs.SetInt("FirstAcceptTerms", 1);
        TokenLogin();
        AcceptTermsWindowDestroy();
    }
    void AcceptTermsWindowDestroy()
    {
        _acceptTermsTiggle = null;
        _personalDataTiggle = null;
        _pushTiggle = null;
        _pushNightTiggle = null;
        Destroy(acceptTermsWindow);
    }
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
        Destroy(loginGoogleButton.gameObject);
        Destroy(loginFacebookButton.gameObject);
        Destroy(loginAppleButton.gameObject);
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
        if (!FB.IsInitialized) FB.Init(OnFBInitComplete, OnFBHideUnity);
        else FB.ActivateApp();

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
                    LoginButtonDestroy();
                }
                else
                {
                    if (values != null)
                    {
                        if (values["ErrorCode"].ToString() == "30007")
                        {
                            Debug.Log(values["WithdrawalKey"].ToString());
                        }
                        else if (values["ErrorCode"].ToString() == "30006")
                        {
                            testText.text = "ErrorCode 30006";
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
        googleSignInConfiguration = new GoogleSignInConfiguration
        {
            RequestIdToken = true,
            WebClientId = "232879415191-t7kqtngfpofp8ct3f2lqeoe80p8oqlnk.apps.googleusercontent.com",
        };

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
                LoginButtonDestroy();
            }
            else
            {
                if (values != null)
                {
                    if (values["ErrorCode"].ToString() == "30007")
                    {
                        Debug.Log(values["WithdrawalKey"].ToString());
                    }
                    else if (values["ErrorCode"].ToString() == "30006")
                    {
                        testText.text = "ErrorCode 30006";
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
                LoginButtonDestroy();
            }
            else
            {
                if (values != null)
                {
                    if (values["ErrorCode"].ToString() == "30007")
                    {
                        Debug.Log(values["WithdrawalKey"].ToString());
                    }
                    else if (values["ErrorCode"].ToString() == "30006")
                    {
                        testText.text = "ErrorCode 30006";
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
                                else if (values["ErrorCode"].ToString() == "30006")
                                {
                                     testText.text = "ErrorCode 30006";
                                }
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
        plugin.AccountTokenInfo((status, errorCode, jsonString, values) => {
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
                Button tapToStartButton = Instantiate(_tapToStartButton);
                tapToStartButton.transform.SetParent(_canvas.transform, false);
                tapToStartButton.onClick.AddListener(TapToStartButton);
                testText.text = "Token Login";
            }
            else
            {
                if (values != null)
                {
                    if (values["ErrorCode"].ToString() == "30007") Debug.Log(values["WithdrawalKey"].ToString());
                    else if (values["ErrorCode"].ToString() == "30006")
                    {
                        testText.text = "ErrorCode 30006";
                    }
                    else if (values["ErrorCode"].ToString() == "30002") TokenRefresh();
                    else Debug.Log("Fail");
                }
                else Debug.Log("Fail");
                LoginButtonInstantiate();
                testText.text = "No Login";
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
[System.Serializable]
public struct RemoteConfig
{
    public bool _isStart;
#if UNITY_ANDROID
    public string _bundleVersion;
    public string _hashCode;
#endif
}