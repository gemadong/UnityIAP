using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST : MonoBehaviour
{
    public void StoreOpen()
    {
        Application.OpenURL("market://details?id=com.gemadong.unityiap");
    }
}
