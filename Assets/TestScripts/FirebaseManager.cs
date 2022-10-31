using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Messaging;
using Firebase.Analytics;
using PlayNANOO;
#if UNITY_IOS
using Unity.Notifications.iOS;
using NotificationServices = UnityEngine.iOS.NotificationServices;
using NotificationType = UnityEngine.iOS.NotificationType;

#endif

public class FirebaseManager : MonoBehaviour
{
    [SerializeField] private Text FcmEnable;
    [SerializeField] private Text NightEnable;
    Plugin plugin;
    bool isnightEnabled = true;
    bool isfcmEnabled = true;
    void Start()
    {
        plugin = Plugin.GetInstance();
        //FirebaseAnalytics.SetAnalyticsCollectionEnabled(true); //?????????? ????
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
                Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;


                //LogEvent("test");
                //LogEvent("param_test_int", "IntParam", 111);
                //LogEvent("param_test_float", "FloatParam", 2.11f);
                //LogEvent("param_test_string", "StringParam", "TEST");
                //LogEvent("param_test_array",
                //    new Parameter(FirebaseAnalytics.ParameterCharacter, "warrior"),
                //    new Parameter(FirebaseAnalytics.ParameterLevel, 5));

            }
            else
            {
                Debug.LogError("Could not resolve all: " + task.Result);
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });
#if UNITY_IOS
        //NotificationServices.RegisterForNotifications(NotificationType.Alert | NotificationType.Badge | NotificationType.Sound, true);
        //IOSToken();
#endif
    }
#if UNITY_ANDROID
    IEnumerator AndroidToken(bool isEnabled, bool isNightEnabled)
    {
        var task = Firebase.Messaging.FirebaseMessaging.GetTokenAsync();
        while (!task.IsCompleted) yield return new WaitForEndOfFrame();

        SaveToken(task.Result,isEnabled, isNightEnabled);
    }
    void SaveToken(string token, bool isEnabled, bool isNightEnabled)
    {
        plugin.PushNotification.Save(token , isEnabled, isNightEnabled, (status, error, jsonString, values) =>
        {
            if (status.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                Debug.Log("Success");
            }
            else
            {
                Debug.Log("Fail");
            }
        });
    }
#endif
    public void IsNightEnable(bool isNightEnabled)
    {
        NightEnable.text = isNightEnabled.ToString();
        FcmEnable.text = isfcmEnabled.ToString();
        ChangeToken(isfcmEnabled, isNightEnabled);
        isnightEnabled = isNightEnabled;
    }

    

    public void ISFCMEnable(bool isEnabled)
    {
        NightEnable.text = isnightEnabled.ToString();
        FcmEnable.text = isEnabled.ToString();
        ChangeToken(isEnabled, isnightEnabled);
        isfcmEnabled = isEnabled;

    }
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
            Debug.Log("finished : " + req.IsFinished);
            Debug.Log("granted : " + req.Granted);
            Debug.Log("error : " + req.Error);
            Debug.Log("deviceToken  : " + deviceToken);
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
            if (status.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                Debug.Log("Success Good!!!");
            }
            else
            {
                Debug.Log("Fail");
            }
        });
        Debug.Log("SaveToken End!!!");
    }
#endif

    public void ChangeToken(bool isEnabled, bool isNightEnabled)
    {
        plugin.PushNotification.Change(isEnabled, isNightEnabled, (status, errorCode, jsonString, values) => {
            if (status.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                Debug.Log("Success");
            }
            else
            {
                Debug.Log("Fail");
            }
        });
    }


    /*public void ToKenOn()
    {
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
    }*/

    public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        UnityEngine.Debug.Log("Received Registration Token: " + token.Token);
    }

    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
    }


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
   
}
