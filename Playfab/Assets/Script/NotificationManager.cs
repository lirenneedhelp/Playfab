using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance = null;
    private void Awake()
    {
        if (!Instance)
            Instance = this;
    }
    void RetrieveNotifications(string playerPlayFabId)
    {
        GetPlayerCombinedInfoRequest request = new GetPlayerCombinedInfoRequest
        {
            PlayFabId = playerPlayFabId,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
               
            }
        };

        PlayFabClientAPI.GetPlayerCombinedInfo(request, OnGetPlayerCombinedInfoSuccess, OnGetPlayerCombinedInfoFailure);
    }

    // Callback for a successful GetPlayerCombinedInfo API call
    void OnGetPlayerCombinedInfoSuccess(GetPlayerCombinedInfoResult result)
    {
        // Handle the success response
        var notifications = result.InfoResultPayload.UserData; // Notifications may be present in UserData

        // Process notifications as needed
        Debug.Log("Received notifications: " + notifications);
    }

    // Callback for a failed GetPlayerCombinedInfo API call
    void OnGetPlayerCombinedInfoFailure(PlayFabError error)
    {
        // Handle the error response
        Debug.LogError("Error retrieving notifications: " + error.ErrorMessage);
    }
}
