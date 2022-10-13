using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayNANOO;
using UnityEngine.Purchasing;

public class Receipt : MonoBehaviour
{
    Plugin plugin;

    void Awake()
    {
        plugin = Plugin.GetInstance();
    }
    
    public void LoginGuest()
    {
        plugin.AccountGuestSignIn((status, errorCode, jsonString, values) => {
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

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        plugin.IAP.Android(args.purchasedProduct.receipt, (status, errorMessage, jsonString, values) =>
        {
            if (status.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                Debug.Log(values["UserID"]);
                Debug.Log(values["PackageName"]);
                Debug.Log(values["OrderID"]);
                Debug.Log(values["ProductID"]);
                Debug.Log(values["Currency"]);
                Debug.Log(values["Quantity"]);
                Debug.Log(values["Price"]);
            }
            else
            {
                Debug.Log("Fail");
            }
        });

        return PurchaseProcessingResult.Complete;
    }

}
