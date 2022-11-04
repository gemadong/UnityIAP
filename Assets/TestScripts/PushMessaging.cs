using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Messaging;
using PlayNANOO;
#if UNITY_IOS
using Unity.Notifications.iOS;
using NotificationServices = UnityEngine.iOS.NotificationServices;
using NotificationType = UnityEngine.iOS.NotificationType;
#endif

public class PushMessaging : MonoBehaviour
{
    Plugin plugin;
    public bool isnightEnabled = true;
    public bool isfcmEnabled = true;
    void Start()
    {
        plugin = Plugin.GetInstance();
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
#if UNITY_IOS
        //NotificationServices.RegisterForNotifications(NotificationType.Alert | NotificationType.Badge | NotificationType.Sound, true);
        //IOSToken();
#endif
    }

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
            if (status.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                Debug.Log("SaveToken Success !!");
            }
            else
            {
                Debug.Log("Fail");
            }
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

    public void OnTokenReceived(object sender,TokenReceivedEventArgs token)
    {
        Debug.Log("Received Registration Token: " + token.Token);
    }

    public void OnMessageReceived(object sender,MessageReceivedEventArgs e)
    {
        Debug.Log("Received a new message from: " + e.Message.From);
    }
}
