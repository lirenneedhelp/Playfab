using Photon.Pun;
using PlayFab;
using PlayFab.GroupsModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class PlayerGuild : MonoBehaviourPun,IPunObservable
{
    [SerializeField]
    GameObject guild_invite_panel;

    PhotonView pv;

    public string groupName = "";

    string groupId = "";


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // If you are the owner, send the value to the network
            stream.SendNext(groupName);
            stream.SendNext(groupId);
        }
        else
        {
            // If you are not the owner, receive the value from the network
            groupName = (string)stream.ReceiveNext();
            groupId = (string)stream.ReceiveNext();
        }
    }
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        Debug.LogError("PhotonView");
    }
    // Start is called before the first frame update
    void Start()
    {
        if (!pv.IsMine)
            return;

        DataCarrier.Instance.ViewID = pv.ViewID;
        ListGroups();
    }
    private void ListGroups()
    {
       // Debug.Log("Acoustic");
        var request = new ListMembershipRequest();
        PlayFabGroupsAPI.ListMembership(request, r=> {
            //Debug.Log("Scott");
            foreach (var pair in r.Groups) //Groups
            {
                var req = new ListGroupMembersRequest()
                {
                    Group = pair.Group,
                };
                PlayFabGroupsAPI.ListGroupMembers(req, result => {
                   // Debug.Log("Awesome");

                    foreach (var info in result.Members) // Role
                    {
                        foreach (var member in info.Members) // People
                        {
                            if (member.Key.Id == DataCarrier.Instance.entityID)
                            {
                                Debug.Log("You're in");
                                OnListGroups(pair.Group.Id, pair.Group.Type);
                                groupName = pair.GroupName;
                                Debug.Log(groupName);
                                groupId = pair.Group.Id;
                                DataCarrier.Instance.inGuild = true;
                                DataCarrier.Instance.group = pair.Group;
                                return;
                            }
                        }
                    }

                }
                , e => {
                    Debug.Log(e);
              
                });
            }

            StartCoroutine(WaitForResult());

           
        }, error => { Debug.Log(error.GenerateErrorReport()); });   
    }
    //private void Update()
    //{
    //    if (pv.IsMine)
    //        Debug.LogError(pv.Owner.NickName);
    //}
    private IEnumerator WaitForResult()
    {
        yield return new WaitForSeconds(2f);

        if (!DataCarrier.Instance.inGuild)
        {
            var deleteplayerkey = new PlayFab.ClientModels.UpdateUserDataRequest()
            {
                KeysToRemove = new() { "entityID", "entityType" }
            };
            PlayFabClientAPI.UpdateUserData(deleteplayerkey,
                r =>
                {
                    Debug.LogError("Successfully removed your existence");
                }, e => { });
        }
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
    public void InviteToGroup(string playerName, string sender, int viewId)
    {
        //Debug.LogError(Player.FindPlayerByNickname(sender).NickName);
        pv.RPC(nameof(RPC_SendGuildInvite), Player.FindPlayerByNickname(sender), playerName, sender, viewId);
    }

    public void AcceptGuildInvite(string groupName)
    {
        var guildInfoReq = new GetGroupRequest() { GroupName = groupName };
        PlayFabGroupsAPI.GetGroup(guildInfoReq, 
            result => {
                var request = new AcceptGroupInvitationRequest { Group = result.Group };
                PlayFabGroupsAPI.AcceptGroupInvitation(request, response=> {
                    var getObjectsRequest = new PlayFab.DataModels.GetObjectsRequest()
                    {
                        Entity = new PlayFab.DataModels.EntityKey
                        {
                            Id = result.Group.Id,
                            Type = result.Group.Type
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
    void RPC_SendGuildInvite(string playerName, string sender, int viewID)
    {
        PhotonView refView = PhotonView.Find(viewID);
        if (refView.Owner.NickName == sender)
        {
            GameObject senderGO = refView.gameObject;
            string senderGroupId = senderGO.GetComponent<PlayerGuild>().groupId;
            string senderGroupName = senderGO.GetComponent<PlayerGuild>().groupName;

            Debug.Log(refView.Owner.NickName);
            Debug.LogError(senderGroupName);

            var guildInfoReq = new GetGroupRequest() { GroupName = senderGroupName };
            PlayFabGroupsAPI.GetGroup(guildInfoReq,
                resultGroup =>
                {
                    // A player-controlled entity invites another player-controlled entity to an existing group
                    //Entity Key is the player you want to invite
                    var invitedPlayerReq = new PlayFab.ClientModels.GetAccountInfoRequest() { Username = playerName };
                    PlayFabClientAPI.GetAccountInfo(invitedPlayerReq, result =>
                    {
                        var request = new InviteToGroupRequest { Group = resultGroup.Group, Entity = new EntityKey() { Id = result.AccountInfo.TitleInfo.TitlePlayerAccount.Id, Type = "title_player_account" } };
                        PlayFabGroupsAPI.InviteToGroup(request, response =>
                        {
                            refView.RPC(nameof(OpenGuildInvite), Player.FindPlayerByNickname(playerName), senderGroupName);
                        }, error => { Debug.Log(error.GenerateErrorReport()); }
                            );
                    }, error => { Debug.LogError(error.GenerateErrorReport()); });

                }, error => { Debug.LogError(error.GenerateErrorReport()); });
        }
    }

    [PunRPC]
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
