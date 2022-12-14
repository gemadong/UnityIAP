using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;
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
    [SerializeField] private GameObject _canvas;                    //사용중인 캔버스
    [SerializeField] private GameObject _acceptTermsWindowPrefeb;   //약관동의 창
    [SerializeField] private GameObject _inspectionWindowPrefeb;    //점검중 알림창
    [SerializeField] private Button _tapToStartButton;              //스타트 버튼
    [SerializeField] private Button _loginGoogleButtonPrefeb;       //구글 로그인 버튼
    [SerializeField] private Button _loginFacebookButtonPrefeb;     //페이스북 로그인 버튼
    [SerializeField] private Button _loginAppleButtonPrefeb;        //애플 로그인 버튼
    [SerializeField] private TextMeshProUGUI testText;              //테스트
#if UNITY_ANDROID
    [SerializeField] private GameObject _updateWindowPrefeb;        //업데이트 창
#endif

    Plugin plugin;                                                  //PlayNANOO 플러그인

#if UNITY_IOS
    void Awake()
    {
        //앱 추적 투명성
        //if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        //{
        //    ATTrackingStatusBinding.RequestAuthorizationTracking();
        //}
    }
#endif
    private void Start()
    {
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

        //Firebase Push (필요한 모든 종속 항목 이 시스템과 필요한 상태에 있는지 비동기적으로 확인하고 그렇지 않은 경우 수정을 시도)
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
        LoginButtonInstantiate();   //테스트!
#if UNITY_ANDROID
        //루트 검사
        //CheckIfDeviceIsRooted();
#endif
#if UNITY_IOS
        //RemoteConfig로 버전 검사.
        RemoteConfigGet();
#endif
    }

    private void Update()
    {
        //plugin.AccountCheckDuplicate(OnCheckAccountDuplicate);        //중복 로그인 검사 코드. 

        //Google Login
        Services();
#if UNITY_IOS
        //Apple Login
        _appleAuthManager?.Update();
#endif
    }

    //void OnCheckAccountDuplicate(bool isDuplicate)                    //중복 로그인 검사 코드.
    //{
    //    if (isDuplicate)
    //    {
    //        Debug.LogError("Duplicate connection has been detected.");
    //    }
    //}

    public void TapToStartButton()      //TestButton
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
            //루트모드 일때 대처
            testText.text = "Root Mode";
        }
        else
        {
            //루트모드 아닐때 다음 화면.
            testText.text = "No Root";
            RemoteConfigGet();
        }

    }
    #endregion

    #region RemoteConfig
    RemoteConfig remoteConfig;      //PlayNANOO RemoteConfig에서 json으로 받아옴 (struct : 정보만 받아옴)

    void RemoteConfigGet()
    {
        //PlayNANOO Instance
        plugin = Plugin.GetInstance();

        plugin.RemoteConfig.Init("dbtest-remote-config-5EDF726F", (isSuccess) => {
            if (isSuccess)
            {
#if UNITY_ANDROID
                string json = plugin.RemoteConfig.GetJson("dbtest-remote-config-5EDF726F", "_remoteConfig").ToString();
                remoteConfig = JsonUtility.FromJson<RemoteConfig>(json);
#endif
#if UNITY_IOS
                string json = plugin.RemoteConfig.GetJson("dbtest-remote-config-5EDF726F", "_remoteConfigIOS").ToString();
                remoteConfig = JsonUtility.FromJson<RemoteConfig>(json);
#endif
                if (remoteConfig._isStart)
                {
                    testText.text = "Start!";
#if UNITY_ANDROID
                    if (remoteConfig._bundleVersion == Application.version)
                    {
                        testText.text = "Version Good";
                        HashCodeGeneration();                //Anti GashCode 검사.
                    }
                    else
                    {
                        testText.text = "No Version";
                        StartCoroutine(CheckForUpdate());   //인앱 업데이트,      앱사이트로 넘어가는 것도 구현. 
                    }
#endif
#if UNITY_IOS
                    AcceptTermsInstantiate();           //약관동의 화면
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
        if (remoteConfig._hashCode == result.SummaryHash)
        {
            //HashCode맞을때.
            testText.text = "Hash Code";
            AcceptTermsInstantiate();
        }
        else
        {
            //HashCode틀릴때.
            testText.text = "No Hash";
        }
    }
    #endregion

    #region InAppUpdate
    AppUpdateManager appUpdateManager;
    IEnumerator CheckForUpdate()
    {
        appUpdateManager = new AppUpdateManager();
        PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation = appUpdateManager.GetAppUpdateInfo();

        yield return appUpdateInfoOperation;
        if (appUpdateInfoOperation.IsSuccessful)
        {
            var appUpdateInfoResult = appUpdateInfoOperation.GetResult();

            if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateAvailable)
            {
                var appUpdateOptions = AppUpdateOptions.ImmediateAppUpdateOptions();
                StartCoroutine(StartImmediateUpdate(appUpdateInfoResult, appUpdateOptions));
            }
            else
            {
                GameObject updateWindowPrefeb = Instantiate(_updateWindowPrefeb);
                updateWindowPrefeb.transform.SetParent(_canvas.transform, false);
                Button upDateButten = updateWindowPrefeb.transform.Find("UpDateButton").gameObject.GetComponent<Button>();
                upDateButten.onClick.AddListener(UpDateButton);
            }
        }
        else
        {
            GameObject updateWindowPrefeb = Instantiate(_updateWindowPrefeb);
            updateWindowPrefeb.transform.SetParent(_canvas.transform, false);
            Button upDateButten = updateWindowPrefeb.transform.Find("UpDateButton").gameObject.GetComponent<Button>();
            upDateButten.onClick.AddListener(UpDateButton);
        }
    }
    IEnumerator StartImmediateUpdate(AppUpdateInfo appUpdateInfo_i, AppUpdateOptions appUpdateOptions_i)
    {
        var startUpdateRequest = appUpdateManager.StartUpdate(appUpdateInfo_i, appUpdateOptions_i);
        yield return startUpdateRequest;
    }
    void UpDateButton()
    {
        Debug.Log("앱 스토어로!!");
    }
    #endregion
#endif

    #region AccepTerms
    GameObject acceptTermsWindow;       //약관동의 창
    Toggle _acceptTermsTiggle;          //약관동의 티글
    Toggle _personalDataTiggle;         //개인정보 티글
    Toggle _pushTiggle;                 //푸쉬알림 티글
    Toggle _pushNightTiggle;            //밤에푸쉬 티글
    Button _startButton;                //스타트 버튼
    Button _allAgreeButton;             //전부 동의 버튼

    bool isfcmEnabled = true;           //푸쉬 선택
    bool isnightEnabled = true;         //밤 푸쉬 선택

    void AcceptTermsInstantiate()
    {
        if (PlayerPrefs.GetInt("FirstAcceptTerms", 0) == 0)
        {
            testText.text = "AccepTerms";
            acceptTermsWindow = Instantiate(_acceptTermsWindowPrefeb, _canvas.transform);
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
#if UNITY_IOS
        AdSettings.SetAdvertiserTrackingEnabled(true);
#endif
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        isfcmEnabled = _pushTiggle.isOn;
        isnightEnabled = _pushNightTiggle.isOn;
        PlayerPrefs.SetInt("FirstAcceptTerms", 1);
        TokenLogin();
        AcceptTermsWindowDestroy();
    }
    void AcceptTermsAllAgreeButton()
    {
#if UNITY_IOS
        AdSettings.SetAdvertiserTrackingEnabled(true);
#endif
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
    Button loginGoogleButton;           //생성된 로그인 버튼.
    Button loginFacebookButton;         //생성된 로그인 버튼.
    Button loginAppleButton;            //생성된 로그인 버튼.
    void LoginButtonInstantiate()
    {
        loginGoogleButton = Instantiate(_loginGoogleButtonPrefeb, _canvas.transform);
        loginGoogleButton.onClick.AddListener(GoogleSignInButton);
        loginFacebookButton = Instantiate(_loginFacebookButtonPrefeb, _canvas.transform);
        loginFacebookButton.onClick.AddListener(FacebookSignIn);
        loginAppleButton = Instantiate(_loginAppleButtonPrefeb, _canvas.transform);
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
    public void FacebookSignIn()
    {
        if (!FB.IsInitialized) FB.Init(OnFBInitComplete, OnFBHideUnity);
        else FB.ActivateApp();


        StartCoroutine(FacebookLogin());
    }

    IEnumerator FacebookLogin()
    {
        yield return new WaitForEndOfFrame();

        var para = new List<string>() { "public_profile", "email" };
        FB.LogInWithReadPermissions(para, FacebookAuthCallback);

        yield break;
    }
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
                    FB.LogOut();
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
    Queue<bool> queue = new Queue<bool>();
    string token;

    public void GoogleSignInButton()
    {
        //queue = new Queue<bool>();
        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            //기본 로그인에는 false로 설정
            UseGameSignIn = false,
            //요청 ID 토큰, 동의가 필요.
            RequestIdToken = true,
            //이 앱과 연결된 웹 클라이언트 ID(인증 코드 또는 ID 토큰을 요청하는 데 필요)
            WebClientId = "968290012768-842bblbv8nh3p7fansm6vr8fqr0q2saf.apps.googleusercontent.com",
        };
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }
    void Services()
    {
        while (queue.Count > 0)
        {
            //Queue의 시작에서 제거
            bool isResult = queue.Dequeue();
            if (isResult)
            {
                plugin.AccountSocialSignIn(token, Configure.PN_ACCOUNT_GOOGLE, (status, errorCode, jsonString, values) =>
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

                        GoogleSignIn.DefaultInstance.SignOut();         //로그 아웃
                        GoogleSignIn.DefaultInstance.Disconnect();      //인스턴스와 끊기
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
                Debug.Log("Fail");
            }
        }
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    Debug.Log("Got Error: " + error.Status + " " + error.Message);
                    queue.Enqueue(false);
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
            token = task.Result.IdToken;
            queue.Enqueue(true);
        }
    }

    #endregion

    #region AppleLogin
#if UNITY_IOS
    IAppleAuthManager _appleAuthManager;
#endif

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
        if (AppleAuthManager.IsCurrentPlatformSupported)
        {
            var deserializer = new PayloadDeserializer();
            _appleAuthManager = new AppleAuthManager(deserializer);
        }


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
            testText.text = values["ErrorCode"].ToString();
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
                Button tapToStartButton = Instantiate(_tapToStartButton, _canvas.transform);
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
        //string deviceToken;
        //IEnumerator RequestAuthorization()
        //{
        //    Debug.Log("Start Coroutine!!!");
        //    var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge;
        //    using (var req = new AuthorizationRequest(authorizationOption, true))
        //    {
        //        while (!req.IsFinished)
        //        {
        //            yield return null;
        //        };

        //        string res = "\n RequestAuthorization:";
        //        res += "\n finished: " + req.IsFinished;
        //        res += "\n granted :  " + req.Granted;
        //        res += "\n error:  " + req.Error;
        //        res += "\n deviceToken:  " + req.DeviceToken;
        //        deviceToken = req.DeviceToken;
        //        IOSToken(deviceToken);
        //    }
        //}

        //public void TokenSaveIOS()
        //{
        //    NotificationServices.RegisterForNotifications(NotificationType.Alert | NotificationType.Badge | NotificationType.Sound, true);
        //    StartCoroutine(RequestAuthorization());

        //}
        //void IOSToken(string deviceToken)
        //{
        //    Debug.Log("IOS Token Click");
        //    SaveToken(deviceToken, isfcmEnabled, isnightEnabled);
        //}
        //void SaveToken(string token, bool isEnabled, bool isNightEnabled)
        //{
        //    Debug.Log("SaveToken Start!!!");
        //    Debug.Log("isFCMEnable :" + isEnabled);
        //    Debug.Log("isNightEnabled : " + isNightEnabled);
        //    plugin.PushNotification.Save(token, true, true, (status, error, jsonString, values) =>
        //    {
        //        if (status.Equals(Configure.PN_API_STATE_SUCCESS)) Debug.Log("Success Good!!!");
        //        else Debug.Log("Fail");
        //    });
        //    Debug.Log("SaveToken End!!!");
        //}
#endif
    public void ChangeToken(bool isEnabled, bool isNightEnabled)
    {
        plugin.PushNotification.Change(isEnabled, isNightEnabled, (status, errorCode, jsonString, values) =>
        {
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