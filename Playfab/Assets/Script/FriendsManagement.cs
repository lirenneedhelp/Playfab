using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;

enum FriendIdType
{
    PlayFabId,
    Username,
    Email,
    DisplayName
}
public class FriendsManagement : MonoBehaviour
{
    void DisplayFriends(List<FriendInfo> friendsCache)
    {

    }

    public void GetFriends()
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest {

        }, result=>{

        }, e=>{
            Debug.Log(e.GenerateErrorReport());
        });
    }

    void AddFriend(FriendIdType idType, string friendId)
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

        }, e=>{
            e.GenerateErrorReport();
        });
    }

    public void OnAddFriend()
    {

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
}
