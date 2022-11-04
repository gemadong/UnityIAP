using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Analytics;

public class Analytics : MonoBehaviour
{
    public void AnalyticsEnable(bool enable)
    {
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(enable);
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
