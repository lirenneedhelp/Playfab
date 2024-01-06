using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TradeManager : MonoBehaviour
{
    void GiveItemTo(string secondPlayerId, string myItemInstanceId)
    {
        PlayFabClientAPI.OpenTrade(new OpenTradeRequest
        {
            AllowedPlayerIds = new List<string> { secondPlayerId }, // PlayFab ID for the friend who will receive your gift
            OfferedInventoryInstanceIds = new List<string> { myItemInstanceId } // The item instanceId fetched from GetUserInventory()
        }, r=> { }, e=> { });
    }
    void ExamineTrade(string firstPlayFabId, string tradeId)
    {
        PlayFabClientAPI.GetTradeStatus(new GetTradeStatusRequest
        {
            OfferingPlayerId = firstPlayFabId,
            TradeId = tradeId
        }, r => { }, e => { });
    }

    void AcceptGiftFrom(string firstPlayFabId, string tradeId)
    {
        PlayFabClientAPI.AcceptTrade(new AcceptTradeRequest
        {
            OfferingPlayerId = firstPlayFabId,
            TradeId = tradeId
        }, r => { }, e => { });
    }

    
}
