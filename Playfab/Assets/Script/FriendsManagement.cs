using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
public enum FriendIdType
{
    PlayFabId,
    Username,
    Email,
    DisplayName
}
public class FriendsManagement : MonoBehaviour
{
    void OnSendFriendRequestSuccess(ExecuteCloudScriptResult result)
    {
        Debug.Log("Friend request sent successfully!");
        infoMessage.text = "Friend request sent successfully!";
    }

    void OnSendFriendRequestFailure(PlayFabError error)
    {
        Debug.LogError("Error sending friend request: " + error.ErrorMessage);
        infoMessage.text = "Error sending friend request: " + error.ErrorMessage;
    }

    [SerializeField] GameObject friendListPrefab;
    [SerializeField] GameObject friendAddPanel;

    [SerializeField] TMP_InputField inputField;
    [SerializeField] TMP_Text infoMessage;
    [SerializeField] RectTransform _content;

    List<FriendInfo> _friends = null;


    void DisplayFriends(List<FriendInfo> friendsCache)
    {
        // Clear existing leaderboard items
        foreach (Transform child in _content.transform)
        {
            Destroy(child.gameObject);
        }

        friendsCache.ForEach(f =>
        {
            GameObject friendPrefab = Instantiate(friendListPrefab, _content);
            //Debug.Log(f.TitleDisplayName);
            friendPrefab.transform.Find("PlayerNameText").GetComponent<TMP_Text>().text = f.TitleDisplayName;
        });
    }

    public void GetFriends()
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest {

        }, result=>{
            _friends = result.Friends;

            //foreach (var friendInfo in result.Friends)
            //{
            //    if (friendInfo.Tags.Contains("requester"))
            //    {
            //        // Player 2 has a friend request from this player
            //        Debug.Log("Received friend request from: " + friendInfo.FriendPlayFabId);
            //    }
            //}
        }, e=>{
            Debug.Log(e.GenerateErrorReport());
        });
    }

    public void DisplayFL()
    {
        DisplayFriends(_friends);
    }

    public void AddFriend(FriendIdType idType, string friendId)
    {
        var req = new AddFriendRequest();

        switch (idType)
        {
            case FriendIdType.PlayFabId:
                req.FriendPlayFabId = friendId;
                break;
            case FriendIdType.Username:
                req.FriendUsername = friendId;
                break;
            case FriendIdType.Email:
                req.FriendEmail = friendId;
                break;
            case FriendIdType.DisplayName:
                req.FriendTitleDisplayName = friendId;
                break;
        }

        PlayFabClientAPI.AddFriend(req, result=>{
            infoMessage.text = "Friend request sent successfully!";
            GetFriends();
        }, e=>{
            infoMessage.text = "Error sending friend request: " + e.ErrorMessage;
            e.GenerateErrorReport();
        });
    }

    public void OnAddFriend()
    {
        string friendDisplay = inputField.text;

        AddFriend(FriendIdType.Username, friendDisplay);

        //ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest
        //{
        //    FunctionName = "SendFriendRequest", // CloudScript function name
        //    FunctionParameter = new
        //    {
        //        friendDisplayName = friendDisplay
        //        // other parameters if required by your CloudScript function
        //    }
        //};

        //PlayFabClientAPI.ExecuteCloudScript(request, OnSendFriendRequestSuccess, OnSendFriendRequestFailure);
    }

    void RemoveFriend(FriendInfo friendInfo)
    {
        PlayFabClientAPI.RemoveFriend(new RemoveFriendRequest{
            FriendPlayFabId = friendInfo.FriendPlayFabId
        }, result=>{

        }, e=>{

        });
    }

    void RemoveFriend(string pfid)
    {
        var req = new RemoveFriendRequest {
            FriendPlayFabId = pfid
        };

        PlayFabClientAPI.RemoveFriend(req,
        result=>{

        },
        e=>{

        });
    }

    public void OnUnfriend()
    {

    }

    public void OpenFriendAddPanel()
    {
        friendAddPanel.SetActive(true);
    }
    public void CloseFriendAddPanel()
    {
        friendAddPanel.SetActive(false);
    }
    private void Start()
    {
        GetFriends();
    }
}
