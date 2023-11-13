using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

[System.Serializable]
public struct JSListWrapper<T>
{
    public List<T> list;
    public JSListWrapper(List<T> list) => this.list = list;
}

[System.Serializable]
public struct PlayerData
{
    public int level;
    public int maxLevel;
    public float currentEXP;
    public int nextLevelEXP;
    public int totalSkillPoints;

    public PlayerData(int _level, int _maxLevel, float _currentEXP, int _nextLevelEXP, int _totalSkillPoints)
    {
        level = _level;
        maxLevel = _maxLevel;
        currentEXP = _currentEXP;
        nextLevelEXP = _nextLevelEXP;
        totalSkillPoints = _totalSkillPoints;
    }
}

[System.Serializable]
public struct Skill
{
    public string name;
    public int level;
    public bool isUnlocked;

    public Skill(string _name, int _level, bool unlocked)
    {
        name = _name;
        level = _level;
        isUnlocked = unlocked;
    }

};

public class PFDataMgr : MonoBehaviour
{
    //[SerializeField] TMP_Text XPDisplay;
    [SerializeField] TMP_Text displayNameText;


    void Awake()
    {
        GetPlayerProfile(DataCarrier.Instance.playfabID);
        CheckIfCustomID();
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

    // Retrieve Player Data Via JSON
    public static void UpdateDataJSON(int level, int maxLevel, float currExp, int nextLvlExp, int sp)
    {
        PlayerData pData = new(level, maxLevel, currExp, nextLvlExp, sp);
        List<PlayerData> list = new()
                                {
                                    pData
                                };
        string stringListAsJson = JsonUtility.ToJson(new JSListWrapper<PlayerData>(list));
        Debug.Log(stringListAsJson);
        var req = new UpdateUserDataRequest{
            Data = new Dictionary<string, string>
            {
                {"PlayerData", stringListAsJson}
            }
        };
        PlayFabClientAPI.UpdateUserData(req, 
        result=>{
            Debug.Log("Data sent success!");
            }, 
        e=>{
            Debug.Log("Error" + e.GenerateErrorReport());
        });
    }

    public static void LoadJSON()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), 
        r=>{
            Debug.Log("Received JSON data");
            if (r.Data != null && r.Data.ContainsKey("PlayerData"))
            {
                Debug.Log(r.Data["PlayerData"].Value);
                JSListWrapper<PlayerData> jlw = JsonUtility.FromJson<JSListWrapper<PlayerData>>(r.Data["PlayerData"].Value);
                
                LevelSystem.Instance.level = jlw.list[0].level;
                LevelSystem.Instance.maxLevel = jlw.list[0].maxLevel;
                LevelSystem.Instance.currentXp = jlw.list[0].currentEXP;
                LevelSystem.Instance.nextLevelXp = jlw.list[0].nextLevelEXP;
                LevelSystem.Instance.skillPoints = jlw.list[0].totalSkillPoints;

            }
            else
            {
                Debug.Log("Invalid Data");
            }
        }, 
        e=>{
            Debug.Log("Error" + e.GenerateErrorReport());
            });
    }

    // Retrieve user data via playfab data title

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

    public static void GetUserData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), 
        result=>{
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
        },
        error=>{
            Debug.LogError("Error: " + error.GenerateErrorReport());
        });
    }

    // Retrieve Player Profile
    void GetPlayerProfile(string playFabId) 
    {
        PlayFabClientAPI.GetPlayerProfile( new GetPlayerProfileRequest() {
            PlayFabId = playFabId,
            ProfileConstraints = new PlayerProfileViewConstraints() {
                ShowDisplayName = true
            }
        },
        result => 
        {
            displayNameText.text = result.PlayerProfile.DisplayName;
            DataCarrier.Instance.displayName = result.PlayerProfile.DisplayName;
        },
        error => Debug.LogError(error.GenerateErrorReport()));
    }

    // Check if logged in is guest
    private void CheckIfCustomID()
    {
        var request = new GetAccountInfoRequest
        {
            PlayFabId = DataCarrier.Instance.playfabID
        };

        PlayFabClientAPI.GetAccountInfo(request, OnGetAccountInfoSuccess, 
        error=>{
            Debug.LogError("Failed to get account info: " + error.GenerateErrorReport());
        });
    }

    private void OnGetAccountInfoSuccess(GetAccountInfoResult result)
    {
        // Check if the user has a custom ID
        if (result.AccountInfo.CustomIdInfo.CustomId == null)
            DataCarrier.Instance.isGuest = false;
        else
            DataCarrier.Instance.isGuest = true;
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
