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
