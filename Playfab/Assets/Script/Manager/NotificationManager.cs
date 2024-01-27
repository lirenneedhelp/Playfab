using PlayFab;
using PlayFab.ClientModels;
using PlayFab.GroupsModels;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationManager : MonoBehaviour
{
    [SerializeField] GameObject friendPendingPrefab, guildPendingPrefab;
    [SerializeField] RectTransform _pending_context;
    [SerializeField] GameObject pendingPanel;
    [SerializeField] TMP_Text notificationCount;
    public int notification_count = 0;

    public List<FriendInfo> _pending = new();
    List<GroupApplication> _pendingGuild = new();

    [SerializeField] FriendsManagement friendManager;
    [SerializeField] GuildTestController guildTestController;

    // Start is called before the first frame update
    void Start()
    {
        GetNotifications();
    }

    public void GetNotifications()
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {

        }, result => {

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
        }, e => {
            Debug.Log(e.GenerateErrorReport());
        });

        PlayFabGroupsAPI.ListMembership(new ListMembershipRequest(), 
            result => 
            {
               PlayFab.GroupsModels.EntityKey entityKey = null; 
                foreach (var pair in result.Groups)
                {
                    entityKey = pair.Group;
                }
                var applicationRequest = new ListGroupApplicationsRequest() { Group = entityKey };
            PlayFabGroupsAPI.ListGroupApplications(applicationRequest, 
                result=> {
                    _pendingGuild = result.Applications;
                    foreach (GroupApplication application in result.Applications)
                    {
                        notification_count++;
                        Update_NotificationCount();
                    }

            }, e=> { Debug.Log(e.GenerateErrorReport()); });
            }, error => { Debug.Log(error.GenerateErrorReport()); });

       
    }
    public void OpenNotifications()
    {
        foreach (var friendInfo in _pending)
        {
            string friendName = friendInfo.TitleDisplayName;

            GameObject fListPendingPrefab = Instantiate(friendPendingPrefab, _pending_context);
            fListPendingPrefab.transform.Find("PlayerNameText").GetComponent<TMP_Text>().text = friendName;
            fListPendingPrefab.transform.Find("AcceptFriend").GetComponent<Button>().onClick.AddListener(() =>
            {
                friendManager.AcceptFriend(friendName, friendInfo);
                Destroy(fListPendingPrefab);

            });
            fListPendingPrefab.transform.Find("DeclineFriend").GetComponent<Button>().onClick.AddListener(() =>
            {
                friendManager.DeclineFriend(friendName, friendInfo);
                Destroy(fListPendingPrefab);
            });
        }

        foreach(var guildApplication in _pendingGuild)
        {
            string applicantName = guildApplication.Entity.Key.Id;

            GameObject guildListPendingPrefab = Instantiate(guildPendingPrefab, _pending_context);
            guildListPendingPrefab.transform.Find("PlayerNameText").GetComponent<TMP_Text>().text = applicantName;
            guildListPendingPrefab.transform.Find("AcceptGuildRequest").GetComponent<Button>().
                onClick.AddListener(() => {
                    guildTestController.AcceptApplication(guildApplication.Group, guildApplication.Entity.Key);
                    _pendingGuild.Remove(guildApplication);
                    Destroy(guildListPendingPrefab);
                });
            guildListPendingPrefab.transform.Find("DeclineGuildRequest").GetComponent<Button>().onClick.AddListener(() => { });

        }

        pendingPanel.SetActive(true);
    }
    public void CloseNotifications()
    {
        pendingPanel.SetActive(false);

        foreach (Transform child in _pending_context.transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void Update_NotificationCount()
    {
        notificationCount.text = notification_count > 0 ? notification_count.ToString() : "";
    }

}
