using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GPGS : MonoBehaviour
{
    [SerializeField] private Text logText;
    string log;

    private void Start()
    {
        LoginB();
    }
    public void LoginB()
    {
        GPGSBinder.Inst.Login((success, localUser) =>
            log = $"{success}, {localUser.userName}, {localUser.id}, {localUser.state}, {localUser.underage}");
        logText.text = log;
    }

    public void LogoutB()
    {
        GPGSBinder.Inst.Logout();
        logText.text = "Logout!!";
    }

}