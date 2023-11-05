using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using System.Collections.Generic;
using TMPro; //for text mesh pro UI elements

public class PlayFabUserMgtTMP : MonoBehaviour
{
    [SerializeField] TMP_InputField userEmail, userPassword, userName, currentScore, displayName, XP, level;
    [SerializeField] TextMeshProUGUI Msg;
    void UpdateMsg(string msg) //to display in console and messagebox
    {
        Debug.Log(msg);
        Msg.text=msg+'\n';
    }
    void OnError(PlayFabError e) //report any errors here!
    {
        UpdateMsg("Error" + e.GenerateErrorReport());
    }

    public void OnButtonRegUser()
    { //for button click
        var registerRequest = new RegisterPlayFabUserRequest
        {
            Email = userEmail.text,
            Password = userPassword.text,
            Username = userName.text
        };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegSuccess, OnError);
    }
    void OnRegSuccess(RegisterPlayFabUserResult r)
    {
        UpdateMsg("Registration success!");

        //To create a player display name 
        var req = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = displayName.text,
        };
        // update to profile
        PlayFabClientAPI.UpdateUserTitleDisplayName(req, OnDisplayNameUpdate, OnError);
    }
    void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult r)
    {
        UpdateMsg("display name updated!" + r.DisplayName);
    }

    void OnLoginSuccess(LoginResult r)
    {
        UpdateMsg("Login Success!"+r.PlayFabId+r.InfoResultPayload.PlayerProfile.DisplayName);
        ClientGetTitleData(); //MOTD
        GetUserData(); //Player Data
    }
    public void OnButtonLoginEmail() //login using email + password
    {
        var loginRequest = new LoginWithEmailAddressRequest
        {
            Email = userEmail.text,
            Password = userPassword.text,
            //to get player profile, to get displayname
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSuccess, OnError);
    }
    public void OnButtonLoginUserName() //login using username + password
    {
        var loginRequest = new LoginWithPlayFabRequest
        {
            Username = userName.text,
            Password = userPassword.text,
            //to get player profile, including displayname
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithPlayFab(loginRequest, OnLoginSuccess, OnError);
    }

    public void OnButtonLogout()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        UpdateMsg("logged out");
    }
    public void PasswordResetRequest()
    {
        var req = new SendAccountRecoveryEmailRequest
        {
            Email = userEmail.text,
            TitleId = "6881E"
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(req, OnPasswordReset, OnError);
    }
    void OnPasswordReset(SendAccountRecoveryEmailResult r)
    {
        Msg.text = "Password reset email sent.";
    }

    public void OnButtonGetLeaderboard()
    {
        var lbreq = new GetLeaderboardRequest
        {
            StatisticName = "highscore", //playfab leaderboard statistic name
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(lbreq, OnLeaderboardGet, OnError);
    }
    void OnLeaderboardGet(GetLeaderboardResult r)
    {
        string LeaderboardStr = "Leaderboard\n";
        foreach (var item in r.Leaderboard)
        {
            string onerow = item.Position + "/" + item.PlayFabId + "/" + item.DisplayName + "/" + item.StatValue + "\n";
            LeaderboardStr+=onerow; //combine all display into one string 1.
        }
        UpdateMsg(LeaderboardStr);
    }

    public void ClientGetTitleData() { //Slide 33: MOTD
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(),
            result => {
                if(result.Data == null || !result.Data.ContainsKey("MOTD")) UpdateMsg("No MOTD");
                else UpdateMsg("MOTD: "+result.Data["MOTD"]);
            },
            error => {
                UpdateMsg("Got error getting titleData:");
                UpdateMsg(error.GenerateErrorReport());
            }
        );
    }
    public void OnButtonSetUserData() { //Player Data
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest() {
            Data = new Dictionary<string, string>() {
                {"XP", XP.text.ToString()},
                {"Level", level.text.ToString()}
            }
        },
        result => UpdateMsg("Successfully updated user data"),
        error => {
            UpdateMsg("Error setting user data");
            UpdateMsg(error.GenerateErrorReport());
        });
    }
    public void GetUserData() { //Player Data
        PlayFabClientAPI.GetUserData(new GetUserDataRequest() 
        , result => {
            UpdateMsg("Got user data:");
            if (result.Data == null || !result.Data.ContainsKey("XP")) UpdateMsg("No XP");
            else XP.text=result.Data["XP"].Value;
            if (result.Data == null || !result.Data.ContainsKey("Level")) UpdateMsg("No Level");
            else level.text=result.Data["Level"].Value;          
        }, (error) => {
            UpdateMsg("Got error retrieving user data:");
            UpdateMsg(error.GenerateErrorReport());
        });
    }

}

