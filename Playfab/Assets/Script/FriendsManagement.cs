using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

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
    [SerializeField] GameObject friendPendingPrefab;
    [SerializeField] GameObject friendAddPanel;
    [SerializeField] GameObject friendReqPanel;
    [SerializeField] GameObject friendInfoPanel;
    [SerializeField] GameObject friendDeletePanel;
    [SerializeField] GameObject deletedPanel;

    [SerializeField] TMP_InputField inputField;
    [SerializeField] TMP_Text infoMessage;
    [SerializeField] RectTransform _content;
    [SerializeField] RectTransform _pending_context;

    [SerializeField] TMP_Text notificationCount;

    List<FriendInfo> _friends = null;

    List<FriendInfo> _pending = new();

    int notification_count = 0;

    string friendToDelete;

    void DisplayFriends(List<FriendInfo> friendsCache)
    {
        // Clear existing leaderboard items
        foreach (Transform child in _content.transform)
        {
            Destroy(child.gameObject);
        }

        friendsCache.ForEach(f =>
        {
            if (f.Tags != null && f.Tags.Contains("confirmed"))
            {
                GameObject friendPrefab = Instantiate(friendListPrefab, _content);
                //Debug.Log(f.TitleDisplayName);
                RetrieveFriendStatus(f.TitleDisplayName, friendPrefab);
                friendPrefab.GetComponent<Button>().onClick.AddListener(() =>
                {
                    OpenFriendInfoPanel(f.TitleDisplayName); 

                });
            }
        });
    }

    public void GetFriends()
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest {

        }, result=>{
            _friends = result.Friends;

            foreach (var friendInfo in result.Friends)
            {
                if (friendInfo.Tags.Contains("requester"))
                {
                    // Player 2 has a friend request from this player
                    notification_count++;
                    Update_NotificationCount();


                    if (!_pending.Contains(friendInfo))
                        _pending.Add(friendInfo);

                    Debug.Log("Received friend request from: " + friendInfo.FriendPlayFabId);
                }
            }
        }, e=>{
            Debug.Log(e.GenerateErrorReport());
        });
    }
    public void GetFriendsForDisplay()
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {

        }, result => {
            _friends = result.Friends;
            DisplayFriends(_friends);
        }, e => {
            Debug.Log(e.GenerateErrorReport());
        });
    }

    public void DisplayFL()
    {
        GetFriendsForDisplay();
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
        string friendId = "";

        PlayFabClientAPI.GetAccountInfo(new()
        {
            TitleDisplayName = friendDisplay
        },
        r =>
        {
            friendId = r.AccountInfo.PlayFabId;
            Debug.Log(friendDisplay);
            Debug.Log(friendId);
            //AddFriend(FriendIdType.Username, friendDisplay);
            ExecuteCloudScriptRequest request = new()
            {
                FunctionName = "SendFriendRequest", // CloudScript function name
                FunctionParameter = new
                {
                    mainPlayFabId = DataCarrier.Instance.playfabID,
                    friendPlayFabId = friendId
                    // other parameters if required by your CloudScript function
                }
            };

            PlayFabClientAPI.ExecuteCloudScript(request, OnSendFriendRequestSuccess, OnSendFriendRequestFailure);
        },
        e =>
        {
            Debug.Log("Cannot Find Friend");
            infoMessage.text = "Cannot Find Friend";
            return;
        });

       
    }

    public void DisplayFriendRequestList()
    {
        // Clear existing leaderboard items
        foreach (Transform child in _pending_context.transform)
        {
            Destroy(child.gameObject);
        }

        foreach(var friendInfo in _pending)
        {
            string friendName = friendInfo.TitleDisplayName;

            GameObject fListPendingPrefab = Instantiate(friendPendingPrefab, _pending_context);
            fListPendingPrefab.transform.Find("PlayerNameText").GetComponent<TMP_Text>().text = friendName;
            fListPendingPrefab.transform.Find("AcceptFriend").GetComponent<Button>().onClick.AddListener(() =>
            {
                AcceptFriend(friendName, friendInfo);
                Destroy(fListPendingPrefab);

            });
            fListPendingPrefab.transform.Find("DeclineFriend").GetComponent<Button>().onClick.AddListener(() =>
            {
                DeclineFriend(friendName, friendInfo);
                Destroy(fListPendingPrefab);
            });
        }
    }

    void DeclineFriend(string fName, FriendInfo pendingFriend)
    {
        string friendId = "";

        PlayFabClientAPI.GetAccountInfo(new()
        {
            TitleDisplayName = fName
        },
        r =>
        {
            friendId = r.AccountInfo.PlayFabId;
            Debug.Log(friendId);

            var request = new ExecuteCloudScriptRequest
            {
                FunctionName = "RemoveFriend",
                FunctionParameter = new
                {
                    mainPlayFabId = DataCarrier.Instance.playfabID,
                    friendPlayFabId = friendId
                }
            };

            PlayFabClientAPI.ExecuteCloudScript(request, result => {
                Debug.Log("Declined friend request");
                _pending.Remove(pendingFriend);
                notification_count--;
                Update_NotificationCount();
            }, e => {
                Debug.Log(e.GenerateErrorReport());
            });
        },
        e =>
        {
            Debug.Log(e.GenerateErrorReport());
            return;
        });
    }

    public void RemoveFriend()
    {
        string fName = friendToDelete;
        string friendId = "";

        PlayFabClientAPI.GetAccountInfo(new()
        {
            TitleDisplayName = fName
        },
        r =>
        {
            friendId = r.AccountInfo.PlayFabId;
            Debug.Log(friendId);

            var request = new ExecuteCloudScriptRequest
            {
                FunctionName = "RemoveFriend",
                FunctionParameter = new
                {
                    mainPlayFabId = DataCarrier.Instance.playfabID,
                    friendPlayFabId = friendId
                }
            };

            PlayFabClientAPI.ExecuteCloudScript(request, result => {
                Debug.Log("Removed friend :-(");
                CloseFriendDeleteConfirmationPanel();
                deletedPanel.SetActive(true);
                deletedPanel.transform.Find("Display").GetComponent<TMP_Text>().text = fName + " is no longer your friend :(";
               
            }, e => {
                Debug.Log(e.GenerateErrorReport());
            });
        },
        e =>
        {
            Debug.Log(e.GenerateErrorReport());
            return;
        });
    }

    void AcceptFriend(string fName, FriendInfo newFriend)
    {
        string friendId = "";

        PlayFabClientAPI.GetAccountInfo(new()
        {
            TitleDisplayName = fName
        },
        r =>
        {
            friendId = r.AccountInfo.PlayFabId;
            Debug.Log(friendId);

            var request = new ExecuteCloudScriptRequest
            {
                FunctionName = "AcceptFriendRequest",
                FunctionParameter = new
                {
                    mainPlayFabId = DataCarrier.Instance.playfabID,
                    friendPlayFabId = friendId
                }
            };

            PlayFabClientAPI.ExecuteCloudScript(request, result => {
                Debug.Log("Added friend");
                _pending.Remove(newFriend);
                notification_count--;
                Update_NotificationCount();
            }, e => {
                Debug.Log(e.GenerateErrorReport());
            });
        },
        e =>
        {
            Debug.Log(e.GenerateErrorReport());
            return;
        });

       

    }
    void RetrieveFriendStatus(string fName, GameObject targetedPrefab)
    {
        string friendId = "";

        PlayFabClientAPI.GetAccountInfo(new()
        {
            TitleDisplayName = fName
        },
        r =>
        {
            friendId = r.AccountInfo.PlayFabId;
            Debug.Log(friendId);
            var request = new GetUserDataRequest
            {
                PlayFabId = friendId
            };

            PlayFabClientAPI.GetUserData(request, result=> {

                if (result.Data == null || !result.Data.ContainsKey("Status"))
                { 
                    Debug.Log("your friend is weird?");
                    return;
                }
                if (!result.Data.ContainsKey("LastLoggedIn"))
                    return;

                DateTime lastLoggedIn = new DateTime(long.Parse(result.Data["LastLoggedIn"].Value), DateTimeKind.Local);
                TimeSpan diff = DateTime.Now - lastLoggedIn;

                bool status;
                bool.TryParse(result.Data["Status"].Value, out status);
                targetedPrefab.transform.Find("PlayerNameText").GetComponent<TMP_Text>().text = fName;
                TMP_Text status_text = targetedPrefab.transform.Find("PlayerStatusText").GetComponent<TMP_Text>();
                status_text.color = status ? Color.green : Color.black;
                status_text.text = status ? "ONLINE" : "OFF(" + TimeFormatter.FormatTimeDifference(diff) + ")";
                //result.Data["OnlineStatus"].Value;

            }, e => { });
        },
        e =>
        {
            Debug.Log(e.GenerateErrorReport());
            return;
        });

    }


    public void OpenFriendAddPanel()
    {
        friendAddPanel.SetActive(true);
    }
    public void CloseFriendAddPanel()
    {
        friendAddPanel.SetActive(false);
    }
    public void OpenFriendRequestPanel()
    {
        DisplayFriendRequestList();
        friendReqPanel.SetActive(true);
    }
    public void CloseFriendRequestPanel()
    {
        friendReqPanel.SetActive(false);
    }
    public void OpenFriendInfoPanel(string fName)
    {
        friendInfoPanel.SetActive(true);
        friendInfoPanel.transform.Find("FriendName_Text").GetComponent<TMP_Text>().text = "name:" + fName;
        friendDeletePanel.transform.Find("Display").GetComponent<TMP_Text>().text = "are you sure you want to remove " + fName + " as a friend?";
        friendToDelete = fName;
    }
    public void CloseFriendInfoPanel()
    {
        friendInfoPanel.SetActive(false);
    }
    public void OpenFriendDeleteConfirmationPanel()
    {
        friendDeletePanel.SetActive(true);
        CloseFriendInfoPanel();
    }
    public void CloseFriendDeleteConfirmationPanel()
    {
        friendDeletePanel.SetActive(false);
    }
    public void CloseDeletedPanel()
    {
        deletedPanel.SetActive(false);
    }
    private void Start()
    {
        GetFriends();
    }
    private void Update_NotificationCount()
    {
       notificationCount.text = notification_count > 0 ? notification_count.ToString() : "";
    }
}
