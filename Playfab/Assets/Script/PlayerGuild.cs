using Photon.Pun;
using PlayFab;
using PlayFab.GroupsModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerGuild : MonoBehaviourPun
{
    [SerializeField]
    GameObject guild_invite_panel;

    [HideInInspector]
    string groupName = "";

    string groupId = "";

    // Start is called before the first frame update
    void Start()
    {
        ListGroups();
    }
    private void ListGroups()
    {
        //if (photonView.IsMine)
        {
            var request = new ListMembershipRequest { };
            PlayFabGroupsAPI.ListMembership(request, OnListGroups, error => { Debug.Log(error.GenerateErrorReport()); });
        }
    }
    private void OnListGroups(ListMembershipResponse response)
    {
        var prevRequest = (ListMembershipRequest)response.Request;
        foreach (var pair in response.Groups)
        {
            groupId = pair.Group.Id;
            groupName = pair.GroupName;
        }

    }
    public void InviteToGroup(string playerName)
    {
        Debug.Log(playerName);

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

    [PunRPC]
    void RPC_SendGuildInvite(string groupName)
    {
        // guild_invite_panel.transform.Find
        OpenGuildInvite(groupName);
    }

    void OpenGuildInvite(string groupName)
    {
        guild_invite_panel.transform.Find("Button_AcceptGuild").GetComponent<Button>().onClick.RemoveAllListeners();
        guild_invite_panel.transform.Find("Button_AcceptGuild").GetComponent<Button>().onClick.AddListener(() => { AcceptGuildInvite(groupName); });
        guild_invite_panel.transform.Find("Guild_InviteRequest_Label").GetComponent<TMPro.TMP_Text>().text = "Do you want to join " + groupName;
        guild_invite_panel.SetActive(true);
    }

    public void CloseGuildInvite()
    {
        guild_invite_panel.SetActive(false);
    }
}
