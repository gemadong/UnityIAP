using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using Google.Play.AppUpdate;
using Google.Play.Common;
#endif

public class InAppUpdate : MonoBehaviour
{
    [SerializeField] private GameObject _updateWindow;
#if UNITY_ANDROID
    private void Start()
    {
        StartCoroutine(CheckForUpdate());
    }

    public IEnumerator CheckForUpdate()
    {
        AppUpdateManager appUpdateManager = new AppUpdateManager();
        PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation =
          appUpdateManager.GetAppUpdateInfo();

        // Wait until the asynchronous operation completes.
        yield return appUpdateInfoOperation;
        if (appUpdateInfoOperation.IsSuccessful)
        {
            var appUpdateInfoResult = appUpdateInfoOperation.GetResult();

            if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateAvailable)
            {
                _updateWindow.SetActive(true);
            }
            else
            {
                Debug.Log("NO Update");
            }

        }
        else
        {
            Debug.Log("NO Update");
        }
    }
    public void UpdateTest()
    {
        StartCoroutine(StartUpdateWindow());
    }

    public IEnumerator StartUpdateWindow()
    {
        _updateWindow.SetActive(false);
        AppUpdateManager appUpdateManager = new AppUpdateManager();
        PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation =
          appUpdateManager.GetAppUpdateInfo();

        // Wait until the asynchronous operation completes.
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
                Debug.Log("NO Update");
            }

        }
        else
        {
            Debug.Log("NO Update");
        }
    }
#endif
}
