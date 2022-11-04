using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AcceptTerms : MonoBehaviour
{
    [SerializeField] private PlayNANOOLogin playNANOOLogin;
    [SerializeField] private PushMessaging _pushMessaging;
    [SerializeField] private Analytics _analytics;
    [SerializeField] private GameObject _acceptTermsWindow;

    [SerializeField] private Toggle _acceptTermsTiggle;
    [SerializeField] private Toggle _personalDataTiggle;
    [SerializeField] private Toggle _pushTiggle;
    [SerializeField] private Toggle _pushNightTiggle;

    [SerializeField] private Button _startButton;

    private void Start()
    {
        if (PlayerPrefs.GetInt("FirstAcceptTerms", 0) == 0)
        {
            _acceptTermsWindow.SetActive(true);
        }
    }
    private void Update()
    {
        if (_acceptTermsTiggle.isOn && _personalDataTiggle.isOn) _startButton.interactable = true;
        else _startButton.interactable = false;
    }

    public void GameStartButton()
    {
        _analytics.AnalyticsEnable(true);
        _pushMessaging.isfcmEnabled = _pushTiggle.isOn;
        _pushMessaging.isnightEnabled = _pushNightTiggle.isOn;
        _acceptTermsWindow.SetActive(false);
        PlayerPrefs.SetInt("FirstAcceptTerms", 1);
        playNANOOLogin.TokenLogin();
    }

    public void AllAgreeButton()
    {
        _analytics.AnalyticsEnable(true);
        _pushMessaging.isfcmEnabled = true;
        _pushMessaging.isnightEnabled = true;
        _acceptTermsWindow.SetActive(false);
        PlayerPrefs.SetInt("FirstAcceptTerms", 1);
        playNANOOLogin.TokenLogin();
    }

    public void ExitButton()
    {
        Application.Quit();
    }



}
