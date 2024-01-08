using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TradeManager : MonoBehaviour
{
    public void GiveItemTo(string secondPlayerId, string myItemInstanceId, string firstPlayerId)
    {
        
        PlayFabClientAPI.GetAccountInfo(new()
        {
            TitleDisplayName = secondPlayerId
        }, 
        r =>
        {
            string secondPlayfabId = r.AccountInfo.PlayFabId;

            //Debug.LogError(secondPlayfabId);

            PlayFabClientAPI.OpenTrade(new OpenTradeRequest
            {
                AllowedPlayerIds = new List<string> { secondPlayfabId }, // PlayFab ID for the friend who will receive your gift
                OfferedInventoryInstanceIds = new List<string> { myItemInstanceId },
                // The item instanceId fetched from GetUserInventory()

            }, r =>
            {
                Debug.LogError("Sucessfully Opened Trade");
                transform.root.GetComponent<PhotonView>().RPC("RPC_CompleteTrade", Player.FindPlayerByNickname(secondPlayerId), firstPlayerId, r.Trade.TradeId);
                //AcceptGiftFrom(firstPlayerId, r.Trade.TradeId);
            }, 
            e =>
            {
                Debug.Log(e.GenerateErrorReport());
            });

        }, 
        e =>
        {
            Debug.Log("Failed to find target player");
        }
        );
        
       
    }
    public string CheckForTrades()
    {
        string tradeID = "";
        PlayFabClientAPI.GetPlayerTrades(new GetPlayerTradesRequest
        {
            StatusFilter = TradeStatus.Open
        }, r => {
            foreach (TradeInfo tradeInfo in r.AcceptedTrades)
            {
                Debug.Log(tradeInfo.TradeId);
                
            }
            tradeID = r.AcceptedTrades[0].TradeId;
        }, e => {
            Debug.Log(e.GenerateErrorReport());
        });
        Debug.LogError(tradeID);

        return tradeID;
    }

    public void AcceptGiftFrom(string playerName, string tradeId)
    {
        PlayFabClientAPI.GetAccountInfo(new()
        {
            TitleDisplayName = playerName
        },
        result =>
        {
            PlayFabClientAPI.AcceptTrade(new AcceptTradeRequest
            {
                OfferingPlayerId = result.AccountInfo.PlayFabId,
                TradeId = tradeId
            }, r => {
                foreach (var tradeInfo in r.Trade.AcceptedInventoryInstanceIds)
                {
                    Debug.Log(tradeInfo);
                }

                Debug.Log("Trade accepted");
                Inventory.Instance.GetPlayerInventory();

            }, e => {
                Debug.Log(e.GenerateErrorReport());
            });

        },
        e =>
        {
            Debug.LogError("No gifts");
        });



    }


    IEnumerator ProcessTradeCall(string playFabId, string tradeId)
    {
        yield return new WaitForSeconds(5f);
        AcceptGiftFrom(playFabId, tradeId);
    }



}
