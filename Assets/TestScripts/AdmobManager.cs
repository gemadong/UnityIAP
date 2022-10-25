using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;


public class AdmobManager : MonoBehaviour
{
    public bool isTestMode; 
    public Text LogText; 
    public Button RewardAdsBtn;


    void Start()
    {
        var requestConfiguration = new RequestConfiguration
           .Builder()
           .SetTestDeviceIds(new List<string>() { "AE89B9952BD4F2C9" }) // test Device ID
           .build();

        MobileAds.SetRequestConfiguration(requestConfiguration);

        LoadRewardAd();
    }

    void Update()
    {
        RewardAdsBtn.interactable = rewardAd.IsLoaded();
    }

    AdRequest GetAdRequest()
    {
        return new AdRequest.Builder().Build();
    }
    const string rewardTestID = "ca-app-pub-3940256099942544/5224354917";
#if UNITY_ANDROID
    const string rewardID = "";
#endif
#if UNITY_IOS
    const string rewardID = "";
#endif
    RewardedAd rewardAd;


    void LoadRewardAd()
    {
        rewardAd = new RewardedAd(isTestMode ? rewardTestID : rewardID);
        rewardAd.LoadAd(GetAdRequest());
        rewardAd.OnUserEarnedReward += (sender, e) =>
        {
            LogText.text = "?????? ???? ????";
        };
    }

    public void ShowRewardAd()
    {
        rewardAd.Show();
        LoadRewardAd();
    }

}
