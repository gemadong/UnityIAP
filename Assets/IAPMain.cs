using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using UnityEngine.Events;

public class IAPMain : MonoBehaviour
{
    public IAPButton btnCoin;



    void Start()
    {
        this.btnCoin.onPurchaseComplete.AddListener(new UnityAction<Product>((product) =>
        {
            Debug.LogFormat("���ż��� : ", product.transactionID);
        }));

        this.btnCoin.onPurchaseFailed.AddListener(new UnityAction<Product, PurchaseFailureReason> ((product, reason) =>
        {
            Debug.LogFormat("���Ž��� : {0} , {1}", product.transactionID, reason);
        }));
    }

}
