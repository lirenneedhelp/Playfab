using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TradeManager : MonoBehaviour
{
    public static void GiveItemTo(string secondPlayerId, string myItemInstanceId)
    {
        
        PlayFabClientAPI.GetAccountInfo(new()
        {
            TitleDisplayName = secondPlayerId
        }, 
        r =>
        {
            string secondPlayfabId = r.AccountInfo.PlayFabId;

            PlayFabClientAPI.OpenTrade(new OpenTradeRequest
            {
                AllowedPlayerIds = new List<string> { secondPlayfabId }, // PlayFab ID for the friend who will receive your gift
                OfferedInventoryInstanceIds = new List<string> { myItemInstanceId } // The item instanceId fetched from GetUserInventory()

            }, r =>
            {
                Debug.Log("Sucessfully transferred item");
            }, e =>
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
    public static string CheckForTrades()
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
        }) ;

        return tradeID;
    }

    public static void AcceptGiftFrom(string firstPlayFabId, string tradeId)
    {
        PlayFabClientAPI.AcceptTrade(new AcceptTradeRequest
        {
            OfferingPlayerId = firstPlayFabId,
            TradeId = tradeId
        }, r => {
            foreach(var tradeInfo in r.Trade.AcceptedInventoryInstanceIds)
            {
                Debug.Log(tradeInfo);
            }
            
            Debug.Log("Trade accepted");
        }, e => { });
    }

    
}
