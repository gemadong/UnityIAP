using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Play.AppUpdate;
using Google.Play.Common;

public class InAppUpdate : MonoBehaviour
{


    public void UpdateTest()
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
}
