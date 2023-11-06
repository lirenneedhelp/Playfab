using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class PFDataMgr : MonoBehaviour
{
    //[SerializeField] TMP_Text XPDisplay;
    [SerializeField] TMP_Text displayNameText;


    void Awake()
    {
        GetPlayerProfile(DataCarrier.Instance.playfabID);
        GetUserData();
        
    }


    #region STATIC FUNCTIONS


        public static void AddPlayerScore(int score)
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

            PlayFabClientAPI.UpdatePlayerStatistics(request, 
            result=>{
                Debug.Log("Successful leaderboard sent: " + result.ToJson());
            }, 
            error=>{
                Debug.LogError("Error: " + error.GenerateErrorReport());
            });
        }



    #endregion

    #region USER FUNCTIONS

    public static void SetUserData(int xp, int xpUp, int level)
    {
        var request = new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>()
            {
                {"XP", xp.ToString()},
                {"RequiredXPToLevelUp", xpUp.ToString()},
                {"Level", level.ToString()}
            }
        };

        PlayFabClientAPI.UpdateUserData(request, 
        result =>{
            Debug.Log("Successfully updated user data");
        }, 
        error =>{
            Debug.LogError("Error: " + error.GenerateErrorReport());
        });
    }

    public void GetUserData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnUserDataSuccess, OnError);
    }

    void GetPlayerProfile(string playFabId) {
    PlayFabClientAPI.GetPlayerProfile( new GetPlayerProfileRequest() {
        PlayFabId = playFabId,
        ProfileConstraints = new PlayerProfileViewConstraints() {
            ShowDisplayName = true
        }
    },
    result => displayNameText.text = result.PlayerProfile.DisplayName,
    error => Debug.LogError(error.GenerateErrorReport()));
}

    #endregion

    #region CALLBACK FUNCTIONS

    private void OnUserDataSuccess(GetUserDataResult result)
    {
        if (result.Data == null)
        {
            Debug.Log("No user data found");
        }
        else
        {
            // Check if the "XP" data exists
            if (result.Data.ContainsKey("XP"))
            {
                string xpValue = result.Data["XP"].Value;
                Debug.Log("XP: " + xpValue);
                LevelSystem.Instance.currentXp = int.Parse(xpValue);
            }
            else
            {
                Debug.Log("No XP data found");
            }

            // Check if the "RequiredXPToLevelUp" data exists
            if (result.Data.ContainsKey("RequiredXPToLevelUp"))
            {
                string requiredXPValue = result.Data["RequiredXPToLevelUp"].Value;
                Debug.Log("Required XP to Level Up: " + requiredXPValue);
                LevelSystem.Instance.nextLevelXp = int.Parse(requiredXPValue);
            }
            else
            {
                Debug.Log("No RequiredXPToLevelUp data found");
            }

            // Check if the "Level" data exists
            if (result.Data.ContainsKey("Level"))
            {
                string levelValue = result.Data["Level"].Value;
                Debug.Log("Level: " + levelValue);
                LevelSystem.Instance.level = int.Parse(levelValue);
            }
            else
            {
                Debug.Log("No Level data found");
            }
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

    #endregion
}
