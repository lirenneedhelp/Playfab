using Photon.Pun;
using PlayFab;
using PlayFab.GroupsModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;

public class PlayerGuild : MonoBehaviourPun
{
    [SerializeField]
    GameObject guild_invite_panel;

    string groupName = "";

    string groupId = "";


    // Start is called before the first frame update
    void Start()
    {
        ListGroups();
    }
    private void ListGroups()
    {
        var request = new ListMembershipRequest();
        PlayFabGroupsAPI.ListMembership(request, r=> {
            foreach (var pair in r.Groups) //Groups
            {
                var req = new ListGroupMembersRequest()
                {
                    Group = pair.Group,
                };
                PlayFabGroupsAPI.ListGroupMembers(req, result => { 
                foreach (var info in result.Members) // Role
                {
                    foreach (var member in info.Members) // People
                    {
                        if (member.Key.Id == DataCarrier.Instance.entityID)
                        {
                            OnListGroups(pair.Group.Id, pair.Group.Type);
                            groupName = pair.GroupName;
                            groupId = pair.Group.Id;
                            DataCarrier.Instance.inGuild = true;
                            DataCarrier.Instance.group = pair.Group;
                            break;
                        }
                    }
                }
            }
            , e => {
                Debug.Log(e);
            });

            }
        }, error => { Debug.Log(error.GenerateErrorReport()); });   
    }

    private void OnListGroups(string groupId, string groupType)
    {
        var getObjectsRequest = new PlayFab.DataModels.GetObjectsRequest()
        {
            Entity = new PlayFab.DataModels.EntityKey
            {
                Id = groupId,
                Type = groupType
            }
        };
        PlayFabDataAPI.GetObjects(getObjectsRequest,
        getObjectResult =>
        {
            if (getObjectResult.Objects.TryGetValue("guild_info", out var entityInfo))
            {
                DataCarrier.Instance.guildStats = JsonConvert.DeserializeObject<DataCarrier.GuildStats>(entityInfo.DataObject.ToString());
                Debug.Log(DataCarrier.Instance.guildStats.currentLevel);
            }
        },
        error =>
        {
            Debug.Log(error);
        });
        

    }
    public void InviteToGroup(string playerName)
    {
        //Debug.Log(playerName);

        var guildInfoReq = new GetGroupRequest() { GroupName = groupName };
        PlayFabGroupsAPI.GetGroup(guildInfoReq,
            resultGroup => {
                // A player-controlled entity invites another player-controlled entity to an existing group
                //Entity Key is the player you want to invite
                var invitedPlayerReq = new PlayFab.ClientModels.GetAccountInfoRequest() { Username = playerName };
                PlayFabClientAPI.GetAccountInfo(invitedPlayerReq, result =>
                {
                    var request = new InviteToGroupRequest { Group = resultGroup.Group, Entity = new EntityKey() { Id = result.AccountInfo.TitleInfo.TitlePlayerAccount.Id , Type = "title_player_account"} };
                    PlayFabGroupsAPI.InviteToGroup(request, response =>
                    {
                        photonView.RPC(nameof(RPC_SendGuildInvite), Player.FindPlayerByNickname(playerName), groupName);
                    }, error => { Debug.Log(error.GenerateErrorReport()); }
                        );
                }, error => { Debug.LogError(error.GenerateErrorReport()); });

            }, error => { Debug.LogError(error.GenerateErrorReport()); });

    }

    public void AcceptGuildInvite(string groupName)
    {
        var guildInfoReq = new GetGroupRequest() { GroupName = groupName };
        PlayFabGroupsAPI.GetGroup(guildInfoReq, 
            result => {
                var request = new AcceptGroupInvitationRequest { Group = result.Group };
                PlayFabGroupsAPI.AcceptGroupInvitation(request, response=> {
                    CloseGuildInvite();
                }, error => { Debug.LogError(error.GenerateErrorReport()); });
            }, error => { Debug.LogError(error.GenerateErrorReport()); });

    }

    public void DeclineGuildInvite(string groupName)
    {
        var guildInfoReq = new GetGroupRequest() { GroupName = groupName };
        PlayFabGroupsAPI.GetGroup(guildInfoReq,
            result => {
                var request = new RemoveGroupInvitationRequest { Group = result.Group };
                PlayFabGroupsAPI.RemoveGroupInvitation(request, response => {
                    CloseGuildInvite();
                }, error => { Debug.LogError(error.GenerateErrorReport()); });
            }, error => { Debug.LogError(error.GenerateErrorReport()); });
    }

    [PunRPC]
    void RPC_SendGuildInvite(string groupName)
    {
        OpenGuildInvite(groupName);
    }

    void OpenGuildInvite(string groupName)
    {
        guild_invite_panel.transform.Find("Button_AcceptGuild").GetComponent<Button>().onClick.RemoveAllListeners();
        guild_invite_panel.transform.Find("Button_AcceptGuild").GetComponent<Button>().onClick.AddListener(() => {
            AcceptGuildInvite(groupName);
        });
        guild_invite_panel.transform.Find("Button_CloseGuild").GetComponent<Button>().onClick.RemoveAllListeners();
        guild_invite_panel.transform.Find("Button_CloseGuild").GetComponent<Button>().onClick.AddListener(() => {
            DeclineGuildInvite(groupName);
        });


        guild_invite_panel.transform.Find("Guild_InviteRequest_Label").GetComponent<TMPro.TMP_Text>().text = "Do you want to join " + groupName;
        guild_invite_panel.SetActive(true);
    }

    public void CloseGuildInvite()
    {
        guild_invite_panel.SetActive(false);
    }
}
