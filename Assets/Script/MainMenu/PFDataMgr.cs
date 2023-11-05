using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class PFDataMgr : MonoBehaviour
{
    //[SerializeField] TMP_Text XPDisplay;
    [SerializeField] TMP_Text displayNameText;

    #region USER FUNCTIONS

    public void SetUserData(int xp, int level)
    {
        var request = new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>()
            {
                {"XP", xp.ToString()},
                {"Level", level.ToString()}
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnUserDataUpdateSuccess, OnError);
    }

    public void AddPlayerScore(int score)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>{
                new StatisticUpdate{
                    StatisticName = "HighScore",
                    Value = score
                }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnError);
    }

    public void GetUserData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnUserDataSuccess, OnError);
    }
    void GetAccountInfo() {
		GetAccountInfoRequest request = new GetAccountInfoRequest();
		PlayFabClientAPI.GetAccountInfo(request, Success, Fail);
	}
 
 
    #endregion

    #region CALLBACK FUNCTIONS

    private void OnUserDataUpdateSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Successfully updated user data");
    }

    private void OnUserDataSuccess(GetUserDataResult result)
    {
        if (result.Data == null || !result.Data.ContainsKey("XP"))
        {
            Debug.Log("No XP data found");
        }
        else
        {
            string xpValue = result.Data["XP"].Value;
            Debug.Log("XP: " + xpValue);
            // You can update UI or perform other actions with the retrieved XP value
        }
    }

    private void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Successful leaderboard sent: " + result.ToJson());
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("Error: " + error.GenerateErrorReport());
    }

    void Success (GetAccountInfoResult result)
	{      
		
		string MyPlayfabID = result.AccountInfo.PlayFabId;
        var request = new GetPlayerProfileRequest
        {
            PlayFabId = MyPlayfabID,
        };
        PlayFabClientAPI.GetPlayerProfile(request, OnGetPlayerProfileSuccess, OnGetPlayerProfileFailure);

	}
 
 
    void Fail(PlayFabError error){
 
		Debug.LogError (error.GenerateErrorReport ());
	}


    private void OnGetPlayerProfileSuccess(GetPlayerProfileResult result)
    {
        string displayName = result.PlayerProfile.DisplayName;
        displayNameText.text = displayName;
        Debug.Log("Player Display Name: " + displayName);
    }

    private void OnGetPlayerProfileFailure(PlayFabError error)
    {
        Debug.LogError("Error getting player profile: " + error.ErrorMessage);
    }

    #endregion
}
