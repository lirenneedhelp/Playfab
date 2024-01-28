using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Add Coins
            AddMoney();
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void AddMoney()
    {
        var addMoneyReq = new AddUserVirtualCurrencyRequest()
        {
            VirtualCurrency = "CC",
            Amount =  50
        };

        PlayFabClientAPI.AddUserVirtualCurrency(addMoneyReq, r =>
        {
            Debug.Log("Successfully Added Money");
        },
        e =>
        {
            Debug.Log(e.GenerateErrorReport());
        });

    }
}
